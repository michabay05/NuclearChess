namespace UCE.src;


class UCI
{
    static Board currBoard = new Board();

    public static void Parse(string command)
    {
        if (command == "")
            return;

        if (command.IndexOf("isready") != -1)
            Console.WriteLine("readyok");
        else if (command.IndexOf("position") != -1)
            Position(command.Substring(9));
        else if (command.IndexOf("go") != -1)
            Go(command.Substring(3));
        else if (command.IndexOf("ucinewgame") != -1)
            Position("startpos");
        else if (command.IndexOf("uci") != -1)
        {
            // Print engine info
            Console.WriteLine("id name VCE");
            Console.WriteLine("id author michabay05");
            Console.WriteLine("uciok");
        }
        else if (command.IndexOf("display") != -1)
            currBoard.DisplayBoard();
        else if (command.IndexOf("help") != -1)
            Help();
        else
            Console.WriteLine($"Unknown command: {command}");
    }

    public static void Loop()
    {
        // Print engine info
        Console.WriteLine("id name VCE");
        Console.WriteLine("id author michabay05");
        Console.WriteLine("uciok");
        while (true)
        {
            // Get input
            string input = "";
            GetInput("", ref input);

            // Quit before parsing
            if (input == "quit")
                break;
            Parse(input);
        }
    }

    private static void Position(string cmdArgs)
    {
        // Possible options: fen, startpos
        string fen = "";
        string[] cmdParts = cmdArgs.Split(" ");
        int currentIndex = 0;

        // Second arguments: startpos fen
        if (cmdParts[0] == "startpos")
        {
            fen = FENUtil.position[1];
            currentIndex += 8;
        }
        else if (cmdParts[0] == "fen" && cmdParts.Length >= 7)
        {
            for (int i = 0; i < 6; i++)
                fen += cmdParts[1 + i] + " ";
            currentIndex += 3 + fen.Length;
        }
        else
        {
            Console.WriteLine($"Unknown command: position {cmdArgs}");
            return;
        }

        if (fen != "")
            currBoard = FENUtil.Parse(fen);

        int movesStart = cmdArgs.IndexOf("moves", currentIndex);
        if (movesStart != -1)
        {
            string[] moveList = cmdArgs.Substring(movesStart + 5).Trim().Split(" ");
            for (int i = 0; i < moveList.Length; i++)
            {
                int move = ParseMove(moveList[i]);
                if (move < 0 || !BoardUtil.MakeMove(ref currBoard, move, MoveType.allMoves))
                {
                    Console.WriteLine($"Move {moveList[i]} is illegal!");
                    continue;
                }
            }
        }
    }

    private static int ParseMove(string moveStr)
    {
        int move = Move.Encode(moveStr, 0, 0, 0, 0, 0);

        MoveList mL = new MoveList(currBoard);
        MoveGen.Generate(ref mL);
        move = mL.Search(moveStr);
        return move;
    }

    private static void Go(string cmdArgs)
    {
        // UCI extension -> took from stockfish
        if (cmdArgs.IndexOf("perft") != -1)
            Perft.Test(ref currBoard, Convert.ToInt32(cmdArgs.Substring(6).Trim()));
        else if (cmdArgs.IndexOf("depth") != -1)
            Search.SearchMove(ref currBoard, Convert.ToInt32(cmdArgs.Substring(6).Trim()));
        else
        {
            Search.SearchMove(ref currBoard, 5);
            Console.WriteLine($"Unknown command: go {cmdArgs}");
        }
    }

    private static void Help()
    {
        Console.WriteLine("\n              Command name               |         Description");
        Console.WriteLine("-------------------------------------------------------------------------------------------------------");
        Console.WriteLine("                  uci                    |    Prints engine info and 'uciok'");
        Console.WriteLine("              isready                    |    Prints 'readyok' if the engine is ready");
        Console.WriteLine("    position startpos                    |    Set board to starting position");
        Console.WriteLine("    position startpos moves <move1> ...  |    Set board to starting position then playing following moves");
        Console.WriteLine("   position fen <FEN>                    |    Set board to a custom FEN");
        Console.WriteLine("   position fen <FEN> moves <move1> ...  |    Set board to a custom FEN then playing following moves");
        Console.WriteLine("     go depth <depth>                    |    Returns the best move after search for given amount of depth");
        Console.WriteLine("                 quit                    |    Exit the UCI mode");
        Console.WriteLine("\n------------------------------------ EXTENSIONS ----------------------------------------");
        Console.WriteLine("              display                    |    Display board");
        Console.WriteLine("     go perft <depth>                    |    Calculate the total number of moves from a position for a given depth");
    }

    private static void GetInput(string prompt, ref string output)
    {
        Console.WriteLine(prompt);
        string? input = Console.ReadLine();
        if (input == null)
            return;
        output = input;
    }
}
