﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

static class Utils
{
    public static int[,] SquareDistance = new int[Square.SQUARE_NB, Square.SQUARE_NB];

    public static Bitboard[] RookMasks = new Bitboard[Square.SQUARE_NB];
    public static Bitboard[] RookMagics = new Bitboard[Square.SQUARE_NB];
    public static List<Bitboard[]> RookAttacks = new List<Bitboard[]>();
    public static uint[] RookShifts = new uint[Square.SQUARE_NB];

    public static Bitboard[] BishopMasks = new Bitboard[Square.SQUARE_NB];
    public static Bitboard[] BishopMagics = new Bitboard[Square.SQUARE_NB];
    public static List<Bitboard[]> BishopAttacks = new List<Bitboard[]>();
    public static uint[] BishopShifts = new uint[Square.SQUARE_NB];

    public static Bitboard[] SquareBB = new Bitboard[Square.SQUARE_NB];
    public static Bitboard[] FileBB = new Bitboard[File.FILE_NB];
    public static Bitboard[] AdjacentFilesBB = new Bitboard[File.FILE_NB];
    public static Bitboard[] RankBB = new Bitboard[Rank.RANK_NB];

    public static Bitboard[,] InFrontBB = new Bitboard[Color.COLOR_NB, Rank.RANK_NB];
    public static Bitboard[,] StepAttacksBB = new Bitboard[Piece.PIECE_NB, Square.SQUARE_NB];
    public static Bitboard[,] BetweenBB = new Bitboard[Square.SQUARE_NB, Square.SQUARE_NB];
    public static Bitboard[,] LineBB = new Bitboard[Square.SQUARE_NB, Square.SQUARE_NB];
    public static Bitboard[,] DistanceRingBB = new Bitboard[Square.SQUARE_NB, 8];
    public static Bitboard[,] ForwardBB = new Bitboard[Color.COLOR_NB, Square.SQUARE_NB];
    public static Bitboard[,] PassedPawnMask = new Bitboard[Color.COLOR_NB, Square.SQUARE_NB];
    public static Bitboard[,] PawnAttackSpan = new Bitboard[Color.COLOR_NB, Square.SQUARE_NB];
    public static Bitboard[,] PseudoAttacks = new Bitboard[Piece.PIECE_NB, Square.SQUARE_NB];

