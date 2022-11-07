namespace UCE.src;

class UCEMain
{
    private static int debug = -1;
    static void Main(string[] args)
    {
        Precalculate.Init();
        Zobrist.InitKeys();

        if (debug == 0)
            UCI.Loop();
        else
            Test();
    }

    static void Test()
    {
        TTUtil.ClearTable();

        // Write Example entry
        Board b = FEN.Parse(FEN.position[2]);
        TTUtil.WriteEntry(ref b, 4, 21, TT.F_HASH_BETA);

        // Read entry
        int score = TTUtil.ReadEntry(ref b, 20, 30, 4);

        Console.WriteLine("Score from hash entry: " + score);
    }
}
