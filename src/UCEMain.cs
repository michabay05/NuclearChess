using System.Runtime.InteropServices;

namespace Nuclear.src;

class UCEMain
{
    private static int debug = 0;
    static void Main(string[] args)
    {
        Attack.Init();
        BoardUtil.InitRankFileMasks();
        Eval.InitMasks();
        Zobrist.InitKeys();
        HashTable.ClearTable();

        PawnEval.Init(100_000);
        PawnEval.ClearTable();

        if (debug == 0)
            UCI.Loop();
        else
            Test();
    }

    static void Test()
    {
        //Board a = FEN.Parse("8/8/6P1/8/8/1p6/6R1/8 w - - 0 1");
        //a.Display();
        UCI.Parse("ucinewgame");
        UCI.Parse("go movetime 2500");
    }
}
