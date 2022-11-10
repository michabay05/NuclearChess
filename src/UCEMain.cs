namespace UCE.src;

class UCEMain
{
    private static int debug = 1;
    static void Main(string[] args)
    {
        BoardUtil.InitRankFileMasks();
        Precalculate.Init();
        Eval.InitMasks();
        Zobrist.InitKeys();
        TTUtil.ClearTable();

        if (debug == 0)
            UCI.Loop();
        else
            Test();
    }

    static void Test()
    {
        for (int r = 0; r < 8; r++)
        {
            for (int f = 0; f < 8; f++)
            {
                int sq = r * 8 + f;
                Console.WriteLine((Squares)sq);
                BitUtil.PrintBits(Precalculate.pawnAttacks[0, sq]);
            }
            Console.WriteLine();
        }
    }
}
