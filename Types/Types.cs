/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2008 Tord Romstad (Glaurung author)
  Copyright (C) 2008-2015 Marco Costalba, Joona Kiiski, Tord Romstad

  Stockfish is free software: you can redistribute it and/or modify
  it under the terms of the GNU General internal License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  Stockfish is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General internal License for more details.

  You should have received a copy of the GNU General internal License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

internal struct _
{
    internal const int MAX_MOVES = 256;

    internal const int MAX_PLY = 128;

    internal const int MAX_THREADS = 128;

    internal const int MAX_SPLITPOINTS_PER_THREAD = 8;

    internal const int MAX_SLAVES_PER_SPLITPOINT = 4;

    internal const double DBL_MIN = 2.2250738585072014e-308; // min positive value
}

/// EndgameType lists all supported endgames
internal enum EndgameType
{
    // Evaluation functions

    KNNK, // KNN vs K

    KXK, // Generic "mate lone king" eval

    KBNK, // KBN vs K

    KPK, // KP vs K

    KRKP, // KR vs KP

    KRKB, // KR vs KB

    KRKN, // KR vs KN

    KQKP, // KQ vs KP

    KQKR, // KQ vs KR

    // Scaling functions
    SCALING_FUNCTIONS,

    KBPsK, // KB and pawns vs K

    KQKRPs, // KQ vs KR and pawns

    KRPKR, // KRP vs KR

    KRPKB, // KRP vs KB

    KRPPKRP, // KRPP vs KRP

    KPsK, // K and pawns vs K

    KBPKB, // KBP vs KB

    KBPPKB, // KBPP vs KB

    KBPKN, // KBP vs KN

    KNPK, // KNP vs K

    KNPKB, // KNP vs KB

    KPKP // KP vs KP
};

// Different node types, used as template parameter
internal enum NodeType
{
    Root,

    PV,

    NonPV
};

internal enum Stages
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

internal enum GenType
{
    CAPTURES,

    QUIETS,

    QUIET_CHECKS,

    EVASIONS,

    NON_EVASIONS,

    LEGAL
};

internal enum Result
{
    INVALID = 0,

    UNKNOWN = 1,

    DRAW = 2,

    WIN = 4
};

internal enum MoveType
{
    NORMAL,

    PROMOTION = 1 << 14,

    ENPASSANT = 2 << 14,

    CASTLING = 3 << 14
};

internal enum CastlingSide
{
    KING_SIDE,

    QUEEN_SIDE,

    CASTLING_SIDE_NB = 2
};

internal enum CastlingRight
{
    NO_CASTLING,

    WHITE_OO,

    WHITE_OOO = WHITE_OO << 1,

    BLACK_OO = WHITE_OO << 2,

    BLACK_OOO = WHITE_OO << 3,

    ANY_CASTLING = WHITE_OO | WHITE_OOO | BLACK_OO | BLACK_OOO,

    CASTLING_RIGHT_NB = 16
};

internal enum Phase
{
    PHASE_ENDGAME = 0,

    PHASE_MIDGAME = 128,

    MG = 0,

    EG = 1,

    PHASE_NB = 2
};

internal enum ScaleFactor
{
    SCALE_FACTOR_DRAW = 0,

    SCALE_FACTOR_ONEPAWN = 48,

    SCALE_FACTOR_NORMAL = 64,

    SCALE_FACTOR_MAX = 128,

    SCALE_FACTOR_NONE = 255
};

internal enum Bound
{
    BOUND_NONE,

    BOUND_UPPER,

    BOUND_LOWER,

    BOUND_EXACT = BOUND_UPPER | BOUND_LOWER
};

/*
extern ValueMe PieceValue[PHASE_NB][PIECE_NB];

template<Color C, CastlingSide S> struct MakeCastling
{
    static const CastlingRight
    right = C == WHITE ? S == QUEEN_SIDE ? WHITE_OOO : WHITE_OO
                       : S == QUEEN_SIDE ? BLACK_OOO : BLACK_OO;
};
*/