internal static class Zobrist
{
    internal static ulong[,,] psq = new ulong[Color.COLOR_NB_C, PieceType.PIECE_TYPE_NB_C, Square.SQUARE_NB_C];

    internal static ulong[] enpassant = new ulong[FileConstants.FILE_NB];

    internal static ulong[] castling = new ulong[(int) CastlingRight.CASTLING_RIGHT_NB];

    internal static ulong side;

    internal static ulong exclusion;
}