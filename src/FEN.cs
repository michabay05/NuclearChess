namespace Nuclear.src;


class FEN
{
    private static string[] fenParts = { };
    public static readonly string[] position = {
        "8/8/8/8/8/8/8/8 w - - 0 1",
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
        "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
        "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
        "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
        "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
        "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1"
    };

    public static Board Parse(string fen)
    {
        // Parse FEN string
        if (fen == "")
        {
            Console.WriteLine("Given FEN string is empty.");
            Environment.Exit(0);
        }
        fenParts = fen.Split(" ");

        // Init board
        Board board = new Board();
        ParsePieces(ref board);

        // Side to play
        if (fenParts[1] == "w") board.side = 0;
        else if (fenParts[1] == "b") board.side = 1;

        ParseCastling(ref board);
        ParseEnPassant(ref board);

        board.halfMoves = Convert.ToInt32(fenParts[4]);
        board.fullMoves = Convert.ToInt32(fenParts[5]);
        board.fullMoves = (board.fullMoves == 1) ? 0 : board.fullMoves;
        board.UpdateUnits();

        // Init hash key
        board.hashKey = Zobrist.GenHashKey(ref board);

        return board;
    }

    private static void ParsePieces(ref Board board)
    {
        int charIndex = 0;
        for (int r = 0; r < 8; r++)
        {
            for (int f = 0; f < 8; f++)
            {
                int sq = r * 8 + f;
                char currChar = fenParts[0][charIndex];
                if (char.IsLetter(currChar))
                {
                    int piece = Piece.pieceToInt[currChar];
                    BitUtil.SetBit(ref board.bitPieces[piece], sq);
                    if ((piece % 6) == 0)
                        board.pawnHashKey ^= Zobrist.pieceKeys[piece, sq];
                }
                else if (char.IsDigit(currChar))
                {
                    int offset = currChar - '0';
                    if (Board.GetPieceOnSquare(board, sq) == 12) offset--;
                    f += offset;
                }
                else if (currChar == '/')
                    f--;
                charIndex++;
            }
        }
    }

    private static void ParseCastling(ref Board board)
    {
        if (fenParts[2] == "-")
            return;
        for (int i = 0; i < fenParts[2].Length; i++)
        {
            switch (fenParts[2][i])
            {
                case 'K':
                    BitUtil.SetBit(ref board.castling, 0);
                    break;
                case 'Q':
                    BitUtil.SetBit(ref board.castling, 1);
                    break;
                case 'k':
                    BitUtil.SetBit(ref board.castling, 2);
                    break;
                case 'q':
                    BitUtil.SetBit(ref board.castling, 3);
                    break;
                default:
                    break;
            }
        }
    }

    private static void ParseEnPassant(ref Board board)
    {
        if (fenParts[3] == "-")
        {
            return;
        }
        int file = fenParts[3][0] - 'a';
        int rank = (8 - (fenParts[3][1] - '0')) * 8;
        board.enPassant = rank + file;
    }

    public static string Generate(ref Board board)
    {
        string fen = "";

        // Piece placement
        int offset = 0;
        for (int r = 0; r < 8; r++)
        {
            offset = 0;
            for (int f = 0; f < 8; f++)
            {
                int sq = r * 8 + f;
                char currChar = Piece.ascii[Board.GetPieceOnSquare(board, sq)];
                if (currChar != ' ')
                {
                    if (offset > 0)
                    {
                        fen += $"{offset}";
                        offset = 0;
                    }
                    fen += $"{currChar}";
                }
                else
                    offset++;
            }
            if (offset > 0)
                fen += $"{offset}";
            if (r != 7)
                fen += "/";
        }
        // Side to move
        if (board.side == 0)
            fen += " w";
        else
            fen += " b";
        // Castling rights
        if (board.castling == 0)
            fen += " -";
        else
        {
            fen += " ";
            if (BitUtil.GetBit(board.castling, 0)) fen += "K";
            if (BitUtil.GetBit(board.castling, 1)) fen += "Q";
            if (BitUtil.GetBit(board.castling, 2)) fen += "k";
            if (BitUtil.GetBit(board.castling, 3)) fen += "q";
        }
        // Enpassant square
        fen += $" {(board.enPassant != -1 ? (Squares)board.enPassant : '-')}";
        // Half moves
        fen += $" {board.halfMoves}";
        // Full moves
        fen += $" {(board.fullMoves == 0 ? 1 : board.fullMoves)}";

        return fen;
    }
}
