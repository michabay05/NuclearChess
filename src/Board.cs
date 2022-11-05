namespace UCE.src;

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
    public void DisplayBoard()
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
        Console.WriteLine($"        Move num: {fullMoves}\n\n");
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
    }
}
