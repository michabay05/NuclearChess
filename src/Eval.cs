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

    // mirror positional score tables for opposite side
    private static readonly int[] flip =
    {
        56, 57, 58, 59, 60, 61, 62, 63,
        48, 49, 50, 51, 52, 53, 54, 55,
        40, 41, 42, 43, 44, 45, 46, 47,
        32, 33, 34, 35, 36, 37, 38, 39,
        24, 25, 26, 27, 28, 29, 30, 31,
        16, 17, 18, 19, 20, 21, 22, 23,
         8,  9, 10, 11, 12, 13, 14, 15,
         0,  1,  2,  3,  4,  5,  6,  7
    };

    public static readonly int[,] mvv_lva = {
        {105, 205, 305, 405, 505, 605},
        {104, 204, 304, 404, 504, 604},
        {103, 203, 303, 403, 503, 603},
        {102, 202, 302, 402, 502, 602},
        {101, 201, 301, 401, 501, 601},
        {100, 200, 300, 400, 500, 600},
    };

    public static int Evaluate(ref Board board)
    {
        int score = 0;
        ulong bitboard;
        int square;
        for (int piece = 0; piece < 12; piece++)
        {
            // Create a copy of piece bitboards
            bitboard = board.bitPieces[piece];

            while (bitboard != 0)
            {
                square = BitUtil.GetLs1bIndex(bitboard);
                score += materialScore[piece];
                switch (piece)
                {
                    case 0: score += pawnScores[square]; break;
                    case 1: score += knightScores[square]; break;
                    case 2: score += bishopScores[square]; break;
                    case 3: score += rookScores[square]; break;
                    case 5: score += kingScores[square]; break;

                    case 6: score -= pawnScores[flip[square]]; break;
                    case 7: score -= knightScores[flip[square]]; break;
                    case 8: score -= bishopScores[flip[square]]; break;
                    case 9: score -= rookScores[flip[square]]; break;
                    case 11: score -= kingScores[flip[square]]; break;
                }
                BitUtil.PopBit(ref bitboard, square);
            }
        }
        return (board.side == 0) ? score : -score;
    }
}
