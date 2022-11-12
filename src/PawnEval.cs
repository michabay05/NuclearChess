namespace Nuclear.src;

class PawnEval
{
    private static int pawnHashSize = 100_000;
    private static PawnEval[] pawnTable;

    // Instance variables
    public ulong pawnHashKey;
    public int openingEval;
    public int endgameEval;

    public PawnEval()
    {
        pawnHashKey = 0UL;
        openingEval = 0;
        endgameEval = 0;
    }

    public static void Init(int size)
    {
        pawnHashSize = size;
        pawnTable = new PawnEval[pawnHashSize];
    }

    public static void ClearTable()
    {
        for (int i = 0; i < pawnHashSize; i++)
            pawnTable[i] = new PawnEval();
    }

    public static int ReadEntry(ulong hashKey, ref int openingScore, ref int endgameScore)
    {
        PawnEval pHashEval = pawnTable[hashKey % (ulong)pawnHashSize];
        if (pHashEval.pawnHashKey == hashKey)
        {
            openingScore = pHashEval.openingEval;
            endgameScore = pHashEval.endgameEval;
        }

        return HashTable.NO_HASH_ENTRY;
    }

    public static void WriteEntry(ulong hashKey, int openingScore, int endgameScore)
    {
        PawnEval pHashEval = pawnTable[hashKey % (ulong)pawnHashSize];
        pHashEval.pawnHashKey = hashKey;
        pHashEval.openingEval = openingScore;
        pHashEval.endgameEval = endgameScore;
    }
}
