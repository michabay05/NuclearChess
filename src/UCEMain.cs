namespace UCE.src;

class UCEMain
{
    private static int debug = 0;
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
        string fen = "7r/8/8/8/8/8/8/R7 w - - 0 1";
        //fen = FEN.position[1];
        Board a = FEN.Parse(fen);
        a.Display();
        Console.WriteLine($"Score: {Eval.Evaluate(ref a)}");
    }
}
