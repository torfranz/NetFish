using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

public sealed class SplitPoint
{
    // Shared variable data
    internal readonly object spinlock = new object();

    internal volatile bool allSlavesSearching;

    internal volatile int alpha;

    internal volatile int bestMove;

    internal volatile int bestValue;

    internal Value beta;

    public bool cutNode;

    internal volatile bool cutoff;

    internal Depth depth;

    internal Thread master;

    internal volatile int moveCount;

    // Const pointers to shared data
    internal MovePicker movePicker;

    internal volatile int nodes;

    internal NodeType nodeType;

    internal SplitPoint parentSplitPoint;

    // Const data after splitPoint has been setup
    internal Position pos;

    internal ulong slavesMask;

    internal StackArrayWrapper ss;
};

/*
class LimitedSizeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    Queue<TKey> queue;
    int size;

    public LimitedSizeDictionary(int size) 
        : base(size + 1)
    {
        this.size = size;
        queue = new Queue<TKey>(size);
    }

    public void Add(TKey key, TValue value)
    {
        base.Add(key, value);
        if (queue.Count == size)
            base.Remove(queue.Dequeue());
        queue.Enqueue(key);
    }

    public bool Remove(TKey key)
    {
        if (base.Remove(key))
        {
            Queue<TKey> newQueue = new Queue<TKey>(size);
            foreach (TKey item in queue)
                if (!base.Comparer.Equals(item, key))
                    newQueue.Enqueue(item);
            queue = newQueue;
            return true;
        }
        else
            return false;
    }
}
*/

/// ThreadBase struct is the base of the hierarchy from where we derive all the
/// specialized thread classes.
public abstract class ThreadBase
{
    public abstract void idle_loop(ManualResetEvent initEvent);

    internal void StartThread(object state)
    {
        var initEvent = (ManualResetEvent)state;
        this.idle_loop(initEvent);
    }

    protected ThreadBase(ManualResetEvent initEvent)
    {
        System.Threading.ThreadPool.QueueUserWorkItem(this.StartThread, initEvent);
    }

    // ThreadBase::notify_one() wakes up the thread when there is some work to do
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public void notify_one()
    {
        ThreadHelper.lock_grab(this.spinlock);
        ThreadHelper.cond_signal(this.sleepCondition);
        ThreadHelper.lock_release(this.spinlock);
    }

    //public Mutex mutex = new Mutex(true);

    public readonly object spinlock = new object();

    public readonly object sleepCondition = new object();

    public volatile bool exit;
};

/// Thread struct keeps together all the thread related stuff like locks, state
/// and especially split points. We also use per-thread pawn and material hash
/// tables so that once we get a pointer to an entry its life time is unlimited
/// and we don't have to care about someone changing the entry under our feet.
public class Thread : ThreadBase
{
    private readonly int idx;

    internal readonly SplitPoint[] splitPoints = new SplitPoint[_.MAX_SPLITPOINTS_PER_THREAD];

    public Position activePosition;

    public volatile SplitPoint activeSplitPoint;

    public Endgames endgames = new Endgames();

    public Dictionary<ulong, MaterialEntry> materialTable = new Dictionary<ulong, MaterialEntry>(8192);

    public int maxPly;

    public Dictionary<ulong, Pawns.Entry> pawnsTable = new Dictionary<ulong, Pawns.Entry>(16384);

    protected volatile bool searching;

    public volatile int splitPointsSize;

    internal Thread(ManualResetEvent initEvent)
        : base(initEvent)
    {
        this.searching = false;
        this.maxPly = 0;
        this.splitPointsSize = 0;
        this.activeSplitPoint = null;
        this.activePosition = null;
        this.idx = ThreadPool.threads.Count; // Starts from 0
        for (var j = 0; j < _.MAX_SPLITPOINTS_PER_THREAD; j++)
        {
            this.splitPoints[j] = new SplitPoint();
        }
    }

