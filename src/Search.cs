namespace Nuclear.src;


class Search
{
    private static readonly int FULL_DEPTH_MOVES = 4;
    private static readonly int REDUCTION_LIMIT = 3;
    public static readonly int MAX_PLY = 64;

    // Mating score bounds
    private static readonly int INFINITY = 50000;
    public static readonly int MATE_VALUE = 49000; // Upper bound
    public static readonly int MATE_SCORE = 48000; // Lower bound
    /*
        Score layout
        -INFINITY < -MATE_VALUE < -MATE_SCORE < NORMAL (non-mating) score < MATE_SCORE < MATE_VALUE < INFINITY
     */

    // half move counter
    public static int ply, nodes;

    // PV flags
    private static bool followPV, scorePV;

    // Killer moves: Moves that cause beta-cuttoffs and are quiet moves
    private static int[,] killerMoves = new int[2, MAX_PLY]; // [id, ply]
    private static int[,] historyMoves = new int[12, 64]; // [piece, square]
    private static int[] pvLength = new int[MAX_PLY];
    public static int[,] pvTable = new int[MAX_PLY, MAX_PLY];

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

        UCI.Stop = false;

        // Initial alpha & beta bounds
        int alpha = -INFINITY, beta = INFINITY;
        // Iterative deepening
        for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
        {
            if (UCI.Stop)
                break;
            followPV = true;
            // Find best move in position
            score = Negamax(ref board, alpha, beta, currentDepth);

            // Aspiration window
            if ((score <= alpha) || (score >= beta))
            {
                alpha = -INFINITY;
                beta = INFINITY;
                continue;
            }
            // Set up the window for the next iteration
            alpha = score - 50;
            beta = score + 50;

            if (pvLength[0] != 0)
            {
                // Print information about current depth
                if (score > -MATE_VALUE && score < -MATE_SCORE)
                    Console.Write($"info score mate {-(score + MATE_VALUE) / 2} depth {currentDepth} nodes {nodes} pv ");
                else if (score > MATE_SCORE && score < MATE_VALUE)
                    Console.Write($"info score mate {(MATE_VALUE - score) / 2} depth {currentDepth} nodes {nodes} pv ");
                else
                    Console.Write($"info score cp {score} depth {currentDepth} nodes {nodes} pv ");

                for (int i = 0; i < pvLength[0]; i++)
                    Console.Write(Move.ToString(pvTable[0, i]) + " ");
                Console.WriteLine();
            }
        }
        Console.WriteLine($"bestmove {Move.ToString(pvTable[0, 0])}");
    }

    // negamax alpha beta search
    private static int Negamax(ref Board board, int alpha, int beta, int depth)
    {
        pvLength[ply] = ply;
        // Stores current move's score
        int score;
        int hashFlag = TT.F_HASH_ALPHA;

        // If position has been repeated
        if (ply > 0 && isRepetition(ref board))
            // Return draw score
            return 0;


        bool isPVNode = (beta - alpha) > 1;

        // If score of current position exists, return score instead of searching
        // Reads hash entry if not root ply, score for current position exists and isn't pv node
        if (ply > 0 && (score = TTUtil.ReadEntry(ref board, alpha, beta, depth)) != TTUtil.NO_HASH_ENTRY && !isPVNode)
            return score;

        // every 2047 nodes
        if ((nodes & 2047) == 0)
            // "listen" to the GUI/user input
            UCI.Communicate();

        // Escape condition
        if (depth == 0)
            return Quiescence(ref board, alpha, beta);

        // Exit if ply > max ply; ply should be <= 63
        if (ply > MAX_PLY - 1)
            return Eval.Evaluate(ref board);

        // Increment node count
        nodes++;

        bool isInCheck = MoveGen.IsSquareAttacked(board.side ^ 1, BitUtil.GetLs1bIndex(board.bitPieces[(board.side == 0) ? 5 : 11]), board);
        // increase search depth if the king has been exposed into a check
        if (isInCheck) depth++;

        int legalMovesCount = 0;

        // NULL move pruning
        if (depth >= 3 && !isInCheck && ply != 0)
        {
            Board anotherBoardCopy = Board.Clone(board);
            ply++;
            // Increment repetition
            board.repetitionIndex++;
            board.repetitionTable[board.repetitionIndex] = board.hashKey;
            // Hash enpassant, if available
            if (board.enPassant != -1) board.hashKey ^= Zobrist.enPassantKeys[board.enPassant];
            board.enPassant = -1;

            // Give opponent an extra move; 2 moves in one turn
            board.side ^= 1;
            // Hash side to move 
            board.hashKey ^= Zobrist.sideKey;

            // Search move with reduced depth to find beta-cutoffs
            score = -Negamax(ref board, -beta, -beta + 1, depth - 1 - 2);
            ply--;
            // Decrement repetition
            board.repetitionIndex--;
            Board.Restore(ref board, anotherBoardCopy);
            // When timer runs out, return 0;
            if (UCI.Stop) return 0;
            // Fail hard; beta-cutoffs
            if (score >= beta)
                return beta;
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

            // Increment repetition
            board.repetitionIndex++;
            board.repetitionTable[board.repetitionIndex] = board.hashKey;

            // Make sure to make only legal moves
            if (!Board.MakeMove(ref board, moveList.list[i], MoveType.allMoves))
            {
                // Decrement ply
                ply--;
                // Decrement repetition
                board.repetitionIndex--;
                continue;
            }

            // Increment legal moves
            legalMovesCount++;

            // Full depth search
            if (movesSearched == 0)
                // Do normal alpha-beta search
                score = -Negamax(ref board, -beta, -alpha, depth - 1);
            else // Late move reduction (LMR)
            {
                if (movesSearched >= FULL_DEPTH_MOVES && depth >= REDUCTION_LIMIT &&
                    !isInCheck && Move.GetPromoted(moveList.list[i]) == ' ' && !Move.IsCapture(moveList.list[i]))
                    score = -Negamax(ref board, -alpha - 1, -alpha, depth - 2);
                else
                    // Hack to ensure full depth search is done
                    score = alpha + 1;

                // PVS (Principal Variation Search)
                if (score > alpha)
                {
                    // re-search at full depth but with narrowed score bandwith
                    score = -Negamax(ref board, -alpha - 1, -alpha, depth - 1);

                    // if LMR fails re-search at full depth and full score bandwith
                    if ((score > alpha) && (score < beta))
                        score = -Negamax(ref board, -beta, -alpha, depth - 1);
                }
            }

            ply--;
            // Decrement repetition
            board.repetitionIndex--;

            Board.Restore(ref board, copy);

            // When timer runs out, return 0;
            if (UCI.Stop) return 0;

            movesSearched++;

            // found a better move
            if (score > alpha)
            {
                // Switch flag to EXACT(PV node) from ALPHA (fail-low node)
                hashFlag = TT.F_HASH_EXACT;

                if (!Move.IsCapture(moveList.list[i]))
                    // Store history moves
                    historyMoves[Move.GetPiece(moveList.list[i]), Move.GetTarget(moveList.list[i])] += depth;

                // PV node (move)
                alpha = score;

                // Write PV moves
                pvTable[ply, ply] = moveList.list[i];

                // Copy PV from following plies
                for (int nextPly = ply + 1; nextPly < pvLength[ply + 1]; nextPly++)
                    pvTable[ply, nextPly] = pvTable[ply + 1, nextPly];

                // Adjust PV length
                pvLength[ply] = pvLength[ply + 1];

                // fail-hard beta cutoff
                if (score >= beta)
                {
                    // Store hash entry with score equal to beta
                    TTUtil.WriteEntry(ref board, depth, beta, TT.F_HASH_BETA);

                    if (!Move.IsCapture(moveList.list[i]))
                    {
                        // Store killer moves
                        killerMoves[1, ply] = killerMoves[0, ply];
                        killerMoves[0, ply] = moveList.list[i];
                    }
                    // node (move) fails high
                    return beta;
                }
            }
        }

        // If no legal moves are available
        if (legalMovesCount == 0)
        {
            // Possible mate
            if (isInCheck)
                // Mating score
                // if 49000 is returned, mate is on the board
                // if not, there are ply number of moves before mate is on the board 
                return -MATE_VALUE + ply;
            // Stalemate
            else
                return 0;
        }
        // Store hash entry with score equal to alpha
        TTUtil.WriteEntry(ref board, depth, alpha, hashFlag);

        // node (move) fails low
        return alpha;
    }

    private static int Quiescence(ref Board board, int alpha, int beta)
    {
        // every 2047 nodes
        if ((nodes & 2047) == 0)
            // "listen" to the GUI/user input
            UCI.Communicate();

        nodes++;

        // Escape condition - fail-hard beta cutoff
        int eval = Eval.Evaluate(ref board);
        // Exit if ply > max ply; ply should be <= 63
        if (ply > MAX_PLY - 1)
            return eval;

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

            // Increment repetition
            board.repetitionIndex++;
            board.repetitionTable[board.repetitionIndex] = board.hashKey;

            // make sure to make only legal moves
            if (!Board.MakeMove(ref board, moveList.list[count], MoveType.onlyCaptures))
            {
                ply--;
                // Decrement repetition
                board.repetitionIndex--;
                continue;
            }

            // score current move
            int score = -Quiescence(ref board, -beta, -alpha);

            ply--;
            // Decrement repetition
            board.repetitionIndex--;

            Board.Restore(ref board, copy);

            // When timer runs out, return 0;
            if (UCI.Stop) return 0;

            // found a better move
            if (score > alpha)
            {
                // PV node (move)
                alpha = score;

                // fail-hard beta cutoff
                if (score >= beta)
                    // node (move) fails high
                    return beta;
            }
        }
        return alpha;
    }

    private static bool isRepetition(ref Board board)
    {
        for (int i = 0; i < board.repetitionIndex; i++)
        {
            if (board.repetitionTable[i] == board.hashKey)
                // Found a repetition
                return true;
        }
        // If no repetition found, return false
        return false;
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
