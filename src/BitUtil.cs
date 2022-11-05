namespace UCE.src;


class BitUtil
{
    public static bool GetBit(ulong board, int index) => (board & 1ul << index) > 0;
    public static bool GetBit(int num, int index) => (num & 1 << index) > 0;

    public static void SetBit(ref ulong board, int index) => board |= 1ul << index;
    public static void SetBit(ref int num, int index) => num |= 1 << index;

    public static void PopBit(ref ulong board, int index) => board ^= 1ul << index;
    public static void PopBit(ref int num, int index) => num ^= 1 << index;

    public static int CountBits(ulong board)
    {
        int count = 0;
        for (; board > 0; board &= board - 1, count++) ;
        return count;
    }

    public static int GetLs1bIndex(ulong board) { return board > 0 ? CountBits(board ^ board - 1) - 1 : 0; }

    public static void PrintBits(ulong board)
    {
        for (int r = 0; r < 8; r++)
        {
            Console.Write($" {8 - r} |");
            for (int f = 0; f < 8; f++)
            {
                Console.Write($" {(GetBit(board, r * 8 + f) ? 1 : 0)}");
            }
            Console.WriteLine();
        }
        Console.WriteLine("     - - - - - - - -\n     a b c d e f g h\n");
        Console.WriteLine($"    Decimal: {board}\n    Hexadecimal: 0x{board:X}\n");
    }
}