    public override void idle_loop(ManualResetEvent initEvent)
    {
        if (initEvent != null)
        {
            // Signal done
            initEvent.Set();
        }

        this.base_idle_loop(initEvent);
    }

    public void base_idle_loop(ManualResetEvent initEvent)
    {
        // Pointer 'this_sp' is not null only if we are called from split(), and not
        // at the thread creation. This means we are the split point's master.
        var this_sp = this.splitPointsSize > 0 ? this.activeSplitPoint : null;
        Debug.Assert(this_sp == null || (this_sp.master == this && this.searching));

        while (!this.exit && !(this_sp != null) && (this_sp.slavesMask == 0))
        {
            // If this thread has been assigned work, launch a search
            while (this.searching)
            {
                ThreadHelper.lock_grab(this.spinlock);

                Debug.Assert(this.activeSplitPoint != null);
                var sp = this.activeSplitPoint;

                ThreadHelper.lock_release(this.spinlock);

                var stack = new StackArrayWrapper(new Stack[_.MAX_PLY + 4]);
                var ss = new StackArrayWrapper(stack.table, 2);
                var pos = new Position(sp.pos, this);

                Array.Copy(sp.ss.table, ss.table, 5);
                ss[ss.current].splitPoint = sp;

                ThreadHelper.lock_grab(sp.spinlock);

                Debug.Assert(this.activePosition == null);

                this.activePosition = pos;

                if (sp.nodeType == NodeType.NonPV)
                {
                    //enable call to search
                    //search < NonPV, true > (pos, ss, sp->alpha, sp->beta, sp->depth, sp->cutNode)
                }

                else if (sp.nodeType == NodeType.PV)
                {
                    //enable call to search
                    //search < PV, true > (pos, ss, sp->alpha, sp->beta, sp->depth, sp->cutNode)
                }

                else if (sp.nodeType == NodeType.Root)
                {
                    //enable call to search
                    //search < Root, true > (pos, ss, sp->alpha, sp->beta, sp->depth, sp->cutNode);
                }

                else
                {
                    Debug.Assert(false);
                }

                Debug.Assert(this.searching);

                ThreadHelper.lock_grab(this.spinlock);

                this.searching = false;
                this.activePosition = null;

                ThreadHelper.lock_release(this.spinlock);

                sp.slavesMask &= ~(1UL << this.idx); //sp.slavesMask.reset(idx);
                sp.allSlavesSearching = false;
                sp.nodes += pos.nodes_searched();

                // After releasing the lock we can't access any SplitPoint related data
                // in a safe way because it could have been released under our feet by
                // the sp master.
                ThreadHelper.lock_release(sp.spinlock);

                // Try to late join to another split point if none of its slaves has
                // already finished.
                SplitPoint bestSp = null;
                var minLevel = int.MaxValue;

                foreach (var th in ThreadPool.threads)
                {
                    var size = th.splitPointsSize; // Local copy
                    sp = size > 0 ? th.splitPoints[size - 1] : null;

                    if (sp != null && sp.allSlavesSearching
                        && Bitcount.popcount_Full(sp.slavesMask) < _.MAX_SLAVES_PER_SPLITPOINT && this.can_join(sp))
                    {
                        Debug.Assert(this != th);
                        Debug.Assert(!(this_sp != null && Bitcount.popcount_Full(sp.slavesMask) == 0));
                        Debug.Assert(ThreadPool.threads.Count > 2);

                        // Prefer to join to SP with few parents to reduce the probability
                        // that a cut-off occurs above us, and hence we waste our work.
                        var level = 0;
                        for (var p = th.activeSplitPoint; p != null; p = p.parentSplitPoint)
                        {
                            level++;
                        }

                        if (level < minLevel)
                        {
                            bestSp = sp;
                            minLevel = level;
                        }
                    }
                }

                if (bestSp != null)
                {
                    sp = bestSp;

                    // Recheck the conditions under lock protection
                    ThreadHelper.lock_grab(sp.spinlock);

                    if (sp.allSlavesSearching && Bitcount.popcount_Full(sp.slavesMask) < _.MAX_SLAVES_PER_SPLITPOINT)
                    {
                        ThreadHelper.lock_grab(this.spinlock);

                        if (this.can_join(sp))
                        {
                            sp.slavesMask &= ~(1UL << this.idx); //sp->slavesMask.set(idx);
                            this.activeSplitPoint = sp;
                            this.searching = true;
                        }

                        ThreadHelper.lock_release(this.spinlock);
                    }

                    ThreadHelper.lock_release(sp.spinlock);
                }

                // If search is finished then sleep, otherwise just yield
                if (!ThreadPool.main().thinking)
                {
                    Debug.Assert(this_sp == null);

                    ThreadHelper.cond_wait(this.sleepCondition, this.spinlock/*mutex*/);
                }
                else
                {
                    System.Threading.Thread.Yield(); // Wait for a new job or for our slaves to finish
                }
            }
        }
    }

