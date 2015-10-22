using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class MovePicker
{
    private readonly Position pos;

    private readonly Value threshold;

    private readonly Move ttMove;

    private Move countermove;

    private readonly CounterMovesHistoryStats counterMovesHistory;

    private PositionArray cur;

    private Depth depth;

    private PositionArray endBadCaptures;

    private PositionArray endMoves;

    private PositionArray endQuiets;

    private readonly HistoryStats history;

    private ExtMove[] killers = new ExtMove[3];

    private readonly ExtMove[] moves = new ExtMove[_.MAX_MOVES];

    private Square recaptureSquare;

    //TODO: Search::Stack!
    private Stack ss;

    private Stages stage;

    /// Constructors of the MovePicker class. As arguments we pass information
    /// to help it to return the (presumably) good moves first, to decide which
    /// moves to return (in the quiescence search, for instance, we only want to
    /// search captures, promotions and some checks) and how important good move
    /// ordering is at the current node.
    public MovePicker(Position p, Move ttm, Depth d, HistoryStats h, CounterMovesHistoryStats cmh, Move cm, Stack s)
    {
        endBadCaptures = new PositionArray(moves, _.MAX_MOVES - 1);
        cur = new PositionArray(moves);
        endMoves = new PositionArray(moves);

        this.pos = p;
        this.history = h;
        this.counterMovesHistory = cmh;
        this.ss = s;
        this.countermove = cm;
        this.depth = d;
        Debug.Assert(d > Depth.DEPTH_ZERO);

        this.stage = this.pos.checkers() ? Stages.EVASION : Stages.MAIN_SEARCH;
        this.ttMove = ttm != 0 && this.pos.pseudo_legal(ttm) ? ttm : Move.MOVE_NONE;
        this.endMoves += this.ttMove != Move.MOVE_NONE ? 1 : 0;
    }

    public MovePicker(Position p, Move ttm, Depth d, HistoryStats h, CounterMovesHistoryStats cmh, Square s)
    {
        endBadCaptures = new PositionArray(moves, _.MAX_MOVES - 1);
        cur = new PositionArray(moves);
        endMoves = new PositionArray(moves);

        this.pos = p;
        this.history = h;
        this.counterMovesHistory = cmh;

        Debug.Assert(d <= Depth.DEPTH_ZERO);

        if (this.pos.checkers())
        {
            this.stage = Stages.EVASION;
        }

        else if (d > Depth.DEPTH_QS_NO_CHECKS)
        {
            this.stage = Stages.QSEARCH_WITH_CHECKS;
        }

        else if (d > Depth.DEPTH_QS_RECAPTURES)
        {
            this.stage = Stages.QSEARCH_WITHOUT_CHECKS;
        }

        else
        {
            this.stage = Stages.RECAPTURE;
            this.recaptureSquare = s;
            ttm = Move.MOVE_NONE;
        }

        this.ttMove = ttm != 0 && this.pos.pseudo_legal(ttm) ? ttm : Move.MOVE_NONE;
        this.endMoves += (this.ttMove != Move.MOVE_NONE) ? 1 : 0;
    }

    public MovePicker(Position p, Move ttm, HistoryStats h, CounterMovesHistoryStats cmh, Value th)
    {
        endBadCaptures = new PositionArray(moves, _.MAX_MOVES - 1);
        cur = new PositionArray(moves);
        endMoves = new PositionArray(moves);

        this.pos = p;
        this.history = h;
        this.counterMovesHistory = cmh;
        this.threshold = th;

        Debug.Assert(!this.pos.checkers());

        this.stage = Stages.PROBCUT;

        // In ProbCut we generate captures with SEE higher than the given threshold
        this.ttMove = ttm != 0 && this.pos.pseudo_legal(ttm) && this.pos.capture(ttm)
                      && this.pos.see(ttm) > this.threshold
                          ? ttm
                          : Move.MOVE_NONE;

        this.endMoves += (this.ttMove != Move.MOVE_NONE) ? 1 : 0;
    }

    // Our insertion sort, which is guaranteed to be stable, as it should be
    private void insertion_sort(PositionArray begin, PositionArray end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.last < end.last);

        var equalityComparer = Comparer<ExtMove>.Default;
        for (var counter = begin.last; counter < end.last - 1; counter++)
        {
            var index = counter + 1;
            while (index > 0)
            {
                if (equalityComparer.Compare(begin.table[index - 1], begin.table[index]) > 0)
                {
                    var temp = begin.table[index - 1];
                    begin.table[index - 1] = begin.table[index];
                    begin.table[index] = temp;
                }
                index--;
            }
        }
    }

    // pick_best() finds the best move in the range (begin, end) and moves it to
    // the front. It's faster than sorting all the moves in advance when there
    // are few moves e.g. the possible captures.
    private Move pick_best(PositionArray begin, PositionArray end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.last < end.last);

        ExtMove? maxVal = null; //nullable so this works even if you have all super-low negatives
        var index = -1;
        for (var i = begin.last; i < end.last; i++)
        {
            var thisNum = moves[i];
            if (!maxVal.HasValue || thisNum > maxVal.Value)
            {
                maxVal = thisNum;
                index = i;
            }
        }

        var first = moves[begin.last];
        moves[begin.last] = moves[index];
        moves[index] = first;

        return moves[begin.last];
    }

    public void score(GenType Type)
    {
        switch (Type)
        {
            case GenType.CAPTURES:
                this.score_CAPTURES();
                return;
            case GenType.EVASIONS:
                this.score_EVASIONS();
                return;
            case GenType.QUIETS:
                this.score_QUIETS();
                return;
        }

        Debug.Assert(false);
    }

    /// score() assigns a numerical value to each move in a move list. The moves with
    /// highest values will be picked first.
    private void score_CAPTURES()
    {
        // Winning and equal captures in the main search are ordered by MVV, preferring
        // captures near our home rank. Suprisingly, this appears to perform slightly
        // better than SEE based move ordering: exchanging big pieces before capturing
        // a hanging piece probably helps to reduce the subtree size.
        // In main search we want to push captures with negative SEE values to the
        // badCaptures[] array, but instead of doing it now we delay until the move
        // has been picked up, saving some SEE calls in case we get a cutoff.

        for (var i = 0; i < this.endMoves.last; i++)
        {
            var m = this.moves[i];
            this.moves[i] = new ExtMove(m, 
                        Value.PieceValue[(int)Phase.MG][this.pos.piece_on(Move.to_sq(m))]
                      - new Value(200 * Rank.relative_rank(this.pos.side_to_move(), Move.to_sq(m))));
        }
    }

    private void score_QUIETS()
    {
        // TODO: replace Stack
        var prevSq = new Square(0); //Move.to_sq((ss - 1)->currentMove);
        var cmh = this.counterMovesHistory.value(this.pos.piece_on(prevSq), prevSq);

        for (var i = 0; i < this.endMoves.last; i++)
        {
            var m = this.moves[i];
            this.moves[i] = new ExtMove(m,
             this.history.value(this.pos.moved_piece(m), Move.to_sq(m))
                      + cmh.value(this.pos.moved_piece(m), Move.to_sq(m)));
        }
    }

    private void score_EVASIONS()
    {
        // Try winning and equal captures captures ordered by MVV/LVA, then non-captures
        // ordered by history value, then bad-captures and quiet moves with a negative
        // SEE ordered by SEE value.
        Value see;

        for (var i = 0; i < this.endMoves.last; i++)
        {
            var m = this.moves[i];
            if ((see = this.pos.see_sign(m)) < Value.VALUE_ZERO)
            {
                this.moves[i] = new ExtMove(m,
                            see - HistoryStats.Max); // At the bottom
            }

            else if (this.pos.capture(m))
            {
                this.moves[i] = new ExtMove(m,
                Value.PieceValue[(int)Phase.MG][this.pos.piece_on(Move.to_sq(m))]
                          - new Value(Piece.type_of(this.pos.moved_piece(m))) + HistoryStats.Max);
            }
            else
            {
                this.moves[i] = new ExtMove(m,
                this.history.value(this.pos.moved_piece(m), Move.to_sq(m)));
            }
        }
    }

    /// generate_next_stage() generates, scores and sorts the next bunch of moves,
    /// when there are no more moves to try for the current stage.

    void generate_next_stage()
    {
        Debug.Assert(stage != Stages.STOP);

        cur.set(moves);

        switch (++stage)
        {

            case Stages.GOOD_CAPTURES:
            case Stages.QCAPTURES_1:
            case Stages.QCAPTURES_2:
            case Stages.PROBCUT_CAPTURES:
            case Stages.RECAPTURES:
                {
                    endMoves  = Movegen.generate(GenType.CAPTURES, pos, new PositionArray(moves));
                    score(GenType.CAPTURES);
                }
                break;

            case Stages.KILLERS:
                //TODO: add stack
                //killers[0] = ss.killers[0];
                //killers[1] = ss.killers[1];
                killers[2] = new ExtMove(countermove, killers[2].Value);
                cur.set(killers);
                endMoves = cur + 2 + ((countermove != killers[0] && countermove != killers[1]) ? 1 : 0);
                break;

            case Stages.GOOD_QUIETS:
                { 
                    var movelistPos = 0;
                    endQuiets = endMoves = Movegen.generate(GenType.QUIETS, pos, new PositionArray(moves));
                    this.score(GenType.QUIETS);

                    // TODO: find solution
                    // endMoves = std::partition(cur, endMoves, [](const ExtMove&m) { return m.value > Value.VALUE_ZERO; });
                    insertion_sort(cur, endMoves);
                }
                break;

            case Stages.BAD_QUIETS:
                cur = endMoves;
                endMoves = endQuiets;
                if (depth >= 3 * Depth.ONE_PLY)
                    insertion_sort(cur, endMoves);
                break;

            case Stages.BAD_CAPTURES:
                // Just pick them in reverse order to get correct ordering
                cur = new PositionArray(moves) + (_.MAX_MOVES - 1);
                endMoves = endBadCaptures;
                break;

            case Stages.ALL_EVASIONS:
                {
                    endMoves = Movegen.generate(GenType.EVASIONS, pos, new PositionArray(moves));
                    
                    if (endMoves.last > 1) score(GenType.EVASIONS);
                }
                break;

            case Stages.CHECKS:
                {
                    endMoves = Movegen.generate(GenType.QUIET_CHECKS, pos, new PositionArray(moves));
                }
                break;

            case Stages.EVASION:
            case Stages.QSEARCH_WITH_CHECKS:
            case Stages.QSEARCH_WITHOUT_CHECKS:
            case Stages.PROBCUT:
            case Stages.RECAPTURE:
            case Stages.STOP:
                stage = Stages.STOP;
                break;

            default:
                Debug.Assert(false);
                break;
        }
    }

    /// next_move() is the most important method of the MovePicker class. It returns
    /// a new pseudo legal move every time it is called, until there are no more moves
    /// left. It picks the move with the biggest value from a list of generated moves
    /// taking care not to return the ttMove if it has already been searched.
    public Move next_move(bool useSplipoint) {

        /// Version of next_move() to use at split point nodes where the move is grabbed
        /// from the split point's shared MovePicker object. This function is not thread
        /// safe so must be lock protected by the caller.
        if(useSplipoint)
        {
            //TODO: add stack
            //return ss.splitPoint.movePicker.next_move(false);
            return Move.MOVE_NONE;
        } 

        Move move;

  while (true)
  {
      while (cur == endMoves && stage != Stages.STOP)
          generate_next_stage();

      switch (stage) {

      case Stages.MAIN_SEARCH:
                case Stages.EVASION:
                case Stages.QSEARCH_WITH_CHECKS:
      case Stages.QSEARCH_WITHOUT_CHECKS:
                case Stages.PROBCUT:
          ++cur;
          return ttMove;

      case Stages.GOOD_CAPTURES:
          move = pick_best(cur++, endMoves);
          if (move != ttMove)
          {
              if (pos.see_sign(move) >= Value.VALUE_ZERO)
                  return move;

                        // Losing capture, move it to the tail of the array
                        (endBadCaptures--).setCurrentMove(move);
                                }
          break;

      case Stages.KILLERS:
          move = (cur++).getCurrentMove();
          if (    move != Move.MOVE_NONE
              &&  move != ttMove
              &&  pos.pseudo_legal(move)
              && !pos.capture(move))
              return move;
          break;

      case Stages.GOOD_QUIETS:
                case Stages.BAD_QUIETS:
          move = (cur++).getCurrentMove();
                    if (   move != ttMove
              && move != killers[0]
              && move != killers[1]
              && move != killers[2])
              return move;
          break;

      case Stages.BAD_CAPTURES:
          return (cur--).getCurrentMove();

                case Stages.ALL_EVASIONS:
                case Stages.QCAPTURES_1:
                case Stages.QCAPTURES_2:
          move = pick_best(cur++, endMoves);
          if (move != ttMove)
              return move;
          break;

      case Stages.PROBCUT_CAPTURES:
           move = pick_best(cur++, endMoves);
           if (move != ttMove && pos.see(move) > threshold)
               return move;
           break;

      case Stages.RECAPTURES:
          move = pick_best(cur++, endMoves);
          if (Move.to_sq(move) == recaptureSquare)
              return move;
          break;

      case Stages.CHECKS:
          move = (cur++).getCurrentMove();
                    if (move != ttMove)
              return move;
          break;

      case Stages.STOP:
          return Move.MOVE_NONE;

      default:
          Debug.Assert(false);
                    break;
      }
  }
}
}