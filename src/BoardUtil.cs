namespace ACE.src;


class BoardUtil
{
    public static readonly int[] rows =
    {
        7, 7, 7, 7, 7, 7, 7, 7,
        6, 6, 6, 6, 6, 6, 6, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        4, 4, 4, 4, 4, 4, 4, 4,
        3, 3, 3, 3, 3, 3, 3, 3,
        2, 2, 2, 2, 2, 2, 2, 2,
        1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0
    };

    public static readonly int[] cols =
    {
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7,
        0, 1, 2, 3, 4, 5, 6, 7
    };

    private static readonly int[] castlingRights =
    {
         7, 15, 15, 15,  3, 15, 15, 11,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        13, 15, 15, 15, 12, 15, 15, 14
    };

    public static bool MakeMove(ref Board main, int move, MoveType moveFlag)
    {
        if (moveFlag == MoveType.allMoves)
        {
            // Copy board to restore board if move is illegal
            Board copy = Board.Clone(main);

            // Parse move information
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
                for (int i = start; i <= end; i++)
                {
                    if (BitUtil.GetBit(main.bitPieces[i], target))
                    {
                        BitUtil.PopBit(ref main.bitPieces[i], target);
                        break;
                    }
                }
            }
            // Promotion move
            if (promotedInt != 12)
            {
                // Remove target bit from either the white(0) or black(6) pawn bitboard
                BitUtil.PopBit(ref main.bitPieces[(main.side == 0) ? 0 : 6], target);
                BitUtil.SetBit(ref main.bitPieces[promotedInt], target);
            }

            // Enpassant capture
            if (isEnpassant)
            {
                if (main.side == 0)
                    BitUtil.PopBit(ref main.bitPieces[6], target + 8);
                else
                    BitUtil.PopBit(ref main.bitPieces[0], target - 8);
            }
            // Remove enpassant attribute regardless of an enpassant move
            main.enPassant = -1;

            // Two square pawn push
            if (isTwoSquarePush)
            {
                // If white to move
                if (main.side == 0)
                    main.enPassant = target + 8;
                else
                    main.enPassant = target - 8;
            }

            // Castling moves
            if (isCastling)
            {
                // Check for all 4 target squares
                switch (target)
                {
                    case (int)Squares.g1:
                        BitUtil.PopBit(ref main.bitPieces[3], (int)Squares.h1);
                        BitUtil.SetBit(ref main.bitPieces[3], (int)Squares.f1);
                        break;
                    case (int)Squares.c1:
                        BitUtil.PopBit(ref main.bitPieces[3], (int)Squares.a1);
                        BitUtil.SetBit(ref main.bitPieces[3], (int)Squares.d1);
                        break;
                    case (int)Squares.g8:
                        BitUtil.PopBit(ref main.bitPieces[9], (int)Squares.h8);
                        BitUtil.SetBit(ref main.bitPieces[9], (int)Squares.f8);
                        break;
                    case (int)Squares.c8:
                        BitUtil.PopBit(ref main.bitPieces[9], (int)Squares.a8);
                        BitUtil.SetBit(ref main.bitPieces[9], (int)Squares.d8);
                        break;
                }
            }

            // Update castling rights
            main.castling &= castlingRights[source];
            main.castling &= castlingRights[target];

            // Update white and black units bitboard
            main.UpdateUnits();

            // Switch side
            main.side ^= 1;

            // Check if king is in check
            if (MoveGen.IsSquareAttacked(main.side, BitUtil.GetLs1bIndex(main.bitPieces[(main.side == 0) ? 11 : 5]), main))
            {
                // Move is check so take move back
                Board.Restore(ref main, copy);
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