    // Thread::cutoff_occurred() checks whether a beta cutoff has occurred in the
    // current active split point, or in some ancestor of the split point.

    public bool cutoff_occurred()
    {
        for (var sp = this.activeSplitPoint; sp != null; sp = sp.parentSplitPoint)
        {
            if (sp.cutoff)
            {
                return true;
            }
        }

        return false;
    }

    // Make a local copy to be sure doesn't become zero under our feet while
    // Thread::can_join() checks whether the thread is available to join the split
    // point 'sp'. An obvious requirement is that thread must be idle. With more than
    // two threads, this is not sufficient: If the thread is the master of some split
    // point, it is only available as a slave for the split points below his active
    // one (the "helpful master" concept in YBWC terminology).

    public bool can_join(SplitPoint sp)
    {
        if (this.searching)
        {
            return false;
        }

        // Make a local copy to be sure it doesn't become zero under our feet while
        // testing next condition and so leading to an out of bounds access.
        var size = this.splitPointsSize;

        // No split points means that the thread is available as a slave for any
        // other thread otherwise apply the "helpful master" concept if possible.
        var bitIsSet = (this.splitPoints[size - 1].slavesMask & (1u << sp.master.idx)) != 0;
        //splitPoints[size - 1].slavesMask.test(sp.master.idx)
        return size > 0 || bitIsSet;
    }

    // Thread::split() does the actual work of distributing the work at a node between
    // several available threads. If it does not succeed in splitting the node
    // (because no idle threads are available), the function immediately returns.
    // If splitting is possible, a SplitPoint object is initialized with all the
    // data that must be copied to the helper threads and then helper threads are
    // informed that they have been assigned work. This will cause them to instantly
    // leave their idle loops and call search(). When all threads have returned from
    // search() then split() returns.

