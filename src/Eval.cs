namespace UCE.src;

class Eval
{
    private static readonly int[] materialScore = {
        100,      // white pawn score
        300,      // white knight scrore
        350,      // white bishop score
        500,      // white rook score
        1000,      // white queen score
        10000,      // white king score
        -100,      // black pawn score
        -300,      // black knight scrore
        -350,      // black bishop score
        -500,      // black rook score
        -1000,      // black queen score
        -10000,      // black king score
    };

    // pawn positional score
    private static readonly int[] pawnScores =
    {
        90,  90,  90,  90,  90,  90,  90,  90,
        30,  30,  30,  40,  40,  30,  30,  30,
        20,  20,  20,  30,  30,  30,  20,  20,
        10,  10,  10,  20,  20,  10,  10,  10,
         5,   5,  10,  20,  20,   5,   5,   5,
         0,   0,   0,   5,   5,   0,   0,   0,
         0,   0,   0, -10, -10,   0,   0,   0,
         0,   0,   0,   0,   0,   0,   0,   0
    };

    // knight positional score
    private static readonly int[] knightScores =
    {
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,  10,  10,   0,   0,  -5,
        -5,   5,  20,  20,  20,  20,   5,  -5,
        -5,  10,  20,  30,  30,  20,  10,  -5,
        -5,  10,  20,  30,  30,  20,  10,  -5,
        -5,   5,  20,  10,  10,  20,   5,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5, -10,   0,   0,   0,   0, -10,  -5
    };

    // bishop positional score
    private static readonly int[] bishopScores =
    {
     0,   0,   0,   0,   0,   0,   0,   0,
     0,   0,   0,   0,   0,   0,   0,   0,
     0,   0,   0,  10,  10,   0,   0,   0,
     0,   0,  10,  20,  20,  10,   0,   0,
     0,   0,  10,  20,  20,  10,   0,   0,
     0,  10,   0,   0,   0,   0,  10,   0,
     0,  30,   0,   0,   0,   0,  30,   0,
     0,   0, -10,   0,   0, -10,   0,   0

};

    // rook positional score
    private static readonly int[] rookScores =
    {
        50,  50,  50,  50,  50,  50,  50,  50,
        50,  50,  50,  50,  50,  50,  50,  50,
         0,   0,  10,  20,  20,  10,   0,   0,
         0,   0,  10,  20,  20,  10,   0,   0,
         0,   0,  10,  20,  20,  10,   0,   0,
         0,   0,  10,  20,  20,  10,   0,   0,
         0,   0,  10,  20,  20,  10,   0,   0,
         0,   0,   0,  20,  20,   0,   0,   0
    };

    // king positional score
    private static readonly int[] kingScores =
    {
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   5,   5,   5,   5,   0,   0,
        0,   5,   5,  10,  10,   5,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   0,   5,  10,  10,   5,   0,   0,
        0,   5,   5,  -5,  -5,   0,   5,   0,
        0,   0,   5,   0, -15,   0,  10,   0
    };

    public static readonly int[,] mvv_lva = {
        {105, 205, 305, 405, 505, 605},
        {104, 204, 304, 404, 504, 604},
        {103, 203, 303, 403, 503, 603},
        {102, 202, 302, 402, 502, 602},
        {101, 201, 301, 401, 501, 601},
        {100, 200, 300, 400, 500, 600},
    };

    private static ulong[] isolatedMasks = new ulong[64]; // square
    private static ulong[,] passedMasks = new ulong[2, 64]; // color, squares

    private static readonly int DOUBLED_PAWN_PENALTY = -10;
    private static readonly int ISOLATED_PAWN_PENALTY = -10;
    private static readonly int[] PASSED_PAWN_BONUS = { 0, 10, 30, 50, 75, 100, 150, 200 };

    private static readonly int SEMI_OPEN_FILE_SCORE = 10;
    private static readonly int OPEN_FILE_SCORE = 15;

    private static readonly int KING_SHIELD_BONUS = 5;

    public static void InitMasks()
    {
        // Init file and rank masks
        for (int r = 0; r < 8; r++)
        {
            for (int f = 0; f < 8; f++)
            {
                int sq = (r * 8) + f;

                isolatedMasks[sq] |= BoardUtil.SetFileAndRank(f - 1, -1);
                isolatedMasks[sq] |= BoardUtil.SetFileAndRank(f + 1, -1);

                passedMasks[0, sq] |= BoardUtil.SetFileAndRank(f - 1, -1);
                passedMasks[0, sq] |= BoardUtil.SetFileAndRank(f, -1);
                passedMasks[0, sq] |= BoardUtil.SetFileAndRank(f + 1, -1);

                for (int i = 0; i < (8 - r); i++)
                    passedMasks[0, sq] &= ~BoardUtil.rankMasks[(7 - i) * 8 + f];

                passedMasks[1, sq] |= BoardUtil.SetFileAndRank(f - 1, -1);
                passedMasks[1, sq] |= BoardUtil.SetFileAndRank(f, -1);
                passedMasks[1, sq] |= BoardUtil.SetFileAndRank(f + 1, -1);

                for (int i = 0; i < r + 1; i++)
                    passedMasks[1, sq] &= ~BoardUtil.rankMasks[i * 8 + f];
            }
        }
    }

