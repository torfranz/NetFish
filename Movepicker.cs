using System.Diagnostics;

internal class MovePicker
{
    private readonly Move countermove;

    private readonly CounterMovesHistoryStats counterMovesHistory;

    private readonly Depth depth;

    private readonly HistoryStats history;

    private readonly ExtMove[] killers = new ExtMove[3];

    internal readonly ExtMove[] moves = new ExtMove[_.MAX_MOVES];

    private readonly Position pos;

    private readonly Square recaptureSquare;

    private readonly StackArrayWrapper ss;

    private readonly Value threshold;

    private readonly Move ttMove;

    private ExtMoveArrayWrapper cur;

    private ExtMoveArrayWrapper endBadCaptures;

    private ExtMoveArrayWrapper endMoves;

    private ExtMoveArrayWrapper endQuiets;

    private Stages stage;

    /// Constructors of the MovePicker class. As arguments we pass information
    /// to help it to return the (presumably) good moves first, to decide which
    /// moves to return (in the quiescence search, for instance, we only want to
    /// search captures, promotions and some checks) and how important good move
    /// ordering is at the current node.
    internal MovePicker(
        Position p,
        Move ttm,
        Depth d,
        HistoryStats h,
        CounterMovesHistoryStats cmh,
        Move cm,
        StackArrayWrapper s)
    {
        endBadCaptures = new ExtMoveArrayWrapper(moves, _.MAX_MOVES - 1);
        cur = new ExtMoveArrayWrapper(moves);
        endMoves = new ExtMoveArrayWrapper(moves);

        pos = p;
        history = h;
        counterMovesHistory = cmh;
        ss = s;
        countermove = cm;
        depth = d;
        Debug.Assert(d > Depth.DEPTH_ZERO);

        stage = pos.checkers() ? Stages.EVASION : Stages.MAIN_SEARCH;
        ttMove = ttm != 0 && pos.pseudo_legal(ttm) ? ttm : Move.MOVE_NONE;
        endMoves += ttMove != Move.MOVE_NONE ? 1 : 0;
    }

    internal MovePicker(Position p, Move ttm, Depth d, HistoryStats h, CounterMovesHistoryStats cmh, Square s)
    {
        endBadCaptures = new ExtMoveArrayWrapper(moves, _.MAX_MOVES - 1);
        cur = new ExtMoveArrayWrapper(moves);
        endMoves = new ExtMoveArrayWrapper(moves);

        pos = p;
        history = h;
        counterMovesHistory = cmh;

        Debug.Assert(d <= Depth.DEPTH_ZERO_C);

        if (pos.checkers())
        {
            stage = Stages.EVASION;
        }

        else if (d > Depth.DEPTH_QS_NO_CHECKS)
        {
            stage = Stages.QSEARCH_WITH_CHECKS;
        }

        else if (d > Depth.DEPTH_QS_RECAPTURES)
        {
            stage = Stages.QSEARCH_WITHOUT_CHECKS;
        }

        else
        {
            stage = Stages.RECAPTURE;
            recaptureSquare = s;
            ttm = Move.MOVE_NONE;
        }

        ttMove = ttm != 0 && pos.pseudo_legal(ttm) ? ttm : Move.MOVE_NONE;
        endMoves += (ttMove != Move.MOVE_NONE) ? 1 : 0;
    }

    internal MovePicker(Position p, Move ttm, HistoryStats h, CounterMovesHistoryStats cmh, Value th)
    {
        endBadCaptures = new ExtMoveArrayWrapper(moves, _.MAX_MOVES - 1);
        cur = new ExtMoveArrayWrapper(moves);
        endMoves = new ExtMoveArrayWrapper(moves);

        pos = p;
        history = h;
        counterMovesHistory = cmh;
        threshold = th;

        Debug.Assert(!pos.checkers());

        stage = Stages.PROBCUT;

        // In ProbCut we generate captures with SEE higher than the given threshold
        ttMove = ttm != 0 && pos.pseudo_legal(ttm) && pos.capture(ttm)
                 && pos.see(ttm) > threshold
            ? ttm
            : Move.MOVE_NONE;

        endMoves += (ttMove != Move.MOVE_NONE) ? 1 : 0;
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
            var thisNum = moves[i];
            if (!maxVal.HasValue || thisNum > maxVal.Value)
            {
                maxVal = thisNum;
                index = i;
            }
        }