    public void split(
        Position pos,
        StackArrayWrapper ss,
        Value alpha,
        Value beta,
        ref Value bestValue,
        ref Move bestMove,
        Depth depth,
        int moveCount,
        MovePicker movePicker,
        NodeType nodeType,
        bool cutNode)
    {
        Debug.Assert(this.searching);
        Debug.Assert(
            -Value.VALUE_INFINITE < bestValue && bestValue <= alpha && alpha < beta && beta <= Value.VALUE_INFINITE);
        Debug.Assert(depth >= ThreadPool.minimumSplitDepth);
        Debug.Assert(this.splitPointsSize < _.MAX_SPLITPOINTS_PER_THREAD);

        // Pick and init the next available split point
        var sp = this.splitPoints[this.splitPointsSize];

        ThreadHelper.lock_grab(sp.spinlock); // No contention here until we don't increment splitPointsSize

        sp.master = this;
        sp.parentSplitPoint = this.activeSplitPoint;
        sp.slavesMask = 0;
        sp.slavesMask = (1u << this.idx);
        sp.depth = depth;
        sp.bestValue = bestValue;
        sp.bestMove = bestMove;
        sp.alpha = alpha;
        sp.beta = beta;
        sp.nodeType = nodeType;
        sp.cutNode = cutNode;
        sp.movePicker = movePicker;
        sp.moveCount = moveCount;
        sp.pos = pos;
        sp.nodes = 0;
        sp.cutoff = false;
        sp.ss = ss;
        sp.allSlavesSearching = true; // Must be set under lock protection

        ++this.splitPointsSize;
        this.activeSplitPoint = sp;
        this.activePosition = null;

        // Try to allocate available threads
        Thread slave = null;

        while (Bitcount.popcount_Full(sp.slavesMask) < _.MAX_SLAVES_PER_SPLITPOINT
               && (slave = ThreadPool.available_slave(sp)) != null)
        {
            ThreadHelper.lock_grab(slave.spinlock);

            if (slave.can_join(this.activeSplitPoint))
            {
                this.activeSplitPoint.slavesMask |= 1u << (slave.idx);
                slave.activeSplitPoint = this.activeSplitPoint;
                slave.searching = true;
            }

            ThreadHelper.lock_release(slave.spinlock);
        }

        // Everything is set up. The master thread enters the idle loop, from which
        // it will instantly launch a search, because its 'searching' flag is set.
        // The thread will return from the idle loop when all slaves have finished
        // their work at this split point.
        ThreadHelper.lock_release(sp.spinlock);

        this.base_idle_loop(null); // Force a call to base class idle_loop()

        // In the helpful master concept, a master can help only a sub-tree of its
        // split point and because everything is finished here, it's not possible
        // for the master to be booked.
        Debug.Assert(!this.searching);
        Debug.Assert(this.activePosition == null);

        // We have returned from the idle loop, which means that all threads are
        // finished. Note that decreasing splitPointsSize must be done under lock
        // protection to avoid a race with Thread::can_join().
        ThreadHelper.lock_grab(this.spinlock);

        this.searching = true;
        --this.splitPointsSize;
        this.activeSplitPoint = sp.parentSplitPoint;
        this.activePosition = pos;

        ThreadHelper.lock_release(this.spinlock);

        // Split point data cannot be changed now, so no need to lock protect
        pos.set_nodes_searched(pos.nodes_searched() + sp.nodes);
        bestMove = new Move(sp.bestMove);
        bestValue = new Value(sp.bestValue);
    }
}

// MainThread and TimerThread are sublassed from Thread to charaterize the two
// special threads: the main one and the recurring timer.

internal sealed class TimerThread : ThreadBase
{
    public const int Resolution = 5; // Millisec between two check_time() calls

    public bool run = false;

    internal TimerThread(ManualResetEvent initEvent)
        : base(initEvent)
    {
    }

    // Thread::timer_loop() is where the timer thread waits maxPly milliseconds and
    // then calls do_timer_event(). If maxPly is 0 thread sleeps until is woken up.
    public override void idle_loop(ManualResetEvent initEvent)
    {
        // Signal done
        initEvent.Set();

        while (!this.exit)
        {
            ThreadHelper.lock_grab(this.spinlock/*mutex*/);
            if (!this.exit)
            {
                ThreadHelper.cond_timedwait(this.sleepCondition, this.spinlock/*mutex*/, this.run ? Resolution : int.MaxValue);
            }

            ThreadHelper.lock_release(this.spinlock/*mutex*/);

            if (this.run)
            {
                Search.check_time();
            }
        }
    }
}

public sealed class MainThread : Thread
{
    internal volatile bool thinking = true; // Avoid a race with start_thinking()

    public MainThread(ManualResetEvent initEvent)
        : base(initEvent)
    {
    }

    // MainThread::idle_loop() is where the main thread is parked waiting to be started
    // when there is a new search. The main thread will launch all the slave threads.

