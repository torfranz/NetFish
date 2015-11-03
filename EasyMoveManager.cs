// EasyMoveManager struct is used to detect a so called 'easy move'; when PV is
// stable across multiple search iterations we can fast return the best move.

using System.Collections.Generic;
using System.Diagnostics;

public class EasyMoveManager
{
    private readonly Move[] pv = new Move[3];

    private ulong expectedPosKey;

    public int stableCnt;

    public void clear()
    {
        this.stableCnt = 0;
        this.expectedPosKey = 0;
        this.pv[0] = this.pv[1] = this.pv[2] = Move.MOVE_NONE;
    }

    public Move get(ulong key)
    {
        return this.expectedPosKey == key ? this.pv[2] : Move.MOVE_NONE;
    }

    public void update(Position pos, List<Move> newPv)
    {
        Debug.Assert(newPv.Count >= 3);

        // Keep track of how many times in a row 3rd ply remains stable
        this.stableCnt = (newPv[2] == this.pv[2]) ? this.stableCnt + 1 : 0;

        if (this.pv[0] != newPv[0] || this.pv[1] != newPv[1] || this.pv[2] != newPv[2])
        {
            this.pv[0] = newPv[0];
            this.pv[1] = newPv[1];
            this.pv[2] = newPv[2];

            var st = new StateInfo[2];
            pos.do_move(newPv[0], st[0], pos.gives_check(newPv[0], new CheckInfo(pos)));
            pos.do_move(newPv[1], st[1], pos.gives_check(newPv[1], new CheckInfo(pos)));
            this.expectedPosKey = pos.key();
            pos.undo_move(newPv[1]);
            pos.undo_move(newPv[0]);
        }
    }
};