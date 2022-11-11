namespace UCE.src;

enum Phase { OPENING, ENDGAME, MIDDLEGAME }

class Eval
{
    /*private static readonly int[] materialScore = {
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
    };*/
    private static readonly int[,] materialScore =
    {
        // opening material score
        { 82, 337, 365, 477, 1025, 12000, -82, -337, -365, -477, -1025, -12000 },

        // endgame material score
        { 94, 281, 297, 512,  936, 12000, -94, -281, -297, -512,  -936, -12000 }
    };

    private static readonly int[,,] positionalScore =
    {
        // Opening positional piece scores //
        {
            {
                0,   0,   0,   0,   0,   0,  0,   0,
                98, 134,  61,  95,  68, 126, 34, -11,
                -6,   7,  26,  31,  65,  56, 25, -20,
                -14,  13,   6,  21,  23,  12, 17, -23,
                -27,  -2,  -5,  12,  17,   6, 10, -25,
                -26,  -4,  -4, -10,   3,   3, 33, -12,
                -35,  -1, -20, -23, -15,  24, 38, -22,
                0,   0,   0,   0,   0,   0,  0,   0
            },
            // knight
            {
                -167, -89, -34, -49,  61, -97, -15, -107,
                -73, -41,  72,  36,  23,  62,   7,  -17,
                -47,  60,  37,  65,  84, 129,  73,   44,
                -9,  17,  19,  53,  37,  69,  18,   22,
                -13,   4,  16,  13,  28,  19,  21,   -8,
                -23,  -9,  12,  10,  19,  17,  25,  -16,
                -29, -53, -12,  -3,  -1,  18, -14,  -19,
                -105, -21, -58, -33, -17, -28, -19,  -23
            },
            // bishop
            {
                -29,   4, -82, -37, -25, -42,   7,  -8,
                -26,  16, -18, -13,  30,  59,  18, -47,
                -16,  37,  43,  40,  35,  50,  37,  -2,
                -4,   5,  19,  50,  37,  37,   7,  -2,
                -6,  13,  13,  26,  34,  12,  10,   4,
                0,  15,  15,  15,  14,  27,  18,  10,
                4,  15,  16,   0,   7,  21,  33,   1,
                -33,  -3, -14, -21, -13, -12, -39, -21
            },
            // rook
            {
                32,  42,  32,  51, 63,  9,  31,  43,
                27,  32,  58,  62, 80, 67,  26,  44,
                -5,  19,  26,  36, 17, 45,  61,  16,
                -24, -11,   7,  26, 24, 35,  -8, -20,
                -36, -26, -12,  -1,  9, -7,   6, -23,
                -45, -25, -16, -17,  3,  0,  -5, -33,
                -44, -16, -20,  -9, -1, 11,  -6, -71,
                -19, -13,   1,  17, 16,  7, -37, -26
            },
            // queen
            {
                28,   0,  29,  12,  59,  44,  43,  45,
                -24, -39,  -5,   1, -16,  57,  28,  54,
                -13, -17,   7,   8,  29,  56,  47,  57,
                -27, -27, -16, -16,  -1,  17,  -2,   1,
                -9, -26,  -9, -10,  -2,  -4,   3,  -3,
                -14,   2, -11,  -2,  -5,   2,  14,   5,
                -35,  -8,  11,   2,   8,  15,  -3,   1,
                -1, -18,  -9,  10, -15, -25, -31, -50
            },
            // king
            {
                -65,  23,  16, -15, -56, -34,   2,  13,
                29,  -1, -20,  -7,  -8,  -4, -38, -29,
                -9,  24,   2, -16, -20,   6,  22, -22,
                -17, -20, -12, -27, -30, -25, -14, -36,
                -49,  -1, -27, -39, -46, -44, -33, -51,
                -14, -14, -22, -46, -44, -30, -15, -27,
                1,   7,  -8, -64, -43, -16,   9,   8,
                -15,  36,  12, -54,   8, -28,  24,  14
            }
        },


    // Endgame positional piece scores //
        {
            //pawn
            {
                0,   0,   0,   0,   0,   0,   0,   0,
                178, 173, 158, 134, 147, 132, 165, 187,
                94, 100,  85,  67,  56,  53,  82,  84,
                32,  24,  13,   5,  -2,   4,  17,  17,
                13,   9,  -3,  -7,  -7,  -8,   3,  -1,
                4,   7,  -6,   1,   0,  -5,  -1,  -8,
                13,   8,   8,  10,  13,   0,   2,  -7,
                0,   0,   0,   0,   0,   0,   0,   0
            },
            // knight
            {
                -58, -38, -13, -28, -31, -27, -63, -99,
                -25,  -8, -25,  -2,  -9, -25, -24, -52,
                -24, -20,  10,   9,  -1,  -9, -19, -41,
                -17,   3,  22,  22,  22,  11,   8, -18,
                -18,  -6,  16,  25,  16,  17,   4, -18,
                -23,  -3,  -1,  15,  10,  -3, -20, -22,
                -42, -20, -10,  -5,  -2, -20, -23, -44,
                -29, -51, -23, -15, -22, -18, -50, -64
            },
            // bishop
            {
                -14, -21, -11,  -8, -7,  -9, -17, -24,
                -8,  -4,   7, -12, -3, -13,  -4, -14,
                2,  -8,   0,  -1, -2,   6,   0,   4,
                -3,   9,  12,   9, 14,  10,   3,   2,
                -6,   3,  13,  19,  7,  10,  -3,  -9,
                -12,  -3,   8,  10, 13,   3,  -7, -15,
                -14, -18,  -7,  -1,  4,  -9, -15, -27,
                -23,  -9, -23,  -5, -9, -16,  -5, -17
            },
            // rook
            {
                13, 10, 18, 15, 12,  12,   8,   5,
                11, 13, 13, 11, -3,   3,   8,   3,
                7,  7,  7,  5,  4,  -3,  -5,  -3,
                4,  3, 13,  1,  2,   1,  -1,   2,
                3,  5,  8,  4, -5,  -6,  -8, -11,
                -4,  0, -5, -1, -7, -12,  -8, -16,
                -6, -6,  0,  2, -9,  -9, -11,  -3,
                -9,  2,  3, -1, -5, -13,   4, -20
            },
            // queen
            {
                -9,  22,  22,  27,  27,  19,  10,  20,
                -17,  20,  32,  41,  58,  25,  30,   0,
                -20,   6,   9,  49,  47,  35,  19,   9,
                3,  22,  24,  45,  57,  40,  57,  36,
                -18,  28,  19,  47,  31,  34,  39,  23,
                -16, -27,  15,   6,   9,  17,  10,   5,
                -22, -23, -30, -16, -16, -23, -36, -32,
                -33, -28, -22, -43,  -5, -32, -20, -41
            },
            // king
            {
                -74, -35, -18, -18, -11,  15,   4, -17,
                -12,  17,  14,  17,  17,  38,  23,  11,
                10,  17,  23,  15,  20,  45,  44,  13,
                -8,  22,  24,  27,  26,  33,  26,   3,
                -18,  -4,  21,  24,  27,  23,   9, -11,
                -19,  -3,  11,  21,  23,  16,   7,  -9,
                -27, -11,   4,  13,  14,   4,  -5, -17,
                -53, -34, -21, -11, -28, -14, -24, -43
            }
        }
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

    private static readonly int[] DOUBLED_PAWN_PENALTY = { -5, -10 };
    private static readonly int[] ISOLATED_PAWN_PENALTY = { -5, -10 };
    private static readonly int[] PASSED_PAWN_BONUS = { 0, 10, 30, 50, 75, 100, 150, 200 };

    private static readonly int SEMI_OPEN_FILE_SCORE = 10;
    private static readonly int OPEN_FILE_SCORE = 15;

    private static readonly int KING_SHIELD_BONUS = 5;

    private static readonly int BISHOP_UNIT = 4;
    private static readonly int QUEEN_UNIT = 9;
    private static readonly int[] BISHOP_MOBILITY = { 5, 5 };
    private static readonly int[] QUEEN_MOBILITY = { 1, 2 };
    /*  These are opening and endgame bounds
        If the total material score > OPENING_PHASE_SCORE -> use pure opening material score
        If the total material score < ENDGAME_PHASE_SCORE -> use pure endgame material score
        If ENDGAME_PHASE_SCORE < total material score < OPENING_PHASE_SCORE -> interpolate the values based on how close they are to the bounds
    */
    private static readonly int OPENING_PHASE_SCORE = 6192;
    private static readonly int ENDGAME_PHASE_SCORE = 518;
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
        // Determine the phase of the game
        int phaseScore = GetPhaseScore(ref board);
        Phase phase;
        if (phaseScore >= OPENING_PHASE_SCORE)
            phase = Phase.OPENING;
        else if (phaseScore <= ENDGAME_PHASE_SCORE)
            phase = Phase.ENDGAME;
        else
            phase = Phase.MIDDLEGAME;

        ulong bitboard;
        int sq;
        int score = 0;
        int openingScore = 0, endgameScore = 0;

        // Penalties
        int doubledPawns = 0;
        for (int piece = 0; piece < 12; piece++)
        {
            // Create a copy of piece bitboards
            bitboard = board.bitPieces[piece];

            while (bitboard != 0)
            {
                sq = BitUtil.GetLs1bIndex(bitboard);
                // Material score based on game phase
                openingScore += materialScore[(int)Phase.OPENING, piece];
                endgameScore += materialScore[(int)Phase.ENDGAME, piece];

                // Positional scores
                switch (piece)
                {
                    // WHITE PAWNS
                    case 0: // PAWN
                        openingScore += positionalScore[(int)Phase.OPENING, 0, sq];
                        endgameScore += positionalScore[(int)Phase.ENDGAME, 0, sq];

                        // Doubled pawn penalty
                        doubledPawns = BitUtil.CountBits(board.bitPieces[0] & BoardUtil.fileMasks[sq]);
                        if (doubledPawns > 1)
                        {
                            openingScore += (doubledPawns - 1) * DOUBLED_PAWN_PENALTY[(int)Phase.OPENING];
                            endgameScore += (doubledPawns - 1) * DOUBLED_PAWN_PENALTY[(int)Phase.ENDGAME];
                        }
                        // Isolated pawns penalty
                        if ((board.bitPieces[0] & isolatedMasks[sq]) == 0)
                        {
                            openingScore += ISOLATED_PAWN_PENALTY[(int)Phase.OPENING];
                            endgameScore += ISOLATED_PAWN_PENALTY[(int)Phase.ENDGAME];
                        }
                        // Passed pawns bonus
                        if ((passedMasks[0, sq] & board.bitPieces[6]) == 0)
                        {
                            openingScore += PASSED_PAWN_BONUS[BoardUtil.GetRank(sq)];
                            endgameScore += PASSED_PAWN_BONUS[BoardUtil.GetRank(sq)];
                        }
                        break;
                    case 1: // KNIGHT
                        openingScore += positionalScore[(int)Phase.OPENING, 1, sq];
                        endgameScore += positionalScore[(int)Phase.ENDGAME, 1, sq];
                        break;
                    case 2: // BISHOP
                        openingScore += positionalScore[(int)Phase.OPENING, 2, sq];
                        endgameScore += positionalScore[(int)Phase.ENDGAME, 2, sq];

                        // Mobility
                        openingScore += (BitUtil.CountBits(Magics.GetBishopAttack(sq, board.bitAll)) - BISHOP_UNIT) * BISHOP_MOBILITY[(int)Phase.OPENING];
                        endgameScore += (BitUtil.CountBits(Magics.GetBishopAttack(sq, board.bitAll)) - BISHOP_UNIT) * BISHOP_MOBILITY[(int)Phase.ENDGAME];
                        break;
                    case 3: // ROOK
                        openingScore += positionalScore[(int)Phase.OPENING, 3, sq];
                        endgameScore += positionalScore[(int)Phase.ENDGAME, 3, sq];

                        // Open file or semi open file bonus
                        if ((board.bitPieces[0] & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore += SEMI_OPEN_FILE_SCORE;
                            endgameScore += SEMI_OPEN_FILE_SCORE;
                        }
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore += OPEN_FILE_SCORE;
                            endgameScore += OPEN_FILE_SCORE;
                        }
                        break;
                    case 4: // QUEEN
                        openingScore += positionalScore[(int)Phase.OPENING, 4, sq];
                        endgameScore += positionalScore[(int)Phase.ENDGAME, 4, sq];

                        // Mobility
                        openingScore += (BitUtil.CountBits(Magics.GetQueenAttack(sq, board.bitAll)) - QUEEN_UNIT) * QUEEN_MOBILITY[(int)Phase.OPENING];
                        endgameScore += (BitUtil.CountBits(Magics.GetQueenAttack(sq, board.bitAll)) - QUEEN_UNIT) * QUEEN_MOBILITY[(int)Phase.ENDGAME];
                        break;
                    case 5: // KING
                        openingScore += positionalScore[(int)Phase.OPENING, 5, sq];
                        endgameScore += positionalScore[(int)Phase.ENDGAME, 5, sq];

                        // Open file or semi open file penalty
                        if ((board.bitPieces[0] & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore -= SEMI_OPEN_FILE_SCORE;
                            endgameScore -= SEMI_OPEN_FILE_SCORE;
                        }
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore -= OPEN_FILE_SCORE;
                            endgameScore -= OPEN_FILE_SCORE;
                        }

                        // Mobility
                        openingScore += BitUtil.CountBits(Precalculate.kingAttacks[sq] & board.bitUnits[0]) * KING_SHIELD_BONUS;
                        endgameScore += BitUtil.CountBits(Precalculate.kingAttacks[sq] & board.bitUnits[0]) * KING_SHIELD_BONUS;
                        break;
                    // BLACK PIECES
                    case 6: // PAWN
                        openingScore -= positionalScore[(int)Phase.OPENING, 0, BoardUtil.flip[sq]];
                        endgameScore -= positionalScore[(int)Phase.ENDGAME, 0, BoardUtil.flip[sq]];

                        // Doubled pawn penalty
                        doubledPawns = BitUtil.CountBits(board.bitPieces[0] & BoardUtil.fileMasks[sq]);
                        if (doubledPawns > 1)
                        {
                            openingScore -= (doubledPawns - 1) * DOUBLED_PAWN_PENALTY[(int)Phase.OPENING];
                            endgameScore -= (doubledPawns - 1) * DOUBLED_PAWN_PENALTY[(int)Phase.ENDGAME];
                        }
                        // Isolated pawns penalty
                        if ((board.bitPieces[0] & isolatedMasks[sq]) == 0)
                        {
                            openingScore -= ISOLATED_PAWN_PENALTY[(int)Phase.OPENING];
                            endgameScore -= ISOLATED_PAWN_PENALTY[(int)Phase.ENDGAME];
                        }
                        // Passed pawns bonus
                        if ((passedMasks[1, sq] & board.bitPieces[0]) == 0)
                        {
                            openingScore -= PASSED_PAWN_BONUS[BoardUtil.GetRank(sq)];
                            endgameScore -= PASSED_PAWN_BONUS[BoardUtil.GetRank(sq)];
                        }
                        break;
                    case 7: // KNIGHT
                        openingScore -= positionalScore[(int)Phase.OPENING, 1, BoardUtil.flip[sq]];
                        endgameScore -= positionalScore[(int)Phase.ENDGAME, 1, BoardUtil.flip[sq]];
                        break;
                    case 8: // BISHOP
                        openingScore -= positionalScore[(int)Phase.OPENING, 2, BoardUtil.flip[sq]];
                        endgameScore -= positionalScore[(int)Phase.ENDGAME, 2, BoardUtil.flip[sq]];

                        // Mobility
                        openingScore -= (BitUtil.CountBits(Magics.GetBishopAttack(sq, board.bitAll)) - BISHOP_UNIT) * BISHOP_MOBILITY[(int)Phase.OPENING];
                        endgameScore -= (BitUtil.CountBits(Magics.GetBishopAttack(sq, board.bitAll)) - BISHOP_UNIT) * BISHOP_MOBILITY[(int)Phase.ENDGAME];
                        break;
                    case 9: // ROOK
                        openingScore -= positionalScore[(int)Phase.OPENING, 3, BoardUtil.flip[sq]];
                        endgameScore -= positionalScore[(int)Phase.ENDGAME, 3, BoardUtil.flip[sq]];

                        // Open file or semi open file bonus
                        if ((board.bitPieces[0] & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore -= SEMI_OPEN_FILE_SCORE;
                            endgameScore -= SEMI_OPEN_FILE_SCORE;
                        }
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore -= OPEN_FILE_SCORE;
                            endgameScore -= OPEN_FILE_SCORE;
                        }
                        break;
                    case 10: // QUEEN
                        openingScore -= positionalScore[(int)Phase.OPENING, 4, BoardUtil.flip[sq]];
                        endgameScore -= positionalScore[(int)Phase.ENDGAME, 4, BoardUtil.flip[sq]];

                        // Mobility
                        openingScore -= (BitUtil.CountBits(Magics.GetQueenAttack(sq, board.bitAll)) - QUEEN_UNIT) * QUEEN_MOBILITY[(int)Phase.OPENING];
                        endgameScore -= (BitUtil.CountBits(Magics.GetQueenAttack(sq, board.bitAll)) - QUEEN_UNIT) * QUEEN_MOBILITY[(int)Phase.ENDGAME];
                        break;
                    case 11: // KING
                        openingScore -= positionalScore[(int)Phase.OPENING, 5, BoardUtil.flip[sq]];
                        endgameScore -= positionalScore[(int)Phase.ENDGAME, 5, BoardUtil.flip[sq]];

                        // Open file or semi open file penalty
                        if ((board.bitPieces[0] & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore += SEMI_OPEN_FILE_SCORE;
                            endgameScore += SEMI_OPEN_FILE_SCORE;
                        }
                        if (((board.bitPieces[0] | board.bitPieces[6]) & BoardUtil.fileMasks[sq]) == 0)
                        {
                            openingScore += OPEN_FILE_SCORE;
                            endgameScore += OPEN_FILE_SCORE;
                        }

                        // Mobility
                        openingScore -= BitUtil.CountBits(Precalculate.kingAttacks[sq] & board.bitUnits[0]) * KING_SHIELD_BONUS;
                        endgameScore -= BitUtil.CountBits(Precalculate.kingAttacks[sq] & board.bitUnits[0]) * KING_SHIELD_BONUS;
                        break;
                }
                BitUtil.PopBit(ref bitboard, sq);
            }
        }

        // Score interpolation
        if (phase == Phase.MIDDLEGAME)
            score = (
                openingScore * phaseScore +
                endgameScore * (OPENING_PHASE_SCORE - phaseScore)
                ) / OPENING_PHASE_SCORE;

        else if (phase == Phase.OPENING) score = openingScore;
        else if (phase == Phase.ENDGAME) score = endgameScore;

        // Return score based on the side to move
        return (board.side == 0) ? score : -score;
    }

    private static int GetPhaseScore(ref Board board)
    {
        /*
            The game phase score of the game is derived from the pieces
            of both sides (not counting pawns and kings) that are still
            on the board. The full material starting position game phase
            score is:

            4 * knight material score in the opening +
            4 * bishop material score in the opening +
            4 * rook material score in the opening +
            2 * queen material score in the opening
        */
        int whitePieceScore = 0, blackPieceScore = 0;
        for (int piece = 1; piece <= 4; piece++)
        {
            whitePieceScore += BitUtil.CountBits(board.bitPieces[piece]) * materialScore[(int)Phase.OPENING, piece];
            blackPieceScore += BitUtil.CountBits(board.bitPieces[piece + 6]) * -materialScore[(int)Phase.OPENING, piece + 6];
        }
        return whitePieceScore + blackPieceScore;
    }
}
