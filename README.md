# UCE - Unique Chess Engine
This is a bitboard-based chess engine written in C#. This engine was created by following the "Bitboard Chess Engine in C" series by the Chess Programming YouTube Channel.

## Features Implemented
- Bitboard representation
- Pre-calculated attack tables
- Magic bitboards to calculate moves for sliding pieces
- FEN parsing and generating
- UCI protocol
- Transposition tables
- Performance Test - Perft
- Copy/Make Approach
- Iterative deepening
- PV/killer/history move ordering
- Move encoding as integers
- Negamax with alpha-beta pruning
- Late Move Reduction(LMR) and Null Move Pruning(NMP)

## Resources used
- Bitboard Chess Engine YouTube PlayList
	- [YouTube Playlist](https://www.youtube.com/playlist?list=PLmN0neTso3Jxh8ZIylk74JpwfiWNI76Cs) 
	- [GitHub](https://github.com/maksimKorzh/chess_programming/tree/master/src/bbc)
		- [Used resources](https://github.com/maksimKorzh/chess_programming/tree/master/resources)
- [List of UCI Protocol commands](https://gist.github.com/aliostad/f4470274f39d29b788c1b09519e67372)
- [TSCP - Tom's Simple Chess Engine](https://github.com/terredeciels/TSCP)
- [CPW Evaluations Page](https://www.chessprogramming.org/Evaluation)
	- [SimplifiedEvaluations for Engines](https://www.chessprogramming.org/Simplified_Evaluation_Function)
- [Bill Jordan's Chess Engine](https://github.com/billjordanchess/Bitboard-Chess)
- Web archives
	- [Bruce Moreland's website](https://web.archive.org/web/20030802112417/http://www.brucemo.com/compchess/programming/index.htm)
	- [Tord Romstad's Glaurung Chess Engine article on Late Move Reductions](https://web.archive.org/web/20150212051846/http://www.glaurungchess.com/lmr.html)