    public override void idle_loop(ManualResetEvent initEvent)
    {
        if (initEvent != null)
        {
            // Signal done
            initEvent.Set();
        }

        while (!this.exit)
        {
            ThreadHelper.lock_grab(this.spinlock/*mutex*/);

            this.thinking = false;

            while (!this.thinking && !this.exit)
            {
                //TODO: correct replacement for sleepCondition.notify_one();?
                ThreadHelper.cond_signal(this.sleepCondition); // Wake up the UI thread if needed, 
                ThreadHelper.cond_wait(this.sleepCondition, this.spinlock/*mutex*/);
            }

            ThreadHelper.lock_release(this.spinlock/*mutex*/);

            if (!this.exit)
            {
                this.searching = true;

                Search.think();

                Debug.Assert(this.searching);

                this.searching = false;
            }
        }
    }

    // MainThread::join() waits for main thread to finish the search
    public void join()
    {
        ThreadHelper.lock_grab(this.spinlock/*mutex*/);

        //sleepCondition.wait(lk, [&]{ return !thinking; });
        while (this.thinking)
        {
            ThreadHelper.cond_wait(this.sleepCondition, this.spinlock/*mutex*/);
        }

        ThreadHelper.lock_release(this.spinlock/*mutex*/);
    }
}

/// ThreadPool struct handles all the threads related stuff like init, starting,
/// parking and, most importantly, launching a slave thread at a split point.
/// All the access to shared thread data is done through this class.
public static class ThreadPool
{
    /* As long as the single ThreadsManager object is defined as a global we don't
       need to explicitly initialize to zero its data members because variables with
       static storage duration are automatically set to zero before enter main()
    */

    internal static readonly List<Thread> threads = new List<Thread>();

    internal static TimerThread timer;

    public static Depth minimumSplitDepth;

    public static MainThread main()
    {
        return (MainThread)threads[0];
    }

    // ThreadPool::read_uci_options() updates internal threads parameters from the
    // corresponding UCI options and creates/destroys threads to match the requested
    // number. Thread objects are dynamically allocated to avoid creating all possible
    // threads in advance (which include pawns and material tables), even if only a
    // few are to be used.
    internal static void read_uci_options(ManualResetEvent[] initEvents)
    {
        minimumSplitDepth = int.Parse(OptionMap.Instance["Min Split Depth"].v) * Depth.ONE_PLY;

        var requested = int.Parse(OptionMap.Instance["Threads"].v);
        var current = 0;

        Debug.Assert(requested > 0);

        while (threads.Count < requested)
        {
            if (initEvents == null)
            {
                threads.Add(new Thread(null));
            }
            else
            {
                threads.Add(new Thread(initEvents[current + 2]));
                current++;
            }
        }

        while (threads.Count > requested)
        {
            delete_thread(threads[threads.Count - 1]);
            threads.RemoveAt(threads.Count - 1);
        }
    }

    private static void delete_thread(ThreadBase th)
    {
        ThreadHelper.lock_grab(th.spinlock/*mutex*/);

        th.exit = true; // Search must be already finished
        ThreadHelper.lock_release(th.spinlock/*mutex*/);

        th.notify_one();

        //TODO: is call needed?
        //th.join(); // Wait for thread termination
    }

    // ThreadPool::init() is called at startup to create and launch requested threads,
    // that will go immediately to sleep. We cannot use a c'tor because Threads is a
    // static object and we need a fully initialized engine at this point due to
    // allocation of Endgames in Thread c'tor.
    public static void init()
    {
        var requested = int.Parse(OptionMap.Instance["Threads"].v);
        var initEvents = new ManualResetEvent[requested + 1];
        for (var i = 0; i < (requested + 1); i++)
        {
            initEvents[i] = new ManualResetEvent(false);
        }

        System.Threading.ThreadPool.QueueUserWorkItem(launch_threads, initEvents);
        WaitHandle.WaitAll(initEvents);
    }

    private static void launch_threads(object state)
    {
        var initEvents = (ManualResetEvent[])state;
        timer = new TimerThread(initEvents[0]);
        threads.Add(new MainThread(initEvents[1]));
        read_uci_options(initEvents);
    }

