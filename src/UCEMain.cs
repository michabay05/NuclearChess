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
    }
}
