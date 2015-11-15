// Struct EvalInfo contains various information computed and collected
// by the evaluation functions.

internal class EvalInfo
{
    // attackedBy[color][piece type] is a bitboard representing all squares
    // attacked by a given color and piece type (can be also ALL_PIECES).
    internal Bitboard[,] attackedBy = new Bitboard[Color.COLOR_NB, PieceType.PIECE_TYPE_NB];

    // kingAdjacentZoneAttacksCount[color] is the number of attacks by the given
    // color to squares directly adjacent to the enemy king. Pieces which attack
    // more than one square are counted multiple times. For instance, if there is
    // a white knight on g5 and black's king is on g8, this white knight adds 2
    // to kingAdjacentZoneAttacksCount[WHITE].
    internal int[] kingAdjacentZoneAttacksCount = new int[Color.COLOR_NB];

    // kingAttackersCount[color] is the number of pieces of the given color
    // which attack a square in the kingRing of the enemy king.
    internal int[] kingAttackersCount = new int[Color.COLOR_NB];

    // kingAttackersWeight[color] is the sum of the "weight" of the pieces of the
    // given color which attack a square in the kingRing of the enemy king. The
    // weights of the individual piece types are given by the elements in the
    // KingAttackWeights array.
    internal int[] kingAttackersWeight = new int[Color.COLOR_NB];

    // kingRing[color] is the zone around the king which is considered
    // by the king safety evaluation. This consists of the squares directly
    // adjacent to the king, and the three (or two, for a king on an edge file)
    // squares two ranks in front of the king. For instance, if black's king
    // is on g8, kingRing[BLACK] is a bitboard containing the squares f8, h8,
    // f7, g7, h7, f6, g6 and h6.
    internal Bitboard[] kingRing = new Bitboard[Color.COLOR_NB];

    internal Pawns.Entry pi;

    internal Bitboard[] pinnedPieces = new Bitboard[Color.COLOR_NB];
};