    // ThreadPool::exit() terminates the threads before the program exits. Cannot be
    // done in d'tor because threads must be terminated before freeing us.
    internal static void exit()
    {
        delete_thread(timer); // As first because check_time() accesses threads data
        timer = null;

        for (var i = 0; i < threads.Count; i++)
        {
            delete_thread(threads[i]);
        }
        threads.Clear();
    }

    // ThreadPool::available_slave() tries to find an idle thread which is available
    // to join SplitPoint 'sp'.
    internal static Thread available_slave(SplitPoint sp)
    {
        for (var i = 0; i < threads.Count; i++)
        {
            if (threads[i].can_join(sp))
            {
                return threads[i];
            }
        }

        return null;
    }

    // ThreadPool::start_thinking() wakes up the main thread sleeping in
    // MainThread::idle_loop() and starts a new search, then returns immediately.

    internal static void start_thinking(Position pos, LimitsType limits, StateInfoWrapper states)
    {
        main().join();

        Search.Signals.stopOnPonderhit = Search.Signals.firstRootMove = false;
        Search.Signals.stop = Search.Signals.failedLowAtRoot = false;

        Search.RootMoves.Clear();
        Search.RootPos = new Position(pos);
        Search.Limits = limits;

        var current = states[states.current];
        if (current != null) // If we don't set a new position, preserve current state
        {
            Search.SetupStates = states; // Ownership transfer here
            Debug.Assert(current != null);
        }

        var ml = new MoveList(GenType.LEGAL, pos);
        for (var index = ml.begin(); index < ml.end(); index++)
        {
            var m = ml.moveList.table[index];
            if (limits.searchmoves.Count == 0 || limits.searchmoves.FindAll(move => move == m.Move).Count == 0)
            {
                Search.RootMoves.Add(new RootMove(m));
            }
        }

        main().thinking = true;
        main().notify_one(); // Wake up main thread: 'thinking' must be already set
    }

    // ThreadsManager::wait_for_think_finished() waits for main thread to go to
    // sleep, this means search is finished. Then returns.
    internal static void wait_for_think_finished()
    {
        var t = main();
        ThreadHelper.lock_grab(t.spinlock);
        ThreadHelper.cond_signal(t.sleepCondition); // In case is waiting for stop or ponderhit
        while (t.thinking)
        {
            ThreadHelper.cond_wait(t.sleepCondition, t.spinlock);
        }
        ThreadHelper.lock_release(t.spinlock);
    }
}

internal static class ThreadHelper
{
    //#  define lock_grab(x) EnterCriticalSection(x)
#if FORCEINLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static void lock_grab(object Lock)
    {
        Monitor.Enter(Lock);
    }

    //#  define lock_release(x) LeaveCriticalSection(x)
#if FORCEINLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static void lock_release(object Lock)
    {
        Monitor.Exit(Lock);
    }

    //#  define cond_signal(x) SetEvent(*x)
#if FORCEINLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static void cond_signal(object sleepCond)
    {
        lock (sleepCond)
        {
            Monitor.Pulse(sleepCond);
        }
    }

    //#  define cond_wait(x,y) { lock_release(y); WaitForSingleObject(*x, INFINITE); lock_grab(y); }
#if FORCEINLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static void cond_wait(object sleepCond, object sleepLock)
    {
        lock_release(sleepLock);
        lock (sleepCond)
        {
            Monitor.Wait(sleepCond);
        }
        lock_grab(sleepLock);
    }

    //#  define cond_timedwait(x,y,z) { lock_release(y); WaitForSingleObject(*x,z); lock_grab(y); }
#if FORCEINLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static void cond_timedwait(object sleepCond, object sleepLock, int msec)
    {
        lock_release(sleepLock);
        lock (sleepCond)
        {
            Monitor.Wait(sleepCond, msec);
        }
        lock_grab(sleepLock);
    }
}