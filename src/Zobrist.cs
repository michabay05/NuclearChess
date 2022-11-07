namespace UCE.src;

class Zobrist
{
    public static ulong[,] pieceKeys = new ulong[12, 64]; // [piece, square]
    public static ulong[] enPassantKeys = new ulong[64]; // [square]
    public static ulong[] castlingKeys = new ulong[16]; // [castling rights variations] => (0b1111): 1 << 4 
    public static ulong sideKey;

    public static void InitKeys()
    {
        // Init keys with random 64-bit numbers
        for (int piece = 0; piece <= 11; piece++)
        {
            for (int sq = 0; sq < 64; sq++)
                pieceKeys[piece, sq] = Magics.Random64bit();
        }

        for (int sq = 0; sq < 64; sq++)
            enPassantKeys[sq] = Magics.Random64bit();
        for (int i = 0; i < 16; i++)
            castlingKeys[i] = Magics.Random64bit();
        sideKey = Magics.Random64bit();
    }

    // Generates a position ID or hash key for a certain position
    // **NOTE**: collisions could occur; two positions represented by the same hash key
    public static ulong GenHashKey(ref Board board)
    {
        ulong finalHash = 0L;
        // Stores a copy of a piece's bitboard
        ulong bitboard = 0L;
        for (int piece = 0; piece <= 11; piece++)
        {
            int square;
            bitboard = board.bitPieces[piece];
            // Loop over every piece of current piece type
            while (bitboard != 0)
            {
                square = BitUtil.GetLs1bIndex(bitboard);
                // XOR with random 64-bit constant
                finalHash ^= pieceKeys[piece, square];
                BitUtil.PopBit(ref bitboard, square);
            }
        }
        // Hash enpassant square
        if (board.enPassant != -1)
            finalHash ^= enPassantKeys[board.enPassant];
        // Hash castling right
        finalHash ^= castlingKeys[board.castling];
        // If side to move is white, don't hash side to move
        // If side to move is black, hash side to move
        if (board.side == 1)
            finalHash ^= sideKey;
        return finalHash;
    }
}
