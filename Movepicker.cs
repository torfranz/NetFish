using System.Diagnostics;

public class MovePicker
{
    private readonly Move countermove;

    private readonly CounterMovesHistoryStats counterMovesHistory;

    private readonly Depth depth;

    private readonly HistoryStats history;

    private readonly ExtMove[] killers = new ExtMove[3];

    public readonly ExtMove[] moves = new ExtMove[_.MAX_MOVES];

    private readonly Position pos;

    private readonly Square recaptureSquare;

    private readonly Value threshold;

    private readonly Move ttMove;

    private ExtMoveArrayWrapper cur;

    private ExtMoveArrayWrapper endBadCaptures;

    private ExtMoveArrayWrapper endMoves;

    private ExtMoveArrayWrapper endQuiets;

    private readonly StackArrayWrapper ss;

    private Stages stage;

    /// Constructors of the MovePicker class. As arguments we pass information
    /// to help it to return the (presumably) good moves first, to decide which
    /// moves to return (in the quiescence search, for instance, we only want to
    /// search captures, promotions and some checks) and how important good move
    /// ordering is at the current node.
    public MovePicker(
        Position p,
        Move ttm,
        Depth d,
        HistoryStats h,
        CounterMovesHistoryStats cmh,
        Move cm,
        StackArrayWrapper s)
    {
        this.endBadCaptures = new ExtMoveArrayWrapper(this.moves, _.MAX_MOVES - 1);
        this.cur = new ExtMoveArrayWrapper(this.moves);
        this.endMoves = new ExtMoveArrayWrapper(this.moves);

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
        this.endBadCaptures = new ExtMoveArrayWrapper(this.moves, _.MAX_MOVES - 1);
        this.cur = new ExtMoveArrayWrapper(this.moves);
        this.endMoves = new ExtMoveArrayWrapper(this.moves);

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
        this.endBadCaptures = new ExtMoveArrayWrapper(this.moves, _.MAX_MOVES - 1);
        this.cur = new ExtMoveArrayWrapper(this.moves);
        this.endMoves = new ExtMoveArrayWrapper(this.moves);

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

    // pick_best() finds the best move in the range (begin, end) and moves it to
    // the front. It's faster than sorting all the moves in advance when there
    // are few moves e.g. the possible captures.
    private Move pick_best(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current < end.current);

        ExtMove? maxVal = null; //nullable so this works even if you have all super-low negatives
        var index = -1;
        for (var i = begin.current; i < end.current; i++)
        {
            var thisNum = this.moves[i];
            if (!maxVal.HasValue || thisNum > maxVal.Value)
            {
                maxVal = thisNum;
                index = i;
            }
        }

        var first = this.moves[begin.current];
        this.moves[begin.current] = this.moves[index];
        this.moves[index] = first;

        return this.moves[begin.current];
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

        for (var i = 0; i < this.endMoves.current; i++)
        {
            var m = this.moves[i];
            this.moves[i] = new ExtMove(
                m,
                Value.PieceValue[(int)Phase.MG][this.pos.piece_on(Move.to_sq(m))]
                - new Value(200 * Rank.relative_rank(this.pos.side_to_move(), Move.to_sq(m))));
        }
    }

