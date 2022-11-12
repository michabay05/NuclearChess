namespace Nuclear.src;

class Board
{
    // Properties of a board
    public ulong bitAll;
    public ulong[] bitUnits;
    public ulong[] bitPieces;
    public int side;
    public int enPassant;
    public int castling; // Format: qkQK
    public int halfMoves;
    public int fullMoves;
    public ulong hashKey;
    public ulong pawnHashKey;
    public ulong[] repetitionTable;
    public int repetitionIndex;

    public Board()
    {
        bitAll = 0ul;
        bitUnits = new ulong[2]; // 0: WHITE, 1: BLACK
        bitPieces = new ulong[12]; // [0,5] > WHITE, [6,11] > BLACK
        side = 0; // WHITE
        enPassant = -1;
        halfMoves = 0;
        fullMoves = 0;
        castling = 0;
        hashKey = 0UL;
        pawnHashKey = 0UL;
        // 1000 refers to # of plys: (500 moves) just for safety
        repetitionTable = new ulong[1000];
        repetitionIndex = 0;
    }

    public void UpdateUnits()
    {
        bitUnits[0] = 0;
        bitUnits[1] = 0;
        // Update white piece occupancy
        for (int i = 0; i <= 5; i++)
            bitUnits[0] |= bitPieces[i];
        // Update black piece occupancy
        for (int i = 6; i <= 11; i++)
            bitUnits[1] |= bitPieces[i];
        // Update all piece occupancy
        bitAll = bitUnits[0] | bitUnits[1];
    }

    /* Prints the board */
    public void Display()
    {
        Console.WriteLine("\n   +---+---+---+---+---+---+---+---+");
        for (int r = 0; r < 8; r++)
        {
            Console.Write($" {8 - r} |");
            for (int f = 0; f < 8; f++)
            {
                Console.Write($" {Piece.ascii[GetPieceOnSquare(this, r * 8 + f)]} |");
            }
            Console.WriteLine("\n   +---+---+---+---+---+---+---+---+");
        }
        Console.WriteLine("     a   b   c   d   e   f   g   h\n");
        Console.WriteLine($"    Side to move: {(side == 0 ? "White" : "Black")}");
        Console.WriteLine($"       Enpassant: {(Squares)enPassant}");
        PrintCastling();
        Console.WriteLine($"        Move num: {fullMoves}\n");
        Console.WriteLine($"        Hash key: {hashKey:X}");
        Console.WriteLine($"   Pawn Hash key: {pawnHashKey:X}\n\n");
    }

    /* Prints the castling rights in a specific format */
    private void PrintCastling()
    {
        Console.WriteLine("        Castling: {0}{1}{2}{3}", BitUtil.GetBit(castling, 0) ? 'K' : '-',
                                          BitUtil.GetBit(castling, 1) ? 'Q' : '-',
                                          BitUtil.GetBit(castling, 2) ? 'k' : '-',
                                          BitUtil.GetBit(castling, 3) ? 'q' : '-'
                                          );
    }

    /* Checks if there's a piece on a square
     * returns the piece number if there's one
     * return 12 (.) if there's none */
    public static int GetPieceOnSquare(Board board, int sq)
    {
        for (int piece = 0; piece < 12; piece++)
        {
            if (BitUtil.GetBit(board.bitPieces[piece], sq))
                return piece;
        }
        return 12; // DEFAULT VALUE: '.' <= asciiPieces[12]
    }

    public static Board Clone(Board main)
    {
        Board copy = new Board();
        Array.Copy(main.bitPieces, copy.bitPieces, 12);
        copy.UpdateUnits();
        copy.castling = main.castling;
        copy.enPassant = main.enPassant;
        copy.side = main.side;
        copy.halfMoves = main.halfMoves;
        copy.fullMoves = main.fullMoves;
        copy.hashKey = main.hashKey;

        return copy;
    }

