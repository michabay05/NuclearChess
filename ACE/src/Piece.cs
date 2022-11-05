namespace ACE.src;

class Piece
{
    // int => char
    public static readonly char[] ascii = {
        'P', 'N', 'B', 'R', 'Q', 'K', 'p', 'n', 'b', 'r', 'q', 'k', ' ',
    };

    // char => int
    public static Dictionary<char, int> pieceToInt = new Dictionary<char, int>()
    {
        { 'P', 0 },
        { 'N', 1 },
        { 'B', 2 },
        { 'R', 3 },
        { 'Q', 4 },
        { 'K', 5 },
        { 'p', 6 },
        { 'n', 7 },
        { 'b', 8 },
        { 'r', 9 },
        { 'q', 10 },
        { 'k', 11 },
        { ' ', 12 }
    };

    public static readonly int PAWN = 0;
    public static readonly int KNIGHT = 1;
    public static readonly int BISHOP = 2;
    public static readonly int ROOK = 3;
    public static readonly int QUEEN = 4;
    public static readonly int KING = 5;
}
