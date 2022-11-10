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
        string fen = "6k1/ppppprbp/8/8/8/8/PPPPPRBP/6K1 w - - 0 1";
        fen = FEN.position[1];
        Board a = FEN.Parse(fen);
        a.Display();
        Console.WriteLine($"Score: {Eval.Evaluate(ref a)}");
        /*
        for (int r = 0; r < 8; r++)
        {
            for (int f = 0; f < 8; f++)
            {
                Console.Write(BoardUtil.GetRank(r * 8 + f) + " ");
            }
            Console.WriteLine();
        }
        */
    }
}
