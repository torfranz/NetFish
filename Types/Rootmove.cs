using System.Collections.Generic;
using System.Diagnostics;

/// RootMove struct is used for moves at the root of the tree. For each root move
/// we store a score and a PV (really a refutation in the case of moves which
/// fail low). Score is normally set at -VALUE_INFINITE for all non-pv moves.
public class RootMove
{
    public readonly List<Move> pv = new List<Move>();

    public Value score = -Value.VALUE_INFINITE;

    public Value previousScore = -Value.VALUE_INFINITE;

    public RootMove(Move m)
    {
        this.pv.Add(m);
    }

    public static bool operator <(RootMove m1, RootMove m2)
    {
        return m1.score > m2.score;
    } // Ascending sort

    public static bool operator >(RootMove m1, RootMove m2)
    {
        return m1.score < m2.score;
    } // Ascending sort

    public static bool operator ==(RootMove m1, Move m)
    {
        return m1.pv[0] == m;
    }

    public static bool operator !=(RootMove m1, Move m)
    {
        return m1.pv[0] != m;
    }

    /// RootMove::insert_pv_in_tt() is called at the end of a search iteration, and
    /// inserts the PV back into the TT. This makes sure the old PV moves are searched
    /// first, even if the old TT entries have been overwritten.
    public void insert_pv_in_tt(Position pos)
    {
        var st = new StateInfoWrapper(new StateInfo[_.MAX_PLY]);
        var ttHit = false;

        foreach (var m in this.pv)
        {
            Debug.Assert(new MoveList(GenType.LEGAL, pos).contains(m));

            var tte = TranspositionTable.probe(pos.key(), ref ttHit);

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

        for (var i = this.pv.Count; i > 0;)
        {
            pos.undo_move(this.pv[--i]);
        }
    }

    /// RootMove::extract_ponder_from_tt() is called in case we have no ponder move before
    /// exiting the search, for instance in case we stop the search during a fail high at
    /// root. We try hard to have a ponder move to return to the GUI, otherwise in case of
    /// 'ponder on' we have nothing to think on.
    public bool extract_ponder_from_tt(Position pos)
    {
        var st = new StateInfo();
        var ttHit = false;

        Debug.Assert(this.pv.Count == 1);

        pos.do_move(this.pv[0], st, pos.gives_check(this.pv[0], new CheckInfo(pos)));
        var tte = TranspositionTable.probe(pos.key(), ref ttHit);
        pos.undo_move(this.pv[0]);

        if (ttHit)
        {
            var m = tte.move(); // Local copy to be SMP safe
            if (new MoveList(GenType.LEGAL, pos).contains(m))
            {
                this.pv.Add(m);
                return true;
            }
        }

        return false;
    }
};