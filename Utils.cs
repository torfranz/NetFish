using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#if PRIMITIVE
using FileType = System.Int32;
using RankType = System.Int32;
using ColorType = System.Int32;
using PieceTypeType = System.Int32;
#endif

internal static class Utils
{
    internal delegate uint Fn(Square s, Bitboard occ);

    // De Bruijn sequences. See chessprogramming.wikispaces.com/BitScan
    private const ulong DeBruijn64 = 0x3F79D71B4CB0A89UL;

    private const ulong DeBruijn32 = 0x783A9B23;

    internal static int[,] SquareDistance = new int[Square.SQUARE_NB_C, Square.SQUARE_NB_C];

    internal static Bitboard[] RookMasks = new Bitboard[Square.SQUARE_NB_C];

    internal static Bitboard[] RookMagics = new Bitboard[Square.SQUARE_NB_C];

    internal static Bitboard[][] RookAttacks = new Bitboard[Square.SQUARE_NB_C][];

    internal static uint[] RookShifts = new uint[Square.SQUARE_NB_C];

    internal static Bitboard[] BishopMasks = new Bitboard[Square.SQUARE_NB_C];

    internal static Bitboard[] BishopMagics = new Bitboard[Square.SQUARE_NB_C];

    internal static Bitboard[][] BishopAttacks = new Bitboard[Square.SQUARE_NB_C][];

    internal static uint[] BishopShifts = new uint[Square.SQUARE_NB_C];

    internal static Bitboard[] SquareBB = new Bitboard[Square.SQUARE_NB_C];

    internal static Bitboard[] FileBB = new Bitboard[File.FILE_NB];

    internal static Bitboard[] AdjacentFilesBB = new Bitboard[File.FILE_NB];

    internal static Bitboard[] RankBB = new Bitboard[Rank.RANK_NB];

    internal static Bitboard[,] InFrontBB = new Bitboard[Color.COLOR_NB, Rank.RANK_NB];

    internal static Bitboard[,] StepAttacksBB = new Bitboard[Piece.PIECE_NB_C, Square.SQUARE_NB_C];

    internal static Bitboard[,] BetweenBB = new Bitboard[Square.SQUARE_NB_C, Square.SQUARE_NB_C];

    internal static Bitboard[,] LineBB = new Bitboard[Square.SQUARE_NB_C, Square.SQUARE_NB_C];

    internal static Bitboard[,] DistanceRingBB = new Bitboard[Square.SQUARE_NB_C, 8];

    internal static Bitboard[,] ForwardBB = new Bitboard[Color.COLOR_NB, Square.SQUARE_NB_C];

    internal static Bitboard[,] PassedPawnMask = new Bitboard[Color.COLOR_NB, Square.SQUARE_NB_C];

    internal static Bitboard[,] PawnAttackSpan = new Bitboard[Color.COLOR_NB, Square.SQUARE_NB_C];

    internal static Bitboard[,] PseudoAttacks = new Bitboard[Piece.PIECE_NB_C, Square.SQUARE_NB_C];

    internal static int[] MSBTable = new int[256]; // To implement software msb()

    internal static Square[] BSFTable = new Square[Square.SQUARE_NB_C]; // To implement software bitscan

    private static bool firstLog = true;

    [Conditional("DEBUG")]
    internal static void WriteToLog(string s)
    {
        using (
            var sw = firstLog
                ? System.IO.File.CreateText("Logfile_Netfish.txt")
                : System.IO.File.AppendText("Logfile_Netfish.txt"))
        {
            firstLog = false;
            sw.WriteLine(s);
        }
    }