    /// rank_bb() and file_bb() return a bitboard representing all the squares on
    /// the given file or rank.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard rank_bb(Rank r)
    {
        return new Bitboard(RankBB[r.Value]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard rank_bb(Square s)
    {
        return new Bitboard(RankBB[s.rank_of().Value]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard file_bb(File f)
    {
        return new Bitboard(FileBB[f.Value]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard file_bb(Square s)
    {
        return new Bitboard(FileBB[s.file_of().Value]);
    }

    /// adjacent_files_bb() returns a bitboard representing all the squares on the
    /// adjacent files of the given one.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard adjacent_files_bb(File f)
    {
        return new Bitboard(AdjacentFilesBB[f.Value]);
    }


    /// between_bb() returns a bitboard representing all the squares between the two
    /// given ones. For instance, between_bb(SQ_C4, SQ_F7) returns a bitboard with
    /// the bits for square d5 and e6 set. If s1 and s2 are not on the same rank, file
    /// or diagonal, 0 is returned.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard between_bb(Square s1, Square s2)
    {
        return new Bitboard(BetweenBB[s1.Value, s2.Value]);
    }


    /// in_front_bb() returns a bitboard representing all the squares on all the ranks
    /// in front of the given one, from the point of view of the given color. For
    /// instance, in_front_bb(BLACK, RANK_3) will return the squares on ranks 1 and 2.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard in_front_bb(Color c, Rank r)
    {
        return new Bitboard(InFrontBB[c.Value, r.Value]);
    }


    /// forward_bb() returns a bitboard representing all the squares along the line
    /// in front of the given one, from the point of view of the given color:
    ///        ForwardBB[c][s] = in_front_bb(c, s) & file_bb(s)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard forward_bb(Color c, Square s)
    {
        return new Bitboard(ForwardBB[c.Value, s.Value]);
    }


    /// pawn_attack_span() returns a bitboard representing all the squares that can be
    /// attacked by a pawn of the given color when it moves along its file, starting
    /// from the given square:
    ///       PawnAttackSpan[c][s] = in_front_bb(c, s) & adjacent_files_bb(s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard pawn_attack_span(Color c, Square s)
    {
        return new Bitboard(PawnAttackSpan[c.Value, s.Value]);
    }


    /// passed_pawn_mask() returns a bitboard mask which can be used to test if a
    /// pawn of the given color and on the given square is a passed pawn:
    ///       PassedPawnMask[c][s] = pawn_attack_span(c, s) | forward_bb(c, s)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard passed_pawn_mask(Color c, Square s)
    {
        return new Bitboard(PassedPawnMask[c.Value, s.Value]);
    }


    /// aligned() returns true if the squares s1, s2 and s3 are aligned either on a
    /// straight or on a diagonal line.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool aligned(Square s1, Square s2, Square s3)
    {
        return (LineBB[s1.Value, s2.Value] & s3).Value != 0;
    }

    /// distance() functions return the distance between x and y, defined as the
    /// number of steps for a king in x to reach y. Works with squares, ranks, files.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int distance_Square(Square x, Square y)
    {
        return SquareDistance[x.Value, y.Value];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int file_distance(Square x, Square y)
    {
        return Math.Abs(x.file_of().Value - y.file_of().Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int rank_distance(Square x, Square y)
    {
        return Math.Abs(x.rank_of().Value - y.rank_of().Value);
    }

    /// attacks_bb() returns a bitboard representing all the squares attacked by a
    /// piece of type Pt (bishop or rook) placed on 's'. The helper magic_index()
    /// looks up the index using the 'magic bitboards' approach.
    public static uint magic_index(PieceType Pt, Square s, Bitboard occupied)
    {
        var Masks = Pt.Value == PieceType.ROOK ? RookMasks : BishopMasks;
        var Magics = Pt.Value == PieceType.ROOK ? RookMagics : BishopMagics;
        var Shifts = Pt.Value == PieceType.ROOK ? RookShifts : BishopShifts;

#if X64
        return (uint)(((occupied.Value & Masks[s.Value].Value) * Magics[s.Value].Value) >> (int)Shifts[s.Value]);
#else

        var lo = (uint)(occupied.Value) & (uint)(Masks[s.Value].Value);
        var hi = (uint)(occupied.Value >> 32) & (uint)(Masks[s.Value].Value >> 32);
        return (lo * (uint)(Magics[s.Value].Value) ^ hi * (uint)(Magics[s.Value].Value >> 32)) >> (int)Shifts[s.Value];
#endif
   }

    public static Bitboard attacks_bb(PieceType Pt, Square s, Bitboard occupied)
    {
        return Pt.Value == PieceType.ROOK ? 
            RookAttacks[s.Value][magic_index(Pt, s, occupied)] : 
            BishopAttacks[s.Value][magic_index(Pt, s, occupied)];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard attacks_bb(Piece pc, Square s, Bitboard occupied)
    {
        switch (pc.type_of().Value)
        {
            case PieceType.BISHOP: return attacks_bb(new PieceType(PieceType.BISHOP), s, occupied);
            case PieceType.ROOK: return attacks_bb(new PieceType(PieceType.ROOK), s, occupied);
            case PieceType.QUEEN: return new Bitboard(attacks_bb(new PieceType(PieceType.BISHOP), s, occupied).Value | attacks_bb(new PieceType(PieceType.ROOK), s, occupied).Value);
            default: return StepAttacksBB[pc.Value, s.Value];
        }
    }

    /*
    /// pop_lsb() finds and clears the least significant bit in a non-zero bitboard
    inline Square pop_lsb(Bitboard* b)
    {
        const Square s = lsb(*b);
        *b &= *b - 1;
        return s;
    }


    /// frontmost_sq() and backmost_sq() return the square corresponding to the
    /// most/least advanced bit relative to the given color.

    inline Square frontmost_sq(Color c, Bitboard b) { return c == WHITE ? msb(b) : lsb(b); }
    inline Square  backmost_sq(Color c, Bitboard b) { return c == WHITE ? lsb(b) : msb(b); }
    */
}

