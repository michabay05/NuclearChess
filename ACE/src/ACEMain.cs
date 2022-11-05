namespace ACE.src;

class ACEMain
{
    private static int debug = 0;
    static void Main(string[] args)
    {
        Precalculate.Init();

        Test();
    }

    static void Test()
    {
        if (debug == 0)
            UCI.Loop();
        else
        {
            Board b = FENUtil.Parse(FENUtil.position[2]);
            b.DisplayBoard();
            int depth = 6;
            Search.SearchMove(ref b, depth);
        }
    }
}
