public static class Zobrist
{
    public static ulong[,,] psq = new ulong[Color.COLOR_NB, PieceType.PIECE_TYPE_NB, Square.SQUARE_NB];
    public static ulong[] enpassant = new ulong[File.FILE_NB];
    public static ulong[] castling = new ulong[(int) CastlingRight.CASTLING_RIGHT_NB];
    public static ulong side;
    public static ulong exclusion;
}