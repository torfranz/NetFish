using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// StateInfo struct stores information needed to restore a Position object to
/// its previous state when we retract a move. Whenever a move is made on the
/// board (by calling Position::do_move), a StateInfo object must be passed.

public class StateInfo
{
    // Copied when making a move
    public ulong pawnKey;
    public ulong materialKey;
    public Value[] nonPawnMaterial = new Value[Color.COLOR_NB];
    public int castlingRights;
    public int rule50;
    public int pliesFromNull;
    public Score psq;
    public Square epSquare;

    // Not copied when making a move
    public ulong key;
    public Bitboard checkersBB;
    public PieceType capturedType;
    public StateInfo previous;
};