    /// rank_bb() and file_bb() return a bitboard representing all the squares on
    /// the given file or rank.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard rank_bb(RankType r)
    {
        return RankBB[r];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard rank_bb(Square s)
    {
        return RankBB[Square.rank_of(s)];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard file_bb(FileType f)
    {
        return FileBB[f];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard file_bb(Square s)
    {
        return FileBB[Square.file_of(s)];
    }

    /// adjacent_files_bb() returns a bitboard representing all the squares on the
    /// adjacent files of the given one.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard adjacent_files_bb(FileType f)
    {
        return AdjacentFilesBB[f];
    }

    /// between_bb() returns a bitboard representing all the squares between the two
    /// given ones. For instance, between_bb(SQ_C4, SQ_F7) returns a bitboard with
    /// the bits for square d5 and e6 set. If s1 and s2 are not on the same rank, file
    /// or diagonal, 0 is returned.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard between_bb(Square s1, Square s2)
    {
        return BetweenBB[s1, s2];
    }

    /// in_front_bb() returns a bitboard representing all the squares on all the ranks
    /// in front of the given one, from the point of view of the given color. For
    /// instance, in_front_bb(BLACK, RANK_3) will return the squares on ranks 1 and 2.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard in_front_bb(ColorType c, RankType r)
    {
        return InFrontBB[c, r];
    }

    /// forward_bb() returns a bitboard representing all the squares along the line
    /// in front of the given one, from the point of view of the given color:
    /// ForwardBB[c][s] = in_front_bb(c, s) & file_bb(s)
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard forward_bb(ColorType c, Square s)
    {
        return ForwardBB[c, s];
    }

    /// pawn_attack_span() returns a bitboard representing all the squares that can be
    /// attacked by a pawn of the given color when it moves along its file, starting
    /// from the given square:
    /// PawnAttackSpan[c][s] = in_front_bb(c, s) & adjacent_files_bb(s);
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard pawn_attack_span(ColorType c, Square s)
    {
        return PawnAttackSpan[c, s];
    }

    /// passed_pawn_mask() returns a bitboard mask which can be used to test if a
    /// pawn of the given color and on the given square is a passed pawn:
    /// PassedPawnMask[c][s] = pawn_attack_span(c, s) | forward_bb(c, s)
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard passed_pawn_mask(ColorType c, Square s)
    {
        return PassedPawnMask[c, s];
    }

    /// aligned() returns true if the squares s1, s2 and s3 are aligned either on a
    /// straight or on a diagonal line.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool aligned(Square s1, Square s2, Square s3)
    {
        return LineBB[s1, s2] & s3;
    }

    /// distance() functions return the distance between x and y, defined as the
    /// number of steps for a king in x to reach y. Works with squares, ranks, files.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static int distance_Square(Square x, Square y)
    {
        return SquareDistance[x, y];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static int distance_Rank(int x, int y)
    {

        return x < y ? y - x : x - y;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static int distance_Rank(Square x, Square y)
    {
        return distance_Rank(Square.rank_of(x), Square.rank_of(y));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static int distance_File(Square x, Square y)
    {
        int xFile = Square.file_of(x);
        int yFile = Square.file_of(y);
        return xFile > yFile ? xFile - yFile : yFile - xFile;
    }

    internal static uint magic_index_Rook(Square s, Bitboard occupied)
    {
        return magic_index(PieceType.ROOK, s, occupied);
    }

    internal static uint magic_index_Bishop(Square s, Bitboard occupied)
    {
        return magic_index(PieceType.BISHOP, s, occupied);
    }

    /// attacks_bb() returns a bitboard representing all the squares attacked by a
    /// piece of type Pt (bishop or rook) placed on 's'. The helper magic_index()
    /// looks up the index using the 'magic bitboards' approach.
    internal static uint magic_index(PieceTypeType Pt, Square s, Bitboard occupied)
    {
        var Masks = Pt == PieceType.ROOK ? RookMasks : BishopMasks;
        var Magics = Pt == PieceType.ROOK ? RookMagics : BishopMagics;
        var Shifts = Pt == PieceType.ROOK ? RookShifts : BishopShifts;

#if X64
        return (uint) ((((occupied & Masks[(int)s])*Magics[(int)s]) >> (int) Shifts[(int)s]));
#else

        var lo = (uint)((occupied & Masks[s]));
        var hi = (uint)((occupied >> 32) & (Masks[s] >> 32));
        return (lo * (uint)(Magics[s]) ^ hi * (uint)(Magics[s] >> 32)) >> (int)Shifts[s];
#endif
    }

    internal static Bitboard attacks_bb(PieceTypeType Pt, Square s, Bitboard occupied)
    {
        return Pt == PieceType.ROOK
            ? RookAttacks[s][magic_index(Pt, s, occupied)]
            : BishopAttacks[s][magic_index(Pt, s, occupied)];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard attacks_bb(Piece pc, Square s, Bitboard occupied)
    {
        switch ((int)Piece.type_of(pc))
        {
            case 3 /*PieceType.BISHOP*/:
                return attacks_bb(PieceType.BISHOP, s, occupied);
            case 4 /*PieceType.ROOK*/:
                return attacks_bb(PieceType.ROOK, s, occupied);
            case 5 /*PieceType.QUEEN*/:
                return attacks_bb(PieceType.BISHOP, s, occupied) | attacks_bb(PieceType.ROOK, s, occupied);
            default:
                return StepAttacksBB[pc, s];
        }
    }

    // bsf_index() returns the index into BSFTable[] to look up the bitscan. Uses
    // Matt Taylor's folding for 32 bit case, extended to 64 bit by Kim Walisch.

    internal static uint bsf_index(Bitboard b)
    {
        var value = (ulong)b;
        value ^= value - 1;
#if X64
        return (uint) ((value*DeBruijn64) >> 58);
#else
        return (uint)((value ^ (value >> 32)) * DeBruijn32) >> 26;
#endif
    }

    internal static Square lsb(Bitboard b)
    {
        return BSFTable[bsf_index(b)];
    }

    internal static Square msb(Bitboard b)
    {
        var value = (ulong)b;
        var result = 0;

        if (value > 0xFFFFFFFF)
        {
            value >>= 32;
            result = 32;
        }

        var b32 = (uint) value;

        if (b32 > 0xFFFF)
        {
            b32 >>= 16;
            result += 16;
        }

        if (b32 > 0xFF)
        {
            b32 >>= 8;
            result += 8;
        }

        return new Square(result + MSBTable[b32]);
    }

    /// pop_lsb() finds and clears the least significant bit in a non-zero bitboard
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square pop_lsb(ref Bitboard b)
    {
        var s = lsb(b);

        var value = (ulong)b;
        value &= value - 1;
        b = new Bitboard(value);
        return s;
    }

    /// frontmost_sq() and backmost_sq() return the square corresponding to the
    /// most/least advanced bit relative to the given color.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square frontmost_sq(ColorType c, Bitboard b)
    {
        return c == Color.WHITE ? msb(b) : lsb(b);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square backmost_sq(ColorType c, Bitboard b)
    {
        return c == Color.WHITE ? lsb(b) : msb(b);
    }

    /// engine_info() returns the full name of the current Stockfish version.
    /// This will be either "Portfish YYMMDD" (where YYMMDD is the date when
    /// the program was compiled) or "Portfish
    /// version number
    ///     ", depending
    ///     on whether Version is empty.
    internal static string engine_info(bool to_uci = false)
    {
#if X64
        const string cpu64 = " 64bit";
#else
        const string cpu64 = "";
#endif
        // Assembly and file version
        var assembly = Assembly.GetExecutingAssembly();
        Version fileVersion = null;
        var attribs = assembly.GetCustomAttributes(typeof (AssemblyFileVersionAttribute), false);
        if (attribs.Length > 0)
        {
            var fileVersionRaw = (AssemblyFileVersionAttribute) (attribs[0]);
            fileVersion = new Version(fileVersionRaw.Version);
        }

        // Extract version/build date
        var fullName = assembly.FullName;
        var vspos = fullName.IndexOf("Version=");
        var vepos = fullName.IndexOf(",", vspos);
        var versionRaw = fullName.Substring(vspos + 8, vepos - vspos - 8);
        var version = new Version(versionRaw);
        var buildDateTime =
            new DateTime(2000, 1, 1).Add(
                new TimeSpan(
                    TimeSpan.TicksPerDay*version.Build + // days since 1 January 2000
                    TimeSpan.TicksPerSecond*2*version.Revision));
        // seconds since midnight, (multiply by 2 to get original)

        // Get version info
        var versionInfo = buildDateTime.Year + buildDateTime.Month.ToString().PadLeft(2, '0')
                          + buildDateTime.Day.ToString().PadLeft(2, '0');
        if (fileVersion != null)
        {
            versionInfo = fileVersion.ToString();
        }

        // Create version
        var sb = new StringBuilder();
        sb.Append("Netfish ").Append(versionInfo).Append(cpu64);
        sb.Append(to_uci ? "\nid author " : " by ").Append("TF");
        return sb.ToString();
    }

    internal static void stable_sort(List<RootMove> data, int firstMove, int lastMove)
    {
        int p;

        for (p = firstMove + 1; p < lastMove; p++)
        {
            var tmp = data[p];
            int q;
            for (q = p; q != firstMove && data[q - 1].score < tmp.score; --q)
            {
                data[q] = data[q - 1];
            }
            data[q] = tmp;
        }
    }
}