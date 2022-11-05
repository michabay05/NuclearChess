namespace UCE.src;


class Search
{
    static readonly int FULL_DEPTH_MOVES = 4;
    static readonly int REDUCTION_LIMIT = 3;
    static readonly int MAX_PLY = 64;
    // half move counter
    static int ply, nodes;

    // PV flags
    static bool followPV, scorePV;

    // Killer moves: Moves that cause beta-cuttoffs and are quiet moves
    static int[,] killerMoves = new int[2, MAX_PLY]; // [id, ply]
    static int[,] historyMoves = new int[12, 64]; // [piece, square]
    static int[] pvLength = new int[MAX_PLY];
    static int[,] pvTable = new int[MAX_PLY, MAX_PLY];

    public static void SearchMove(ref Board board, int depth)
    {
        int score = 0;
        // Reset arrays with garbage values
        killerMoves = new int[2, MAX_PLY]; // [id, ply]
        historyMoves = new int[12, 64]; // [piece, square]
        pvLength = new int[MAX_PLY];
        pvTable = new int[MAX_PLY, MAX_PLY];

        followPV = false;
        scorePV = false;
        nodes = 0;

        // Iterative deepening
        for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
        {
            followPV = true;
            score = Negamax(ref board, -50000, 50000, currentDepth);
            Console.Write($"info score cp {score} depth {currentDepth} nodes {nodes} pv ");
            for (int i = 0; i < pvLength[0]; i++)
                Console.Write(Move.ToString(pvTable[0, i]) + " ");
            Console.WriteLine();
        }
        Console.WriteLine($"bestmove {Move.ToString(pvTable[0, 0])}");
    }

    // negamax alpha beta search
    private static int Negamax(ref Board board, int alpha, int beta, int depth)
    {
        bool foundPV = false;
        pvLength[ply] = ply;
        // Escape condition
        if (depth == 0)
            return Quiescence(ref board, alpha, beta);

        // Exit if ply > max ply; ply should be <= 63
        if (ply > MAX_PLY - 1)
            return Eval.Evaluate(ref board);

        nodes++;

        bool isInCheck = MoveGen.IsSquareAttacked(board.side ^ 1, BitUtil.GetLs1bIndex(board.bitPieces[(board.side == 0) ? 5 : 11]), board);
        // increase search depth if the king has been exposed into a check
        if (isInCheck) depth++;

        int legalMovesCount = 0;

        if (depth >= 3 && !isInCheck && ply != 0)
        {
            Board anotherBoardCopy = Board.Clone(board);
            // Give opponent an extra move; 2 moves in one turn
            anotherBoardCopy.side ^= 1;
            anotherBoardCopy.enPassant = -1;
            // Search move with reduced depth to find beta-cutoffs
            int score = -Negamax(ref board, -beta, -beta + 1, depth - 1 - 2);
            // Fail hard; beta-cutoffs
            if (score >= beta)
                return beta;
            Board.Restore(ref board, anotherBoardCopy);
        }

        // Generate and sort moves to decrease # of nodes searched
        MoveList moveList = new MoveList(board);
        MoveGen.Generate(ref moveList);
        if (followPV)
            EnablePVScoring(moveList);
        SortMoves(ref moveList);

        Board copy;
        int movesSearched = 0;
        for (int i = 0; i < moveList.count; i++)
        {
            // preserve board state
            copy = Board.Clone(board);

            ply++;

            // make sure to make only legal moves
            if (!BoardUtil.MakeMove(ref board, moveList.list[i], MoveType.allMoves))
            {
                // decrement ply
                ply--;
                continue;
            }

            // Increment legal moves
            legalMovesCount++;

            // Null move pruning
            int score;

            /* Source:
            https://web.archive.org/web/20071030220825/http://www.brucemo.com/compchess/programming/pvs.htm */
            if (foundPV)
            {
                score = -Negamax(ref board, -alpha - 1, -alpha, depth - 1);

                if ((score > alpha) && (score < beta)) // Check for failure.
                    score = -Negamax(ref board, -beta, -alpha, depth - 1);
            }
            else
            {
                if (movesSearched == 0)
                    score = -Negamax(ref board, -alpha - 1, -alpha, depth - 1);
                else
                {
                    if (movesSearched >= FULL_DEPTH_MOVES && depth >= REDUCTION_LIMIT &&
                        !isInCheck && Move.GetPromoted(moveList.list[i]) == ' ' && !Move.IsCapture(moveList.list[i]))
                        score = -Negamax(ref board, -alpha - 1, -alpha, depth - 2);
                    else
                        score = alpha + 1;

                    if (score > alpha)
                    {
                        // re-search at full depth but with narrowed score bandwith
                        score = -Negamax(ref board, -alpha - 1, -alpha, depth - 1);

                        // if LMR fails re-search at full depth and full score bandwith
                        if ((score > alpha) && (score < beta))
                            score = -Negamax(ref board, -beta, -alpha, depth - 1);
                    }
                }

            }

            ply--;

            Board.Restore(ref board, copy);

            movesSearched++;
            // fail-hard beta cutoff
            if (score >= beta)
            {
                if (!Move.IsCapture(moveList.list[i]))
                {
                    // Store killer moves
                    killerMoves[1, ply] = killerMoves[0, ply];
                    killerMoves[0, ply] = moveList.list[i];
                }
                // node (move) fails high
                return beta;
            }

            // found a better move
            if (score > alpha)
            {
                if (!Move.IsCapture(moveList.list[i]))
                    // Store history moves
                    historyMoves[Move.GetPiece(moveList.list[i]), Move.GetTarget(moveList.list[i])] += depth;

                // PV node (move)
                alpha = score;

                // Enable foundPV flag
                foundPV = true;

                // Write PV moves
                pvTable[ply, ply] = moveList.list[i];

                // Copy PV from following plies
                for (int nextPly = ply + 1; nextPly < pvLength[ply + 1]; nextPly++)
                    pvTable[ply, nextPly] = pvTable[ply + 1, nextPly];

                // Adjust PV length
                pvLength[ply] = pvLength[ply + 1];
            }
        }

        // If no legal moves are available
        if (legalMovesCount == 0)
        {
            // Possible mate
            if (isInCheck)
                // Mating score
                return -49000 + ply;
            // Stalemate
            else
                return 0;
        }

        // node (move) fails low
        return alpha;
    }

