namespace UCE.src;

class MoveGen
{
    static Board genBoard = new Board();
    /* Returns true if a square is attacked by specified color,
     * returns false if not */
    public static bool IsSquareAttacked(int side, int sq, Board board)
    {
        // Attacked by white pawns
        if ((side == 0) && ((Precalculate.pawnAttacks[1, sq] & board.bitPieces[0]) != 0))
            return true;
        // Attacked by black pawns
        if ((side == 1) && ((Precalculate.pawnAttacks[0, sq] & board.bitPieces[6]) != 0))
            return true;
        // Attacked by knights
        if ((Precalculate.knightAttacks[sq] & ((side == 0) ? board.bitPieces[1] : board.bitPieces[7])) != 0)
            return true;
        // Attacked by bishops
        if ((Magics.GetBishopAttack(sq, board.bitAll) & ((side == 0) ? board.bitPieces[2] : board.bitPieces[8])) != 0)
            return true;
        // Attacked by rooks
        if ((Magics.GetRookAttack(sq, board.bitAll) & ((side == 0) ? board.bitPieces[3] : board.bitPieces[9])) != 0)
            return true;
        // Attacked by queens
        if ((Magics.GetQueenAttack(sq, board.bitAll) & ((side == 0) ? board.bitPieces[4] : board.bitPieces[10])) != 0)
            return true;
        // Attacked by kings
        if ((Precalculate.kingAttacks[sq] & ((side == 0) ? board.bitPieces[5] : board.bitPieces[11])) != 0)
            return true;

        // If all these conditions fail, `sq` is not attacked.
        return false;
    }

    /* Debugging method: prints all the attacked square for a color */
    public static void PrintAttacked(int side, ref Board board)
    {
        for (int r = 0; r < 8; r++)
        {
            Console.Write($" {8 - r} |");
            for (int f = 0; f < 8; f++)
            {
                int sq = (r * 8) + f;
                if (IsSquareAttacked(side, sq, board))
                    Console.Write(" 1");
                else
                    Console.Write(" 0");
            }
            Console.WriteLine();
        }
        Console.WriteLine("     - - - - - - - -\n     a b c d e f g h\n");
    }

    /* Generates moves for both sides - depends on which side's turn it is */
    public static void Generate(ref MoveList moveList)
    {
        genBoard = Board.Clone(moveList.boardRef);
        // Generate moves for each piece type and color
        GenPawnMoves(ref moveList);
        GenKnightMoves(ref moveList);
        GenBishopMoves(ref moveList);
        GenRookMoves(ref moveList);
        GenQueenMoves(ref moveList);
        GenKingMoves(ref moveList);
    }

