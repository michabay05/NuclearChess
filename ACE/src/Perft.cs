namespace ACE.src;

class Perft
{
    public static long nodes;
    public static long MsTime
    {
        get => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public static void Driver(ref Board board, int depth)
    {
        // Base case - escape condition
        if (depth == 0)
        {
            nodes++;
            return;
        }

        // Generate moves
        MoveList moveList = new MoveList(board);
        MoveGen.Generate(ref moveList);

        Board copy;
        for (int i = 0; i < moveList.count; i++)
        {
            // Preserve current board state
            copy = Board.Clone(board);

            // Is generated move legal?
            if (!BoardUtil.MakeMove(ref board, moveList.list[i], MoveType.allMoves))
                continue;

            // Recursively generate moves
            Driver(ref board, depth - 1);

            // Restore board
            Board.Restore(ref board, copy);
        }
    }

    public static void Test(ref Board board, int depth)
    {
        Console.WriteLine($"\n----------------- Performance Test ({depth}) -----------------");

        // Reset counters
        nodes = 0L;

        // create move list instance
        MoveList moveList = new MoveList(board);

        // generate moves
        MoveGen.Generate(ref moveList);

        // Init start time
        long start = MsTime;

        Board copy;
        // Loop over generated moves
        for (int i = 0; i < moveList.count; i++)
        {
            // Preserve current board state
            copy = Board.Clone(board);

            // Is generated move legal?
            if (!BoardUtil.MakeMove(ref board, moveList.list[i], MoveType.allMoves))
                continue;

            long totalNodes = nodes;
            Driver(ref board, depth - 1);
            long oldNodes = nodes - totalNodes;

            // Take back
            Board.Restore(ref board, copy);

            // Print move
            Console.WriteLine($"     {Move.ToString(moveList.list[i])}: {oldNodes}");
        }

        // Print results
        Console.WriteLine($"\n     Depth: {depth}");
        Console.WriteLine($"     Nodes: {nodes}");
        Console.WriteLine($"      Time: {MsTime - start} ms");
    }
}