    private static int Quiescence(ref Board board, int alpha, int beta)
    {
        nodes++;

        // Escape condition - fail-hard beta cutoff
        int eval = Eval.Evaluate(ref board);
        if (eval >= beta)
            // node (move) fails high
            return beta;

        // found a better move
        if (eval > alpha)
            // PV node (move)
            alpha = eval;

        MoveList moveList = new MoveList(board);
        MoveGen.Generate(ref moveList);
        SortMoves(ref moveList);

        Board copy;
        for (int count = 0; count < moveList.count; count++)
        {
            // preserve board state
            copy = Board.Clone(board);

            ply++;

            // make sure to make only legal moves
            if (!BoardUtil.MakeMove(ref board, moveList.list[count], MoveType.onlyCaptures))
            {
                ply--;
                continue;
            }

            // score current move
            int score = -Quiescence(ref board, -beta, -alpha);

            ply--;

            Board.Restore(ref board, copy);

            // fail-hard beta cutoff
            if (score >= beta)
                // node (move) fails high
                return beta;

            // found a better move
            if (score > alpha)
                // PV node (move)
                alpha = score;

        }
        return alpha;
    }

    private static int ScoreMove(Board board, int move)
    {
        if (scorePV)
        {
            // Check if move on current ply is a PV move
            if (pvTable[0, ply] == move)
            {
                scorePV = false;
                return 20000;
            }
        }

        if (Move.IsCapture(move))
        {
            // Set to a pawn by default; for enpassant
            int captured = 0;
            int targetSquare = Move.GetTarget(move);

            // Get opponent piece start and end range
            int start, end;
            if (board.side == 0) { start = 6; end = 11; }
            else { start = 0; end = 5; }

            for (int i = start; i <= end; i++)
            {
                if (BitUtil.GetBit(board.bitPieces[i], targetSquare))
                {
                    captured = i;
                    break;
                }
            }
            // Add 10,000 to ensure captures are evaluated before killer moves
            return Eval.mvv_lva[Move.GetPiece(move), captured % 6] + 10000;
        }
        else
        {
            // Score the best killer move
            if (killerMoves[0, ply] == move)
                return 9000;
            // Score 2nd best killer move
            else if (killerMoves[1, ply] == move)
                return 8000;
            else // Score history move
                return historyMoves[Move.GetPiece(move), Move.GetTarget(move)];
        }
    }

    public static void SortMoves(ref MoveList moveList)
    {
        int[] moveScoresList = new int[moveList.count];
        // Init moveScoresList with the score of all the moves in the move list
        for (int i = 0; i < moveList.count; i++)
            moveScoresList[i] = ScoreMove(moveList.boardRef, moveList.list[i]);

        // Sort moves and their scores in descending order
        for (int i = 0; i < moveList.count; i++)
        {
            for (int j = i + 1; j < moveScoresList.Length; j++)
            {
                if (moveScoresList[i] < moveScoresList[j])
                {
                    // Swap scores
                    int temp = moveScoresList[i];
                    moveScoresList[i] = moveScoresList[j];
                    moveScoresList[j] = temp;
                    // Swap moves
                    temp = moveList.list[i];
                    moveList.list[i] = moveList.list[j];
                    moveList.list[j] = temp;
                }
            }
        }
    }

    private static void EnablePVScoring(MoveList moveList)
    {
        // Stop following PV after following it once
        followPV = false;

        for (int i = 0; i < moveList.count; i++)
        {
            if (pvTable[0, ply] == moveList.list[i])
            {
                scorePV = true;
                followPV = true;
            }
        }
    }
}
