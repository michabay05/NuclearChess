namespace UCE.src;

class UCEMain
{
    private static int debug = 1;
    static void Main(string[] args)
    {
        Precalculate.Init();

        if (debug == 0)
            UCI.Loop();
        else
            Test();
    }

    static void Test()
    {
        Board b = FENUtil.Parse(FENUtil.position[2]);
        b.DisplayBoard();
        int depth = 6;
        Search.SearchMove(ref b, depth);
    }
}