    public static void Restore(ref Board main, Board copy)
    {
        Array.Copy(copy.bitPieces, main.bitPieces, 12);
        main.UpdateUnits();
        main.castling = copy.castling;
        main.enPassant = copy.enPassant;
        main.side = copy.side;
        main.halfMoves = copy.halfMoves;
        main.fullMoves = copy.fullMoves;
        main.hashKey = copy.hashKey;
    }

    public static bool MakeMove(ref Board main, int move, MoveType moveFlag)
    {
        if (moveFlag == MoveType.allMoves)
        {
            // Copy board to restore board if move is illegal
            Board copy = Clone(main);

            // Decode move information
            int source = Move.GetSource(move);
            int target = Move.GetTarget(move);
            int piece = Move.GetPiece(move);
            int promotedInt = Piece.pieceToInt[Move.GetPromoted(move)];
            bool isCapture = Move.IsCapture(move);
            bool isTwoSquarePush = Move.IsTwoSquarePush(move);
            bool isEnpassant = Move.IsEnpassant(move);
            bool isCastling = Move.IsCastling(move);

            // Move piece from source to target
            BitUtil.PopBit(ref main.bitPieces[(main.side == 0) ? piece : piece + 6], source);
            BitUtil.SetBit(ref main.bitPieces[(main.side == 0) ? piece : piece + 6], target);

            // Hash piece: Remove 'source' piece and place on 'target'
            main.hashKey ^= Zobrist.pieceKeys[(main.side == 0) ? piece : piece + 6, source];
            main.hashKey ^= Zobrist.pieceKeys[(main.side == 0) ? piece : piece + 6, target];

            if (piece == 0)
            {
                main.pawnHashKey ^= Zobrist.pieceKeys[(main.side == 0) ? piece : piece + 6, source];
                main.pawnHashKey ^= Zobrist.pieceKeys[(main.side == 0) ? piece : piece + 6, target];
            }

            // Remove enemy pieces on target square if capture
            if (isCapture)
            {
                // Get opponent piece start and end range
                int start, end;
                if (main.side == 0)
                {
                    start = 6;
                    end = 11;
                }
                else
                {
                    start = 0;
                    end = 5;
                }
                for (int bbPiece = start; bbPiece <= end; bbPiece++)
                {
                    if (BitUtil.GetBit(main.bitPieces[bbPiece], target))
                    {
                        BitUtil.PopBit(ref main.bitPieces[bbPiece], target);
                        // Hash capture: remove captured piece
                        main.hashKey ^= Zobrist.pieceKeys[bbPiece, target];
                        // If captured piece is a pawn, remove it from the hash key
                        if (GetPieceOnSquare(main, target) == start)
                            main.pawnHashKey ^= Zobrist.pieceKeys[0, target];
                        break;
                    }
                }
            }
            // Promotion move
            if (promotedInt != 12)
            {
                // Remove target bit from either the white(0) or black(6) pawn bitboard
                // Also hash promotion
                BitUtil.PopBit(ref main.bitPieces[(main.side == 0) ? 0 : 6], target);
                main.hashKey ^= Zobrist.pieceKeys[(main.side == 0) ? 0 : 6, target];

                BitUtil.SetBit(ref main.bitPieces[promotedInt], target);
                main.hashKey ^= Zobrist.pieceKeys[promotedInt, target];
            }

            // Enpassant capture
            if (isEnpassant)
            {
                if (main.side == 0)
                {
                    BitUtil.PopBit(ref main.bitPieces[6], target + 8);
                    main.hashKey ^= Zobrist.pieceKeys[6, target + 8];
                }
                else
                {
                    BitUtil.PopBit(ref main.bitPieces[0], target - 8);
                    main.hashKey ^= Zobrist.pieceKeys[0, target - 8];
                }
            }
            // Hash enpassant move
            if (main.enPassant != -1)
                main.hashKey ^= Zobrist.enPassantKeys[main.enPassant];

            // Reset enpassant, regardless of an enpassant move being played or not
            main.enPassant = -1;

            // Two square pawn push
            if (isTwoSquarePush)
            {
                // If white to move
                if (main.side == 0)
                    main.enPassant = target + 8;
                else
                    main.enPassant = target - 8;

                main.hashKey ^= Zobrist.enPassantKeys[main.enPassant];
            }

            // Castling moves
            if (isCastling)
            {
                // Check for all 4 target squares
                // Also hash the rook move
                switch (target)
                {
                    case (int)Squares.g1:
                        BitUtil.PopBit(ref main.bitPieces[3], (int)Squares.h1);
                        main.hashKey ^= Zobrist.pieceKeys[3, (int)Squares.h1];

                        BitUtil.SetBit(ref main.bitPieces[3], (int)Squares.f1);
                        main.hashKey ^= Zobrist.pieceKeys[3, (int)Squares.f1];
                        break;
                    case (int)Squares.c1:
                        BitUtil.PopBit(ref main.bitPieces[3], (int)Squares.a1);
                        main.hashKey ^= Zobrist.pieceKeys[3, (int)Squares.a1];

                        BitUtil.SetBit(ref main.bitPieces[3], (int)Squares.d1);
                        main.hashKey ^= Zobrist.pieceKeys[3, (int)Squares.d1];
                        break;
                    case (int)Squares.g8:
                        BitUtil.PopBit(ref main.bitPieces[9], (int)Squares.h8);
                        main.hashKey ^= Zobrist.pieceKeys[9, (int)Squares.h8];

                        BitUtil.SetBit(ref main.bitPieces[9], (int)Squares.f8);
                        main.hashKey ^= Zobrist.pieceKeys[9, (int)Squares.f8];
                        break;
                    case (int)Squares.c8:
                        BitUtil.PopBit(ref main.bitPieces[9], (int)Squares.a8);
                        main.hashKey ^= Zobrist.pieceKeys[9, (int)Squares.a8];

                        BitUtil.SetBit(ref main.bitPieces[9], (int)Squares.d8);
                        main.hashKey ^= Zobrist.pieceKeys[9, (int)Squares.d8];
                        break;
                }
            }

            // Hash castling rights
            main.hashKey ^= Zobrist.castlingKeys[main.castling];

            // Update castling rights
            main.castling &= BoardUtil.castlingRights[source];
            main.castling &= BoardUtil.castlingRights[target];

            // Hash castling rights
            main.hashKey ^= Zobrist.castlingKeys[main.castling];

            // Update white and black units bitboard
            main.UpdateUnits();

            // Switch side
            main.side ^= 1;

            // Hash side to move
            // If side to move is white, don't hash side to move
            // If side to move is black, hash side to move
            //if (main.side == 1)
            main.hashKey ^= Zobrist.sideKey;

            /* ============= FOR DEBUG PURPOSES ONLY ===============
            ulong hashFromScratch = Zobrist.GenHashKey(ref main);
            if (main.hashKey != hashFromScratch)
            {
                Console.WriteLine($"\nBoard.MakeMove({Move.ToString(move)})");
                main.Display();
                Console.WriteLine($"Hash should've been {hashFromScratch:X}");
                Console.ReadLine();
            }
             ============= FOR DEBUG PURPOSES ONLY =============== */

            // Check if king is in check
            if (MoveGen.IsSquareAttacked(main.side, BitUtil.GetLs1bIndex(main.bitPieces[(main.side == 0) ? 11 : 5]), main))
            {
                // Move is check so take move back
                Restore(ref main, copy);
                return false;
            }
            else
            {
                main.fullMoves++;
                // Move isn't check
                return true;
            }
        }
        else
        {
            // make sure move is the capture
            if (Move.IsCapture(move))
            {
                return MakeMove(ref main, move, MoveType.allMoves);
            }

            // otherwise the move is not a capture
            else
                // don't make it
                return false;
        }
    }

}
