﻿/// StateInfo struct stores information needed to restore a Position object to
/// its previous state when we retract a move. Whenever a move is made on the
/// board (by calling Position::do_move), a StateInfo object must be passed.
internal class StateInfo
{
    internal PieceType capturedType;

    internal int castlingRights;

    internal Bitboard checkersBB;

    internal Square epSquare = Square.SQ_NONE;

    // Not copied when making a move
    internal ulong key;

    internal ulong materialKey;

    internal Value[] nonPawnMaterial = new Value[Color.COLOR_NB];

    // Copied when making a move
    internal ulong pawnKey;

    internal int pliesFromNull;

    internal StateInfo previous;

    internal Score psq;

    internal int rule50;

    internal void copyFrom(StateInfo other)
    {
        pawnKey = other.pawnKey;
        materialKey = other.materialKey;
        nonPawnMaterial[0] = other.nonPawnMaterial[0];
        nonPawnMaterial[1] = other.nonPawnMaterial[1];
        castlingRights = other.castlingRights;
        rule50 = other.rule50;
        pliesFromNull = other.pliesFromNull;
        psq = other.psq;
        epSquare = other.epSquare;
        key = other.key;
        checkersBB = other.checkersBB;
        capturedType = other.capturedType;
    }
};