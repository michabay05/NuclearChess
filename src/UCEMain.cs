namespace UCE.src;

class UCEMain
{
    private static int debug = 0;
    static void Main(string[] args)
    {
        Precalculate.Init();
        Zobrist.InitKeys();
        TTUtil.ClearTable();

        if (debug == 0)
            UCI.Loop();
        else
            Test();
    }

    static void Test()
    {
        string fen = "2r3k1/R7/8/1R6/8/8/P4KPP/8 w - - 0 40";
        Board b = FEN.Parse(fen);
        b.Display();
        Search.SearchMove(ref b, 10);
    }
}
