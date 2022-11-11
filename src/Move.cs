namespace Nuclear.src;

enum MoveType { allMoves, onlyCaptures }

class Move
{
    public static int Encode(int source, int target, int piece, char promotedLtr, int capture, int twoSquarePush, int enpassant, int castling)
    {
        int promoted;
        switch (char.ToLower(promotedLtr))
        {
            case 'q':
                promoted = Piece.pieceToInt['q'];
                break;
            case 'r':
                promoted = Piece.pieceToInt['r'];
                break;
            case 'b':
                promoted = Piece.pieceToInt['b'];
                break;
            case 'n':
                promoted = Piece.pieceToInt['n'];
                break;
            default:
                promoted = 0;
                break;
        }
        return (source) | (target << 6) | (piece << 12) | (promoted << 16) | (capture << 20) | (twoSquarePush << 21) | (enpassant << 22) | (castling << 23);
    }
    public static int Encode(string moveStr, int piece, int capture, int twoSquarePush, int enpassant, int castling)
    {
        if (moveStr.Length != 4 && moveStr.Length != 5) return -1;
        string sourceStr, targetStr;
        int source, target;
        char promoted;
        if (moveStr.Length == 4)
        {
            sourceStr = moveStr.Substring(0, 2);
            source = (sourceStr[0] - 'a') + ((8 - (sourceStr[1] - '0')) * 8);
            targetStr = moveStr.Substring(2);
            target = (targetStr[0] - 'a') + ((8 - (targetStr[1] - '0')) * 8);
            promoted = ' ';
        }
        else
        {
            sourceStr = moveStr.Substring(0, 2);
            source = (sourceStr[0] - 'a') + ((8 - (sourceStr[1] - '0')) * 8);
            targetStr = moveStr.Substring(2);
            target = (targetStr[0] - 'a') + ((8 - (targetStr[1] - '0')) * 8);
            promoted = targetStr[2];
        }
        return Encode(source, target, piece, promoted, capture, twoSquarePush, enpassant, castling);
    }

    public static int GetSource(int move) => move & 0x3f;

    public static int GetTarget(int move) => (move & 0xfc0) >> 6;

    public static int GetPiece(int move) => (move & 0xf000) >> 12;

    public static char GetPromoted(int move)
    {
        int promoted = (move & 0xf0000) >> 16;
        int target = GetTarget(move);
        if (promoted > 0)
            return target <= (int)Squares.h8 ? char.ToUpper(Piece.ascii[promoted]) : Piece.ascii[promoted];
        else
            return ' ';
    }

    public static bool IsCapture(int move) => (move & 0x100000) != 0;

    public static bool IsTwoSquarePush(int move) => (move & 0x200000) != 0;

    public static bool IsEnpassant(int move) => (move & 0x400000) != 0;

    public static bool IsCastling(int move) => (move & 0x800000) != 0;

    public static string ToString(int move)
    {
        string sourceStr, targetStr;
        string promoted = "";

        sourceStr = ((Squares)GetSource(move)).ToString();
        targetStr = ((Squares)GetTarget(move)).ToString();
        if (GetPromoted(move) > 0)
            promoted = "" + Piece.ascii[Piece.pieceToInt[GetPromoted(move)]];
        if (promoted == " ")
            promoted = "";
        return $"{sourceStr}{targetStr}{(promoted)}";
    }
}

struct MoveList
{
    public int[] list;
    public int count;
    public Board boardRef;

    public MoveList(Board board)
    {
        list = new int[256];
        count = 0;
        boardRef = board;
    }

    public void Add(int move)
    {
        if (count == 255)
        {
            Console.WriteLine($"moveList.count = {count}\nOne more and I'll overflow.");
            Environment.Exit(0);
        }
        list[count] = move;
        count++;
    }

    public int Search(string moveStr)
    {
        int source = (moveStr[0] - 'a') + ((8 - (moveStr[1] - '0')) * 8);
        int piece = Board.GetPieceOnSquare(boardRef, source);
        int move = Move.Encode(moveStr, piece, 0, 0, 0, 0);
        for (int i = 0; i < count; i++)
        {
            if ((Move.GetSource(list[i]) == Move.GetSource(move)) &&
                (Move.GetTarget(list[i]) == Move.GetTarget(move)) &&
                (Move.GetPromoted(list[i]) == Move.GetPromoted(move)))
                return list[i];
        }
        return 0;
    }

    public void PrintList()
    {
        int move;
        Console.WriteLine("Source  |  Target  |  Piece  |  Promoted  |  Capture  |  Two Square  |  Enpassant  |  Castling");
        Console.WriteLine("--------------------------------------------------------------------------------------------------");
        for (int i = 0; i < count; i++)
        {
            move = list[i];
            Console.WriteLine("   {0}   |   {1}     |    {2}    |      {3}     |     {4}     |       {5}      |     {6}       |      {7}", (Squares)Move.GetSource(move),
                                                                                                                              (Squares)Move.GetTarget(move),
                                                                                                                              Move.GetPiece(move),
                                                                                                                              Move.GetPromoted(move),
                                                                                                                              Move.IsCapture(move) ? 1 : 0,
                                                                                                                              Move.IsTwoSquarePush(move) ? 1 : 0,
                                                                                                                              Move.IsEnpassant(move) ? 1 : 0,
                                                                                                                              Move.IsCastling(move) ? 1 : 0);
        }
        Console.WriteLine($"\nTotal number of moves: {count}");
    }
}
