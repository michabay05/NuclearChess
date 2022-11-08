using System.Xml;
using System.Xml.Schema;

namespace UCE.src;


class UCI
{
    private static Board currentBoard = new Board();
    public static bool Stop = false;
    public static bool Quit = false;
    public static bool isInfinite = false;
    public static bool isTimeControlled = false;
    private static int timeLeft = -1;
    private static int increment = 0;
    private static int movesToGo = 40, moveTime = -1;
    public static long startTime = 0L, stopTime = 0L;
    public static long MsTime
    {
        get => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }


    public static void Parse(string command)
    {
        if (command == "" || command == null)
            return;

        if (command.IndexOf("isready") != -1)
            Console.WriteLine("readyok");
        else if (command.IndexOf("position") != -1)
        {
            Position(command.Substring(9));
            TTUtil.ClearTable();
        }
        else if (command.IndexOf("ucinewgame") != -1)
            Position("startpos");
        else if (command.IndexOf("go") != -1)
            Go(command.Substring(3));
        else if (command.IndexOf("uci") != -1)
        {
            // Print engine info
            Console.WriteLine("id name UCE");
            Console.WriteLine("id author michabay05");
            Console.WriteLine("uciok");
        }
        else if (command.IndexOf("display") != -1)
            currentBoard.Display();
        else if (command.IndexOf("help") != -1)
            Help();
        else if (command.IndexOf("stop") != -1)
            Stop = true;
        else
            Console.WriteLine($"Unknown command: {command}");
    }

    public static void Loop()
    {
        // Print engine info
        Console.WriteLine("id name UCE");
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
            fen = FEN.position[1];
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
            currentBoard = FEN.Parse(fen);

        int movesStart = cmdArgs.IndexOf("moves", currentIndex);
        if (movesStart != -1)
        {
            string[] listOfMoves = cmdArgs.Substring(movesStart + 5).Trim().Split(" ");
            for (int i = 0; i < listOfMoves.Length; i++)
            {
                int move = ParseMove(listOfMoves[i]);
                if (move == 0)
                    break;

                // Increment repetition
                currentBoard.repetitionIndex++;
                currentBoard.repetitionTable[currentBoard.repetitionIndex] = currentBoard.hashKey;

                Board.MakeMove(ref currentBoard, move, MoveType.allMoves);
            }
        }
    }

    private static int ParseMove(string moveStr)
    {
        MoveList mL = new MoveList(currentBoard);
        MoveGen.Generate(ref mL);
        return mL.Search(moveStr);
    }

    private static void Go(string cmdArgs)
    {
        if (cmdArgs == "")
            return;
        int currentIndex, depth = -1;
        // UCI extension -> took from stockfish
        if ((currentIndex = cmdArgs.IndexOf("perft")) != -1)
        {
            Perft.Test(ref currentBoard, Convert.ToInt32(cmdArgs.Substring(currentIndex + 6)));
            return;
        }
        ParseParam(cmdArgs, "depth", ref depth);

        // Time control related commands
        // Example command: "go depth 6 wtime 180000 btime 100000 binc 1000 winc 1000 movetime 1000 movestogo 40" 
        if (currentBoard.side == 0)
        {
            ParseParam(cmdArgs, "wtime", ref timeLeft);
            ParseParam(cmdArgs, "winc", ref increment);
        }
        else
        {
            ParseParam(cmdArgs, "btime", ref timeLeft);
            ParseParam(cmdArgs, "binc", ref increment);
        }
        ParseParam(cmdArgs, "movetime", ref moveTime);
        ParseParam(cmdArgs, "movestogo", ref movesToGo);

        // If time for each move is set
        if (moveTime != -1)
        {
            timeLeft = moveTime;
            movesToGo = 1;
        }
        startTime = MsTime;
        // If time control is available
        if (timeLeft != -1)
        {
            isTimeControlled = true;
            timeLeft /= movesToGo;
            // Just to be safe, reduce time per move by 50 ms
            timeLeft -= 50;
            stopTime = startTime + timeLeft + increment;
        }

        // If depth not set, set default value
        if (depth == -1)
            depth = Search.MAX_PLY; // i.e. 64 plies
        Search.SearchMove(ref currentBoard, depth);
    }

    private static void ParseParam(string cmdArgs, string cmdName, ref int output)
    {
        int currentIndex, nextSpaceInd;
        string param;
        if ((currentIndex = cmdArgs.IndexOf(cmdName)) != -1 && cmdArgs.Length >= cmdName.Length + 1)
        {
            param = cmdArgs.Substring(currentIndex + cmdName.Length + 1);
            if ((nextSpaceInd = param.IndexOf(" ")) != -1)
                param = param.Substring(0, nextSpaceInd);
            output = Convert.ToInt32(param);
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

    public static void Communicate()
    {
        // Set stop to true, when time is up
        if (isTimeControlled && MsTime >= stopTime)
            Stop = true;
    }
}
