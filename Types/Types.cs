/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2008 Tord Romstad (Glaurung author)
  Copyright (C) 2008-2015 Marco Costalba, Joona Kiiski, Tord Romstad

  Stockfish is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  Stockfish is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

internal struct _
{
    internal const int MAX_MOVES = 256;

    internal const int MAX_PLY = 128;

    internal const int MAX_THREADS = 128;

    internal const int MAX_SPLITPOINTS_PER_THREAD = 8;

    internal const int MAX_SLAVES_PER_SPLITPOINT = 4;
}

// Different node types, used as template parameter
public enum NodeType
{
    Root,

    PV,

    NonPV
};

public enum Stages
{
    MAIN_SEARCH,

    GOOD_CAPTURES,

    KILLERS,

    GOOD_QUIETS,

    BAD_QUIETS,

    BAD_CAPTURES,

    EVASION,

    ALL_EVASIONS,

    QSEARCH_WITH_CHECKS,

    QCAPTURES_1,

    CHECKS,

    QSEARCH_WITHOUT_CHECKS,

    QCAPTURES_2,

    PROBCUT,

    PROBCUT_CAPTURES,

    RECAPTURE,

    RECAPTURES,

    STOP
};

public enum GenType
{
    CAPTURES,

    QUIETS,

    QUIET_CHECKS,

    EVASIONS,

    NON_EVASIONS,

    LEGAL
};

public enum Result
{
    INVALID = 0,

    UNKNOWN = 1,

    DRAW = 2,

    WIN = 4
};

public enum MoveType
{
    NORMAL,

    PROMOTION = 1 << 14,

    ENPASSANT = 2 << 14,

    CASTLING = 3 << 14
};

public enum CastlingSide
{
    KING_SIDE,

    QUEEN_SIDE,

    CASTLING_SIDE_NB = 2
};

public enum CastlingRight
{
    NO_CASTLING,

    WHITE_OO,

    WHITE_OOO = WHITE_OO << 1,

    BLACK_OO = WHITE_OO << 2,

    BLACK_OOO = WHITE_OO << 3,

    ANY_CASTLING = WHITE_OO | WHITE_OOO | BLACK_OO | BLACK_OOO,

    CASTLING_RIGHT_NB = 16
};

public enum Phase
{
    PHASE_ENDGAME = 0,

    PHASE_MIDGAME = 128,

    MG = 0,

    EG = 1,

    PHASE_NB = 2
};

public enum ScaleFactor
{
    SCALE_FACTOR_DRAW = 0,

    SCALE_FACTOR_ONEPAWN = 48,

    SCALE_FACTOR_NORMAL = 64,

    SCALE_FACTOR_MAX = 128,

    SCALE_FACTOR_NONE = 255
};

public enum Bound
{
    BOUND_NONE,

    BOUND_UPPER,

    BOUND_LOWER,

    BOUND_EXACT = BOUND_UPPER | BOUND_LOWER
};

/*
extern Value PieceValue[PHASE_NB][PIECE_NB];

template<Color C, CastlingSide S> struct MakeCastling
{
    static const CastlingRight
    right = C == WHITE ? S == QUEEN_SIDE ? WHITE_OOO : WHITE_OO
                       : S == QUEEN_SIDE ? BLACK_OOO : BLACK_OO;
};
*/