    private void score_QUIETS()
    {
        var prevSq = Move.to_sq(this.ss[this.ss.current - 1].currentMove);
        var cmh = this.counterMovesHistory.value(this.pos.piece_on(prevSq), prevSq);

        for (var i = 0; i < this.endMoves.current; i++)
        {
            var m = this.moves[i];
            this.moves[i] = new ExtMove(
                m,
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

        for (var i = 0; i < this.endMoves.current; i++)
        {
            var m = this.moves[i];
            if ((see = this.pos.see_sign(m)) < Value.VALUE_ZERO)
            {
                this.moves[i] = new ExtMove(m, see - HistoryStats.Max); // At the bottom
            }

            else if (this.pos.capture(m))
            {
                this.moves[i] = new ExtMove(
                    m,
                    Value.PieceValue[(int)Phase.MG][this.pos.piece_on(Move.to_sq(m))]
                    - new Value(Piece.type_of(this.pos.moved_piece(m))) + HistoryStats.Max);
            }
            else
            {
                this.moves[i] = new ExtMove(m, this.history.value(this.pos.moved_piece(m), Move.to_sq(m)));
            }
        }
    }

    /// generate_next_stage() generates, scores and sorts the next bunch of moves,
    /// when there are no more moves to try for the current stage.
    private void generate_next_stage()
    {
        Debug.Assert(this.stage != Stages.STOP);

        this.cur.set(this.moves);

        switch (++this.stage)
        {
            case Stages.GOOD_CAPTURES:
            case Stages.QCAPTURES_1:
            case Stages.QCAPTURES_2:
            case Stages.PROBCUT_CAPTURES:
            case Stages.RECAPTURES:
                {
                    this.endMoves = Movegen.generate(GenType.CAPTURES, this.pos, new ExtMoveArrayWrapper(this.moves));
                    this.score(GenType.CAPTURES);
                }
                break;

            case Stages.KILLERS:
                this.killers[0] = new ExtMove(this.ss[this.ss.current].killers0, this.killers[0].Value);
                this.killers[1] = new ExtMove(this.ss[this.ss.current].killers1, this.killers[1].Value);
                this.killers[2] = new ExtMove(this.countermove, this.killers[2].Value);
                this.cur.set(this.killers);
                this.endMoves = this.cur + 2
                                + ((this.countermove != this.killers[0] && this.countermove != this.killers[1]) ? 1 : 0);
                break;

            case Stages.GOOD_QUIETS:
                {
                    this.endQuiets = Movegen.generate(GenType.QUIETS, this.pos, new ExtMoveArrayWrapper(this.moves));
                    this.endMoves = this.endQuiets;
                    this.score(GenType.QUIETS);

                    this.endMoves = ExtMoveArrayWrapper.Partition(this.cur, this.endMoves);
                    ExtMoveArrayWrapper.insertion_sort(this.cur, this.endMoves);
                }
                break;

            case Stages.BAD_QUIETS:
                this.cur = new ExtMoveArrayWrapper(this.endMoves);
                this.endMoves = this.endQuiets;
                if (this.depth >= 3 * Depth.ONE_PLY)
                {
                    ExtMoveArrayWrapper.insertion_sort(this.cur, this.endMoves);
                }
                break;

            case Stages.BAD_CAPTURES:
                // Just pick them in reverse order to get correct ordering
                this.cur = new ExtMoveArrayWrapper(this.moves) + (_.MAX_MOVES - 1);
                this.endMoves = this.endBadCaptures;
                break;

            case Stages.ALL_EVASIONS:
                {
                    this.endMoves = Movegen.generate(GenType.EVASIONS, this.pos, new ExtMoveArrayWrapper(this.moves));

                    if (this.endMoves.current > 1)
                    {
                        this.score(GenType.EVASIONS);
                    }
                }
                break;

            case Stages.CHECKS:
                {
                    this.endMoves = Movegen.generate(
                        GenType.QUIET_CHECKS,
                        this.pos,
                        new ExtMoveArrayWrapper(this.moves));
                }
                break;

            case Stages.EVASION:
            case Stages.QSEARCH_WITH_CHECKS:
            case Stages.QSEARCH_WITHOUT_CHECKS:
            case Stages.PROBCUT:
            case Stages.RECAPTURE:
            case Stages.STOP:
                this.stage = Stages.STOP;
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
    public Move next_move(bool useSplitpoint)
    {
        /// Version of next_move() to use at split point nodes where the move is grabbed
        /// from the split point's shared MovePicker object. This function is not thread
        /// safe so must be lock protected by the caller.
        if (useSplitpoint)
        {
            return this.ss[this.ss.current].splitPoint.movePicker.next_move(false);
        }

        Move move;

        while (true)
        {
            while (this.cur == this.endMoves && this.stage != Stages.STOP)
            {
                this.generate_next_stage();
            }

            switch (this.stage)
            {
                case Stages.MAIN_SEARCH:
                case Stages.EVASION:
                case Stages.QSEARCH_WITH_CHECKS:
                case Stages.QSEARCH_WITHOUT_CHECKS:
                case Stages.PROBCUT:
                    ++this.cur;
                    return this.ttMove;

                case Stages.GOOD_CAPTURES:
                    move = this.pick_best(this.cur, this.endMoves);
                    this.cur++;
                    if (move != this.ttMove)
                    {
                        if (this.pos.see_sign(move) >= Value.VALUE_ZERO)
                        {
                            return move;
                        }

                        // Losing capture, move it to the tail of the array
                        this.endBadCaptures.setCurrentMove(move);
                        --this.endBadCaptures;
                    }
                    break;

                case Stages.KILLERS:
                    move = this.cur.getCurrentMove();
                    this.cur++;
                    if (move != Move.MOVE_NONE && move != this.ttMove && this.pos.pseudo_legal(move)
                        && !this.pos.capture(move))
                    {
                        return move;
                    }
                    break;

                case Stages.GOOD_QUIETS:
                case Stages.BAD_QUIETS:
                    move = this.cur.getCurrentMove();
                    this.cur++;
                    if (move != this.ttMove && move != this.killers[0] && move != this.killers[1]
                        && move != this.killers[2])
                    {
                        return move;
                    }
                    break;

                case Stages.BAD_CAPTURES:
                    return (this.cur--).getCurrentMove();

                case Stages.ALL_EVASIONS:
                case Stages.QCAPTURES_1:
                case Stages.QCAPTURES_2:
                    move = this.pick_best(this.cur, this.endMoves);
                    this.cur++;
                    if (move != this.ttMove)
                    {
                        return move;
                    }
                    break;

                case Stages.PROBCUT_CAPTURES:
                    move = this.pick_best(this.cur, this.endMoves);
                    this.cur++;
                    if (move != this.ttMove && this.pos.see(move) > this.threshold)
                    {
                        return move;
                    }
                    break;

                case Stages.RECAPTURES:
                    move = this.pick_best(this.cur, this.endMoves);
                    this.cur++;
                    if (Move.to_sq(move) == this.recaptureSquare)
                    {
                        return move;
                    }
                    break;

                case Stages.CHECKS:
                    move = this.cur.getCurrentMove();
                    this.cur++;
                    if (move != this.ttMove)
                    {
                        return move;
                    }
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