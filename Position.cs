using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Position class stores information regarding the board representation as
/// pieces, side to move, hash keys, castling info, etc. Important methods are
/// do_move() and undo_move(), used by the search to update node info when
/// traversing the search tree.
public class Position
{
    /// Position::init() initializes at startup the various arrays used to compute
    /// hash keys.
    public static void init()
    {
        PRNG rng = new PRNG(1070372);

        for (Color c = Color.WHITE; c <= Color.BLACK; ++c)
            for (PieceType pt = PieceType.PAWN; pt <= PieceType.KING; ++pt)
                for (Square s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                    Zobrist.psq[c, pt, s] = rng.rand();

        for (File f = File.FILE_A; f <= File.FILE_H; ++f)
            Zobrist.enpassant[f] = rng.rand();

        for (int cr = (int)CastlingRight.NO_CASTLING; cr <= (int)CastlingRight.ANY_CASTLING; ++cr)
        {
            Zobrist.castling[cr] = 0;
            Bitboard b = new Bitboard((ulong)cr);
            while (b)
            {
                ulong k = Zobrist.castling[1 << Utils.pop_lsb(ref b)];
                Zobrist.castling[cr] ^= (k != 0 ? k : rng.rand());
            }
        }

        Zobrist.side = rng.rand();
        Zobrist.exclusion = rng.rand();
    }
}

/*
friend std::ostream& operator<<(std::ostream&, const Position&);

public:
  

  Position() = default; // To define the global object RootPos
  Position(const Position&) = delete;
  Position(const Position& pos, Thread* th) { *this = pos; thisThread = th; }
  Position(const std::string& f, bool c960, Thread* th) { set(f, c960, th); }
  Position& operator=(const Position&); // To assign RootPos from UCI

  // FEN string input/output
  void set(const std::string& fenStr, bool isChess960, Thread* th);
  const std::string fen() const;

  // Position representation
  Bitboard pieces() const;
  Bitboard pieces(PieceType pt) const;
  Bitboard pieces(PieceType pt1, PieceType pt2) const;
  Bitboard pieces(Color c) const;
  Bitboard pieces(Color c, PieceType pt) const;
  Bitboard pieces(Color c, PieceType pt1, PieceType pt2) const;
  Piece piece_on(Square s) const;
  Square ep_square() const;
  bool empty(Square s) const;
  template<PieceType Pt> int count(Color c) const;
  template<PieceType Pt> const Square* squares(Color c) const;
  template<PieceType Pt> Square square(Color c) const;

  // Castling
  int can_castle(Color c) const;
  int can_castle(CastlingRight cr) const;
  bool castling_impeded(CastlingRight cr) const;
  Square castling_rook_square(CastlingRight cr) const;

  // Checking
  Bitboard checkers() const;
  Bitboard discovered_check_candidates() const;
  Bitboard pinned_pieces(Color c) const;

  // Attacks to/from a given square
  Bitboard attackers_to(Square s) const;
  Bitboard attackers_to(Square s, Bitboard occupied) const;
  Bitboard attacks_from(Piece pc, Square s) const;
  template<PieceType> Bitboard attacks_from(Square s) const;
  template<PieceType> Bitboard attacks_from(Square s, Color c) const;

  // Properties of moves
  bool legal(Move m, Bitboard pinned) const;
  bool pseudo_legal(const Move m) const;
  bool capture(Move m) const;
  bool capture_or_promotion(Move m) const;
  bool gives_check(Move m, const CheckInfo& ci) const;
  bool advanced_pawn_push(Move m) const;
  Piece moved_piece(Move m) const;
  PieceType captured_piece_type() const;

  // Piece specific
  bool pawn_passed(Color c, Square s) const;
  bool opposite_bishops() const;

  // Doing and undoing moves
  void do_move(Move m, StateInfo& st, bool givesCheck);
  void undo_move(Move m);
  void do_null_move(StateInfo& st);
  void undo_null_move();

  // Static exchange evaluation
  Value see(Move m) const;
  Value see_sign(Move m) const;

  // Accessing hash keys
  Key key() const;
  Key key_after(Move m) const;
  Key exclusion_key() const;
  Key material_key() const;
  Key pawn_key() const;

  // Other properties of the position
  Color side_to_move() const;
  Phase game_phase() const;
  int game_ply() const;
  bool is_chess960() const;
  Thread* this_thread() const;
  uint64_t nodes_searched() const;
  void set_nodes_searched(uint64_t n);
  bool is_draw() const;
  int rule50_count() const;
  Score psq_score() const;
  Value non_pawn_material(Color c) const;

  // Position consistency check, for debugging
  bool pos_is_ok(int* failedStep = nullptr) const;
  void flip();

private:
  // Initialization helpers (used while setting up a position)
  void clear();
  void set_castling_right(Color c, Square rfrom);
  void set_state(StateInfo* si) const;

  // Other helpers
  Bitboard check_blockers(Color c, Color kingColor) const;
  void put_piece(Color c, PieceType pt, Square s);
  void remove_piece(Color c, PieceType pt, Square s);
  void move_piece(Color c, PieceType pt, Square from, Square to);
  template<bool Do>
  void do_castling(Color us, Square from, Square& to, Square& rfrom, Square& rto);

  // Data members
  Piece board[SQUARE_NB];
  Bitboard byTypeBB[PIECE_TYPE_NB];
  Bitboard byColorBB[COLOR_NB];
  int pieceCount[COLOR_NB][PIECE_TYPE_NB];
  Square pieceList[COLOR_NB][PIECE_TYPE_NB][16];
  int index[SQUARE_NB];
  int castlingRightsMask[SQUARE_NB];
  Square castlingRookSquare[CASTLING_RIGHT_NB];
  Bitboard castlingPath[CASTLING_RIGHT_NB];
  StateInfo startState;
  uint64_t nodes;
  int gamePly;
  Color sideToMove;
  Thread* thisThread;
  StateInfo* st;
  bool chess960;
*/