    public static int Evaluate(ref Board board)
    {
        int score = 0;
        ulong bitboard;
        int sq;
        // Penalty
        int doubledPawns;
        for (int piece = 0; piece < 12; piece++)
        {
            // Create a copy of piece bitboards
            bitboard = board.bitPieces[piece];

            while (bitboard != 0)
            {
                sq = BitUtil.GetLs1bIndex(bitboard);
                score += materialScore[piece];
                switch (piece)
                {
                    case 0:
                        // Positional score
                        score += pawnScores[sq];
                        // Doubled pawn evaluation - penalty
                        doubledPawns = BitUtil.CountBits(bitboard);
                        if (doubledPawns > 1)
                            // Punish for each set of doubled pawns
                            score += doubledPawns * DOUBLED_PAWN_PENALTY;

                        // Isolated pawn evaluation - penalty
                        if ((board.bitPieces[0] & isolatedMasks[sq]) == 0)
                            score += ISOLATED_PAWN_PENALTY;

                        // Passed pawn evaluation - bonus
                        if ((board.bitPieces[6] & passedMasks[0, sq]) == 0)
                            score += PASSED_PAWN_BONUS[BoardUtil.ranks[sq]];
                        break;
                    case 1: score += knightScores[sq]; break;
                    case 2:
                        // Positional scores
                        score += bishopScores[sq];

                        // Mobility scores
                        score += BitUtil.CountBits(Magics.GetBishopAttack(sq, board.bitAll));
                        break;
                    case 3:
                        // Positional score
                        score += rookScores[sq];

                        if ((board.bitPieces[0] & BoardUtil.fileMasks[sq]) == 0)
                            // File is semi open, add semi open file bonus
                            score += SEMI_OPEN_FILE_SCORE;

                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                            // File is semi open, add semi open file bonus
                            score += OPEN_FILE_SCORE;
                        break;
                    case 4:
                        // Mobility scores
                        score += BitUtil.CountBits(Magics.GetQueenAttack(sq, board.bitAll));
                        break;
                    case 5:
                        // Positional score
                        score += kingScores[sq];
                        if ((board.bitPieces[0] & BoardUtil.fileMasks[sq]) == 0)
                            // File is semi open, add semi open file penalty
                            score -= SEMI_OPEN_FILE_SCORE;
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                            // File is open, add semi open file penalty
                            score -= OPEN_FILE_SCORE;
                        // King safety score
                        score += BitUtil.CountBits(Precalculate.kingAttacks[sq] & board.bitUnits[0]) * KING_SHIELD_BONUS;
                        break;

                    case 6:
                        // Positional score
                        score -= pawnScores[BoardUtil.flip[sq]];
                        // Doubled pawn evaluation - penalty
                        doubledPawns = BitUtil.CountBits(bitboard);
                        if (doubledPawns > 1)
                            // Punish for each set of doubled pawns
                            score -= doubledPawns * DOUBLED_PAWN_PENALTY;
                        // Isolated pawn evaluation - penalty
                        if ((board.bitPieces[6] & isolatedMasks[sq]) == 0)
                            score -= ISOLATED_PAWN_PENALTY;
                        // Passed pawn evaluation - bonus
                        if ((board.bitPieces[0] & passedMasks[1, sq]) == 0)
                            score -= PASSED_PAWN_BONUS[BoardUtil.ranks[BoardUtil.flip[sq]]];
                        break;
                    case 7: score -= knightScores[BoardUtil.flip[sq]]; break;
                    case 8:
                        // Positional scores
                        score -= bishopScores[BoardUtil.flip[sq]];
                        // Mobility scores
                        score -= BitUtil.CountBits(Magics.GetBishopAttack(sq, board.bitAll));
                        break;
                    case 9:
                        // Positional score
                        score -= rookScores[BoardUtil.flip[sq]];
                        if ((board.bitPieces[6] & BoardUtil.fileMasks[sq]) == 0)
                            // File is semi open, add semi open file bonus
                            score -= SEMI_OPEN_FILE_SCORE;
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                            // File is semi open, add semi open file bonus
                            score -= OPEN_FILE_SCORE;
                        break;
                    case 10:
                        // Mobility scores
                        score -= BitUtil.CountBits(Magics.GetQueenAttack(sq, board.bitAll));
                        break;
                    case 11:
                        // Positional score
                        score -= kingScores[BoardUtil.flip[sq]];
                        if ((board.bitPieces[6] & BoardUtil.fileMasks[sq]) == 0)
                            // File is semi open, add semi open file penalty
                            score += SEMI_OPEN_FILE_SCORE;
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                            // File is open, add semi open file penalty
                            score += OPEN_FILE_SCORE;
                        // King safety score
                        score -= BitUtil.CountBits(Precalculate.kingAttacks[sq] & board.bitUnits[1]) * KING_SHIELD_BONUS;
                        break;
                }
                BitUtil.PopBit(ref bitboard, sq);
            }
        }
        return (board.side == 0) ? score : -score;
    }
}
