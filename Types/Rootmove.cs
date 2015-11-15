using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if PRIMITIVE
using ValueT = System.Int32;
#endif
/// RootMove struct is used for moves at the root of the tree. For each root move
/// we store a score and a PV (really a refutation in the case of moves which
/// fail low). Score is normally set at -VALUE_INFINITE for all non-pv moves.
internal class RootMove
{
    internal readonly List<Move> pv = new List<Move>();

    internal ValueT previousScore = -Value.VALUE_INFINITE;

    internal ValueT score = -Value.VALUE_INFINITE;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal RootMove(Move m)
    {
        pv.Add(m);
    }

    /// RootMove::insert_pv_in_tt() is called at the end of a search iteration, and
    /// inserts the PV back into the TT. This makes sure the old PV moves are searched
    /// first, even if the old TT entries have been overwritten.
    internal void insert_pv_in_tt(Position pos)
    {
        var st = new StateInfoWrapper();

        foreach (var m in pv)
        {
            Debug.Assert(new MoveList(GenType.LEGAL, pos).contains(m));

            bool ttHit;
            var tte = TranspositionTable.probe(pos.key(), out ttHit);

            if (!ttHit || tte.move() != m) // Don't overwrite correct entries
            {
                tte.save(
                    pos.key(),
                    Value.VALUE_NONE,
                    Bound.BOUND_NONE,
                    Depth.DEPTH_NONE,
                    m,
                    Value.VALUE_NONE,
                    TranspositionTable.generation());
            }

            var current = st[st.current];
            st++;
            pos.do_move(m, current, pos.gives_check(m, new CheckInfo(pos)));
        }

        for (var i = pv.Count; i > 0;)
        {
            pos.undo_move(pv[--i]);
        }
    }

    /// RootMove::extract_ponder_from_tt() is called in case we have no ponder move before
    /// exiting the search, for instance in case we stop the search during a fail high at
    /// root. We try hard to have a ponder move to return to the GUI, otherwise in case of
    /// 'ponder on' we have nothing to think on.
    internal bool extract_ponder_from_tt(Position pos)
    {
        var st = new StateInfo();
        bool ttHit;

        Debug.Assert(pv.Count == 1);

        pos.do_move(pv[0], st, pos.gives_check(pv[0], new CheckInfo(pos)));
        var tte = TranspositionTable.probe(pos.key(), out ttHit);
        pos.undo_move(pv[0]);

        if (ttHit)
        {
            var m = tte.move(); // Local copy to be SMP safe
            if (new MoveList(GenType.LEGAL, pos).contains(m))
            {
                pv.Add(m);
                return true;
            }
        }

        return false;
    }
};