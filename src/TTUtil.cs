using System.Runtime.InteropServices;

namespace Nuclear.src;

class TTUtil
{
    public static readonly int ONE_MB = 1_000_000 / Marshal.SizeOf(typeof(TT));
    public static readonly int HASH_SIZE = 50 * ONE_MB;
    //public static readonly int PAWN_HASH_SIZE = 0;
    public static readonly int NO_HASH_ENTRY = 100_000;

    public static TT[] hashTable = new TT[HASH_SIZE];

    public static void ClearTable()
    {
        for (int i = 0; i < HASH_SIZE; i++)
            hashTable[i] = new TT();
    }

    public static int ReadEntry(ref Board board, int alpha, int beta, int depth)
    {
        // TT pointer that stores particular hash entry for current board
        // and the scoring data, if available
        TT ttable = hashTable[board.hashKey % (ulong)HASH_SIZE];

        // Check if hashTable ID and board ID is the same
        if (ttable.hashKey == board.hashKey)
        {
            // Extract score from hash entry
            int score = ttable.score;
            if (score < -Search.MATE_SCORE) score += Search.ply;
            if (score > Search.MATE_SCORE) score -= Search.ply;

            // Check if depth is the same
            if (ttable.depth >= depth)
            {
                // Match EXACT (PV node) score
                if (ttable.flag == TT.F_HASH_EXACT)
                    return ttable.score;
                // Match ALPHA (fail-low node) score
                if ((ttable.flag == TT.F_HASH_ALPHA) && (score <= alpha))
                    return alpha;
                // Match BETA (fail-high node) score
                if ((ttable.flag == TT.F_HASH_BETA) && (score >= beta))
                    return beta;
            }
        }
        // If no hash value available, return no hash entry
        return NO_HASH_ENTRY;
    }

    public static void WriteEntry(ref Board board, int depth, int score, int flag)
    {
        // Hash table index that stores particular hash entry for current board
        // and the scoring data, if available
        ulong index = board.hashKey % (ulong)HASH_SIZE;

        if (score < -Search.MATE_SCORE) score -= Search.ply;
        if (score > Search.MATE_SCORE) score += Search.ply;

        // Write entry into hash table
        hashTable[index].hashKey = board.hashKey;
        hashTable[index].score = score;
        hashTable[index].depth = depth;
        hashTable[index].flag = flag;
    }
}

struct TT
{
    public static readonly int F_HASH_EXACT = 0;
    public static readonly int F_HASH_ALPHA = 1;
    public static readonly int F_HASH_BETA = 2;

    public ulong hashKey;   // Unique position identifier
    public int score;       // score (alpha/beta/PV)
    public int depth;       // Current search depth
    public int flag;        // Flag for nodes that fail-high, fail-low, PV nodes

    public TT()
    {
        hashKey = 0L;
        depth = 0;
        flag = 0;
        score = 0;
    }

    public override string ToString()
    {
        string flagStr = "";
        if (flag == 0) flagStr = "exact";
        else if (flag == 1) flagStr = "alpha";
        else if (flag == 2) flagStr = "beta";

        return $"Hash Key: {hashKey}\n   Score: {score}\n   Depth: {depth}\n    Flag: {flagStr}\n";
    }
}