    /* Generates pawn moves */
    private static void GenPawnMoves(ref MoveList moveList)
    {
        int enemyRankStart, promotionStart, promotionEnd, directionToMove, doublePawnStart, doublePawnEnd;
        int source, target;
        ulong bitboard, attack;
        if (genBoard.side == 0) // WHITE PAWN
        {
            enemyRankStart = (int)Squares.a8;
            promotionStart = (int)Squares.a7;
            promotionEnd = (int)Squares.h7;
            directionToMove = (int)Direction.S;
            doublePawnStart = (int)Squares.a2;
            doublePawnEnd = (int)Squares.h2;
            bitboard = genBoard.bitPieces[0];
        }
        else // BLACK PAWN
        {
            enemyRankStart = (int)Squares.h1;
            promotionStart = (int)Squares.a2;
            promotionEnd = (int)Squares.h2;
            directionToMove = (int)Direction.N;
            doublePawnStart = (int)Squares.a7;
            doublePawnEnd = (int)Squares.h7;
            bitboard = genBoard.bitPieces[6];
        }

        // Quiet Pawn Moves (Non-capture moves)
        while (bitboard != 0)
        {
            source = BitUtil.GetLs1bIndex(bitboard);
            target = source + directionToMove;

            if ((genBoard.side == 0 ? target >= enemyRankStart : target <= enemyRankStart) && !BitUtil.GetBit(genBoard.bitAll, target))
            {
                // Promotion
                if (source >= promotionStart && source <= promotionEnd)
                {
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'q', 0, 0, 0, 0));
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'r', 0, 0, 0, 0));
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'b', 0, 0, 0, 0));
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'n', 0, 0, 0, 0));
                }
                else
                {
                    // One square push
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, ' ', 0, 0, 0, 0));
                    // Double square push
                    if ((source >= doublePawnStart && source <= doublePawnEnd) && !BitUtil.GetBit(genBoard.bitAll, target + directionToMove))
                        moveList.Add(Move.Encode(source, target + directionToMove, Piece.PAWN, ' ', 0, 1, 0, 0));
                }
            }

            // Stores opposite color occupied squares attacked by current side
            attack = Precalculate.pawnAttacks[genBoard.side, source] & genBoard.bitUnits[genBoard.side ^ 1];

            // Generate pawn capture moves
            while (attack != 0)
            {
                target = BitUtil.GetLs1bIndex(attack);

                // Capture Promotion
                if (source >= promotionStart && source <= promotionEnd)
                {
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'q', 1, 0, 0, 0));
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'r', 1, 0, 0, 0));
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'b', 1, 0, 0, 0));
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, 'n', 1, 0, 0, 0));
                }
                else
                    // Normal capture
                    moveList.Add(Move.Encode(source, target, Piece.PAWN, ' ', 1, 0, 0, 0));

                // Remove bit to move onto other squares
                BitUtil.PopBit(ref attack, target);
            }

            // If there is an enpassant capture
            if (genBoard.enPassant != -1)
            {
                ulong enPassCapture = Precalculate.pawnAttacks[genBoard.side, source] & (1UL << genBoard.enPassant);
                // Validate if there's an enpassant capture
                if (enPassCapture != 0)
                {
                    int enPassTarget = BitUtil.GetLs1bIndex(enPassCapture);
                    moveList.Add(Move.Encode(source, enPassTarget, Piece.PAWN, ' ', 1, 0, 1, 0));
                }
            }

            // Remove bit to move onto other squares
            BitUtil.PopBit(ref bitboard, source);
        }
    }

    private static void GenKnightMoves(ref MoveList moveList)
    {
        int source, target;
        ulong bitboard, attack;
        bitboard = genBoard.bitPieces[genBoard.side == 0 ? 1 : 7];
        while (bitboard != 0)
        {
            source = BitUtil.GetLs1bIndex(bitboard);

            attack = Precalculate.knightAttacks[source] & (genBoard.side == 0 ? ~genBoard.bitUnits[0] : ~genBoard.bitUnits[1]);
            while (attack != 0)
            {
                target = BitUtil.GetLs1bIndex(attack);

                if (BitUtil.GetBit((genBoard.side == 0 ? genBoard.bitUnits[1] : genBoard.bitUnits[0]), target))
                    moveList.Add(Move.Encode(source, target, Piece.KNIGHT, ' ', 1, 0, 0, 0));
                else
                    moveList.Add(Move.Encode(source, target, Piece.KNIGHT, ' ', 0, 0, 0, 0));

                // Remove `target` bit
                BitUtil.PopBit(ref attack, target);
            }

            // Remove bit to move onto other bits
            BitUtil.PopBit(ref bitboard, source);
        }
    }

    private static void GenBishopMoves(ref MoveList moveList)
    {
        int source, target;
        ulong bitboard, attack;
        bitboard = genBoard.bitPieces[genBoard.side == 0 ? 2 : 8];
        while (bitboard != 0)
        {
            source = BitUtil.GetLs1bIndex(bitboard);

            attack = Magics.GetBishopAttack(source, genBoard.bitAll) & (genBoard.side == 0 ? ~genBoard.bitUnits[0] : ~genBoard.bitUnits[1]);
            while (attack != 0)
            {
                target = BitUtil.GetLs1bIndex(attack);

                if (BitUtil.GetBit((genBoard.side == 0 ? genBoard.bitUnits[1] : genBoard.bitUnits[0]), target))
                    moveList.Add(Move.Encode(source, target, Piece.BISHOP, ' ', 1, 0, 0, 0));
                else
                    moveList.Add(Move.Encode(source, target, Piece.BISHOP, ' ', 0, 0, 0, 0));

                // Remove `target` bit
                BitUtil.PopBit(ref attack, target);
            }

            // Remove bit to move onto other bits
            BitUtil.PopBit(ref bitboard, source);
        }
    }

    private static void GenRookMoves(ref MoveList moveList)
    {
        int source, target;
        ulong bitboard, attack;
        bitboard = genBoard.bitPieces[genBoard.side == 0 ? 3 : 9];
        while (bitboard != 0)
        {
            source = BitUtil.GetLs1bIndex(bitboard);

            attack = Magics.GetRookAttack(source, genBoard.bitAll) & (genBoard.side == 0 ? ~genBoard.bitUnits[0] : ~genBoard.bitUnits[1]);
            while (attack != 0)
            {
                target = BitUtil.GetLs1bIndex(attack);

                if (BitUtil.GetBit((genBoard.side == 0 ? genBoard.bitUnits[1] : genBoard.bitUnits[0]), target))
                    moveList.Add(Move.Encode(source, target, Piece.ROOK, ' ', 1, 0, 0, 0));
                else
                    moveList.Add(Move.Encode(source, target, Piece.ROOK, ' ', 0, 0, 0, 0));

                // Remove `target` bit
                BitUtil.PopBit(ref attack, target);
            }

            // Remove bit to move onto other bits
            BitUtil.PopBit(ref bitboard, source);
        }

    }

    private static void GenQueenMoves(ref MoveList moveList)
    {
        int source, target;
        ulong bitboard, attack;
        bitboard = genBoard.bitPieces[genBoard.side == 0 ? 4 : 10];
        while (bitboard != 0)
        {
            source = BitUtil.GetLs1bIndex(bitboard);

            attack = Magics.GetQueenAttack(source, genBoard.bitAll) & (genBoard.side == 0 ? ~genBoard.bitUnits[0] : ~genBoard.bitUnits[1]);
            while (attack != 0)
            {
                target = BitUtil.GetLs1bIndex(attack);

                if (BitUtil.GetBit((genBoard.side == 0 ? genBoard.bitUnits[1] : genBoard.bitUnits[0]), target))
                    moveList.Add(Move.Encode(source, target, Piece.QUEEN, ' ', 1, 0, 0, 0));
                else
                    moveList.Add(Move.Encode(source, target, Piece.QUEEN, ' ', 0, 0, 0, 0));

                // Remove `target` bit
                BitUtil.PopBit(ref attack, target);
            }

            // Remove bit to move onto other bits
            BitUtil.PopBit(ref bitboard, source);
        }

    }

    private static void GenKingMoves(ref MoveList moveList)
    {
        int source, target;
        ulong bitboard, attack;
        bitboard = genBoard.bitPieces[genBoard.side == 0 ? 5 : 11];
        while (bitboard != 0)
        {
            source = BitUtil.GetLs1bIndex(bitboard);

            attack = Precalculate.kingAttacks[source] & (genBoard.side == 0 ? ~genBoard.bitUnits[0] : ~genBoard.bitUnits[1]);
            while (attack != 0)
            {
                target = BitUtil.GetLs1bIndex(attack);

                if (BitUtil.GetBit((genBoard.side == 0 ? genBoard.bitUnits[1] : genBoard.bitUnits[0]), target))
                    moveList.Add(Move.Encode(source, target, Piece.KING, ' ', 1, 0, 0, 0));
                else
                    moveList.Add(Move.Encode(source, target, Piece.KING, ' ', 0, 0, 0, 0));

                // Remove target bit to move onto the next bit
                BitUtil.PopBit(ref attack, target);
            }
            // Remove source bit to move onto the next bit
            BitUtil.PopBit(ref bitboard, source);
        }
        // Generate castling moves
        GenCastlingMoves(ref moveList);
    }

    private static void GenCastlingMoves(ref MoveList moveList)
    {
        if (genBoard.side == 0) // WHITE
        {
            // Kingside castling
            if (BitUtil.GetBit(genBoard.castling, 0))
            {
                // Check if path is obstructed
                if (!BitUtil.GetBit(genBoard.bitAll, (int)Squares.f1) && !BitUtil.GetBit(genBoard.bitAll, (int)Squares.g1))
                {
                    // Is e1 or f1 attacked by a black piece?
                    if (!IsSquareAttacked(1, (int)Squares.e1, genBoard) && !IsSquareAttacked(1, (int)Squares.f1, genBoard))
                        moveList.Add(Move.Encode("e1g1", Piece.KING, 0, 0, 0, 1));
                }
            }
            // Queenside castling
            if (BitUtil.GetBit(genBoard.castling, 1))
            {
                // Check if path is obstructed
                if (!BitUtil.GetBit(genBoard.bitAll, (int)Squares.b1) && !BitUtil.GetBit(genBoard.bitAll, (int)Squares.c1) && !BitUtil.GetBit(genBoard.bitAll, (int)Squares.d1))
                {
                    // Is d1 or e1 attacked by a black piece?
                    if (!IsSquareAttacked(1, (int)Squares.d1, genBoard) && !IsSquareAttacked(1, (int)Squares.e1, genBoard))
                        moveList.Add(Move.Encode("e1c1", Piece.KING, 0, 0, 0, 1));
                }
            }
        }
        else // BLACK
        {
            // Kingside castling
            if (BitUtil.GetBit(genBoard.castling, 2))
            {
                // Check if path is obstructed
                if (!BitUtil.GetBit(genBoard.bitAll, (int)Squares.f8) && !BitUtil.GetBit(genBoard.bitAll, (int)Squares.g8))
                {
                    // Is e8 or f8 attacked by a white piece?
                    if (!IsSquareAttacked(0, (int)Squares.e8, genBoard) && !IsSquareAttacked(0, (int)Squares.f8, genBoard))
                        moveList.Add(Move.Encode("e8g8", Piece.KING, 0, 0, 0, 1));
                }
            }
            // Queenside castling
            if (BitUtil.GetBit(genBoard.castling, 3))
            {
                // Check if path is obstructed
                if (!BitUtil.GetBit(genBoard.bitAll, (int)Squares.b8) && !BitUtil.GetBit(genBoard.bitAll, (int)Squares.c8) && !BitUtil.GetBit(genBoard.bitAll, (int)Squares.d8))
                {
                    // Is d8 or e8 attacked by a white piece?
                    if (!IsSquareAttacked(0, (int)Squares.d8, genBoard) && !IsSquareAttacked(0, (int)Squares.e8, genBoard))
                        moveList.Add(Move.Encode("e8c8", Piece.KING, 0, 0, 0, 1));
                }
            }
        }
    }
}
