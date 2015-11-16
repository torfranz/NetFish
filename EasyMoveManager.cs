// EasyMoveManager struct is used to detect a so called 'easy move'; when PV is
// stable across multiple search iterations we can fast return the best move.

using System.Collections.Generic;
using System.Diagnostics;

#if PRIMITIVE
using MoveT = System.Int32;
#endif

internal class EasyMoveManager
{
    private readonly MoveT[] pv = new MoveT[3];

    private ulong expectedPosKey;

    internal int stableCnt;

    internal void clear()
    {
        stableCnt = 0;
        expectedPosKey = 0;
        pv[0] = pv[1] = pv[2] = Move.MOVE_NONE;
    }

    internal MoveT get(ulong key)
    {
        return expectedPosKey == key ? pv[2] : Move.MOVE_NONE;
    }

    internal void update(Position pos, List<MoveT> newPv)
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