// EasyMoveManager struct is used to detect a so called 'easy move'; when PV is
// stable across multiple search iterations we can fast return the best move.

using System.Collections.Generic;
using System.Diagnostics;

public class EasyMoveManager
{
    private ulong expectedPosKey;
    private readonly Move[] pv = new Move[3];
    private int stableCnt;

    private void clear()
    {
        stableCnt = 0;
        expectedPosKey = 0;
        pv[0] = pv[1] = pv[2] = Move.MOVE_NONE;
    }

    private Move get(ulong key)
    {
        return expectedPosKey == key ? pv[2] : Move.MOVE_NONE;
    }

    private void update(Position pos, List<Move> newPv)
    {
        Debug.Assert(newPv.Count >= 3);

        // Keep track of how many times in a row 3rd ply remains stable
        stableCnt = (newPv[2] == pv[2]) ? stableCnt + 1 : 0;

        if (pv[0] != newPv[0] || pv[1] != newPv[1] || pv[2] != newPv[2])
        {
            pv[0] = newPv[0];
            pv[1] = newPv[1];
            pv[2] = newPv[2];
            
            var st = new StateInfo[2];
            pos.do_move(newPv[0], st[0], pos.gives_check(newPv[0], new CheckInfo(pos)));
            pos.do_move(newPv[1], st[1], pos.gives_check(newPv[1], new CheckInfo(pos)));
            expectedPosKey = pos.key();
            pos.undo_move(newPv[1]);
            pos.undo_move(newPv[0]);
        }
    }
};