        var first = moves[begin.current];
        moves[begin.current] = moves[index];
        moves[index] = first;

        return moves[begin.current];
    }

    internal void score(GenType Type)
    {
        switch (Type)
        {
            case GenType.CAPTURES:
                score_CAPTURES();
                return;
            case GenType.EVASIONS:
                score_EVASIONS();
                return;
            case GenType.QUIETS:
                score_QUIETS();
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

        for (var i = 0; i < endMoves.current; i++)
        {
            var m = moves[i];
            moves[i] = new ExtMove(
                m,
                Value.PieceValue[(int) Phase.MG][pos.piece_on(Move.to_sq(m))]
                - new Value(200 * Rank.relative_rank(pos.side_to_move(), Move.to_sq(m))));
        }
    }

    private void score_QUIETS()
    {
        var prevSq = Move.to_sq(ss[ss.current - 1].currentMove);
        var cmh = counterMovesHistory.value(pos.piece_on(prevSq), prevSq);

        for (var i = 0; i < endMoves.current; i++)
        {
            var m = moves[i];
            moves[i] = new ExtMove(
                m,
                history.value(pos.moved_piece(m), Move.to_sq(m))
                + cmh.value(pos.moved_piece(m), Move.to_sq(m)));
        }
    }

    private void score_EVASIONS()
    {
        // Try winning and equal captures captures ordered by MVV/LVA, then non-captures
        // ordered by history value, then bad-captures and quiet moves with a negative
        // SEE ordered by SEE value.

        for (var i = 0; i < endMoves.current; i++)
        {
            var m = moves[i];
            Value see;
            if ((see = pos.see_sign(m)) < Value.VALUE_ZERO)
            {
                moves[i] = new ExtMove(m, see - HistoryStats.Max); // At the bottom
            }

            else if (pos.capture(m))
            {
                moves[i] = new ExtMove(
                    m,
                    Value.PieceValue[(int) Phase.MG][pos.piece_on(Move.to_sq(m))]
                    - new Value(Piece.type_of(pos.moved_piece(m))) + HistoryStats.Max);
            }
            else
            {
                moves[i] = new ExtMove(m, history.value(pos.moved_piece(m), Move.to_sq(m)));
            }
        }
    }

    /// generate_next_stage() generates, scores and sorts the next bunch of moves,
    /// when there are no more moves to try for the current stage.
    private void generate_next_stage()
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
                endMoves = Movegen.generate(GenType.CAPTURES, pos, new ExtMoveArrayWrapper(moves));
                score(GenType.CAPTURES);
            }
                break;

            case Stages.KILLERS:
                killers[0] = new ExtMove(ss[ss.current].killers0, killers[0].Value);
                killers[1] = new ExtMove(ss[ss.current].killers1, killers[1].Value);
                killers[2] = new ExtMove(countermove, killers[2].Value);
                cur.set(killers);
                endMoves = new ExtMoveArrayWrapper(cur.table, cur.current + 2
                                                              +
                                                              ((countermove != killers[0] && countermove != killers[1])
                                                                  ? 1
                                                                  : 0));
                break;

            case Stages.GOOD_QUIETS:
            {
                endQuiets = Movegen.generate(GenType.QUIETS, pos, new ExtMoveArrayWrapper(moves));
                endMoves = endQuiets;
                score(GenType.QUIETS);

                endMoves = ExtMoveArrayWrapper.Partition(cur, endMoves);
                ExtMoveArrayWrapper.insertion_sort(cur, endMoves);
            }
                break;

            case Stages.BAD_QUIETS:
                cur = new ExtMoveArrayWrapper(endMoves);
                endMoves = endQuiets;
                if (depth >= 3*Depth.ONE_PLY_C)
                {
                    ExtMoveArrayWrapper.insertion_sort(cur, endMoves);
                }
                break;

            case Stages.BAD_CAPTURES:
                // Just pick them in reverse order to get correct ordering
                cur = new ExtMoveArrayWrapper(moves) + (_.MAX_MOVES - 1);
                endMoves = endBadCaptures;
                break;

            case Stages.ALL_EVASIONS:
            {
                endMoves = Movegen.generate(GenType.EVASIONS, pos, new ExtMoveArrayWrapper(moves));

                if (endMoves.current > 1)
                {
                    score(GenType.EVASIONS);
                }
            }
                break;

            case Stages.CHECKS:
            {
                endMoves = Movegen.generate(
                    GenType.QUIET_CHECKS,
                    pos,
                    new ExtMoveArrayWrapper(moves));
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
    internal Move next_move(bool useSplitpoint)
    {
        // Version of next_move() to use at split point nodes where the move is grabbed
        // from the split point's shared MovePicker object. This function is not thread
        // safe so must be lock protected by the caller.
        if (useSplitpoint)
        {
            return ss[ss.current].splitPoint.movePicker.next_move(false);
        }

        while (true)
        {
            while (cur == endMoves && stage != Stages.STOP)
            {
                generate_next_stage();
            }

            Move move;
            switch (stage)
            {
                case Stages.MAIN_SEARCH:
                case Stages.EVASION:
                case Stages.QSEARCH_WITH_CHECKS:
                case Stages.QSEARCH_WITHOUT_CHECKS:
                case Stages.PROBCUT:
                    ++cur;
                    return ttMove;

                case Stages.GOOD_CAPTURES:
                    move = pick_best(cur, endMoves);
                    cur++;
                    if (move != ttMove)
                    {
                        if (pos.see_sign(move) >= Value.VALUE_ZERO)
                        {
                            return move;
                        }

                        // Losing capture, move it to the tail of the array
                        endBadCaptures.setCurrentMove(move);
                        --endBadCaptures;
                    }
                    break;

                case Stages.KILLERS:
                    move = cur.getCurrentMove();
                    cur++;
                    if (move != Move.MOVE_NONE && move != ttMove && pos.pseudo_legal(move)
                        && !pos.capture(move))
                    {
                        return move;
                    }
                    break;

                case Stages.GOOD_QUIETS:
                case Stages.BAD_QUIETS:
                    move = cur.getCurrentMove();
                    cur++;
                    if (move != ttMove && move != killers[0] && move != killers[1]
                        && move != killers[2])
                    {
                        return move;
                    }
                    break;

                case Stages.BAD_CAPTURES:
                {
                    var move2 = cur.getCurrentMove();
                    cur--;
                    return move2;
                }
                case Stages.ALL_EVASIONS:
                case Stages.QCAPTURES_1:
                case Stages.QCAPTURES_2:
                    move = pick_best(cur, endMoves);
                    cur++;
                    if (move != ttMove)
                    {
                        return move;
                    }
                    break;

                case Stages.PROBCUT_CAPTURES:
                    move = pick_best(cur, endMoves);
                    cur++;
                    if (move != ttMove && pos.see(move) > threshold)
                    {
                        return move;
                    }
                    break;

                case Stages.RECAPTURES:
                    move = pick_best(cur, endMoves);
                    cur++;
                    if (Move.to_sq(move) == recaptureSquare)
                    {
                        return move;
                    }
                    break;

                case Stages.CHECKS:
                    move = cur.getCurrentMove();
                    cur++;
                    if (move != ttMove)
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