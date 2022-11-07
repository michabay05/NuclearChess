namespace UCE.src;


class Magics
{
    public static ulong[] bishopMagics =
    {
        0x3060510108088480ul, 0x2102081921020100ul, 0x204081204420020ul,
        0x844850000000Aul, 0x804104448021488ul, 0x2689094840000208ul,
        0x2120120080020ul, 0xA101010090010800ul, 0x89885010D0810044ul,
        0x802221061020088ul, 0x24480A20C130ul, 0x4000022A02006000ul,
        0x880011040000858ul, 0x10402408010ul, 0x1011020109213000ul,
        0x8000080A8082210ul, 0x8009000408280812ul, 0x442211004481880ul,
        0x402082C48020080ul, 0x80004021124C0ul, 0x24000480A04200ul,
        0x82000288040220ul, 0x204D880208245A01ul, 0x2200192080201ul,
        0x8008460088101000ul, 0x12200608980080ul, 0x240A80044080068ul,
        0x4041004014040002ul, 0x880101004010400Aul, 0x10050002088240ul,
        0x10104028100C800ul, 0x8011004002004458ul, 0x2204405220801ul,
        0xA188010800050800ul, 0x40C1400208100ul, 0x10014A0180080080ul,
        0x1404024200040108ul, 0x810202010083ul, 0x40408904400A1ul,
        0x8850100404C00ul, 0x621015140015014ul, 0x4E222100200B400ul,
        0x2008101090000800ul, 0x101204A014400800ul, 0x6060404101000210ul,
        0x4002041820200200ul, 0x1004440404000040ul, 0x4401010102080104ul,
        0x4A0804408010ul, 0x4504430080000ul, 0xA000002402084110ul,
        0x10000205040080ul, 0x80490440000ul, 0x2202420A0400ul,
        0x8600142020001ul, 0x4F2622202060090ul, 0x100120904200440ul,
        0x4820101291102ul, 0x10001442080480ul, 0xA00601148800ul,
        0x4102600040108250ul, 0x2000604070040122ul, 0x10A042028024484ul,
        0x2220082080808600ul,
    };
    public static ulong[] rookMagics =
    {
        0x5080008010400022ul, 0x90C010004000A000ul, 0x60800B8010022000ul,
        0x200042008104200ul, 0x1200040802001120ul, 0x8200040200100801ul,
        0x400088410221128ul, 0x200004200240081ul, 0x802040008000ul,
        0x21404000201000ul, 0x8401001100402000ul, 0x808010000800ul,
        0x6008808088000400ul, 0x2006002200102804ul, 0x208E002200410804ul,
        0x3000100038042ul, 0x128208010804000ul, 0x800404010002006ul,
        0x808020001000ul, 0x1010010002008ul, 0x84008008000482ul,
        0x4020808004000200ul, 0x205240008010230ul, 0xA840200090440A4ul,
        0x80008080204000ul, 0x200040100040ul, 0x8410080020040022ul,
        0x240100080800800ul, 0x120040080800800ul, 0x4000200801108440ul,
        0x1810080400011002ul, 0x700204200140081ul, 0x20E0400031800080ul,
        0xE320802101004001ul, 0x1000801000802008ul, 0x200828802801000ul,
        0x88001C01800880ul, 0x8101000401000802ul, 0x1000401000200ul,
        0xA000004082000401ul, 0x844000228002ul, 0x4040100800202000ul,
        0x4290080400202000ul, 0x3150C84201320020ul, 0x10040008008080ul,
        0x6001008520084ul, 0x102000801020004ul, 0x4000A04084020001ul,
        0x4000482200810200ul, 0x1420400890200080ul, 0x10008020001080ul,
        0x1041000810002100ul, 0x40080080080ul, 0x1102000411080200ul,
        0x1000402000100ul, 0x10080440200ul, 0x820420010810Aul,
        0x261002440008411ul, 0x7228104100082001ul, 0x400100008200501ul,
        0x20010A0080482ul, 0x202000408104182ul, 0x8000080230010084ul,
        0x5000808104002042ul
    };

    private static int randomState = 1804289383;
    public static int Random32bit()
    {
        int number = randomState;

        // XOR shift algorithm
        number ^= number << 13;
        number ^= number >> 17;
        number ^= number << 5;

        // update random number state
        randomState = number;

        // return random number
        return number;
    }

    public static ulong Random64bit()
    {
        ulong rand1, rand2, rand3, rand4;
        rand1 = (ulong)(Random32bit() & 0xFFFF);
        rand2 = (ulong)(Random32bit() & 0xFFFF);
        rand3 = (ulong)(Random32bit() & 0xFFFF);
        rand4 = (ulong)(Random32bit() & 0xFFFF);
        return rand1 | (rand2 << 16) | (rand3 << 32) | (rand4 << 48);
    }

    private static ulong RandomFew64bit() => Random64bit() & Random64bit() & Random64bit();

    private static ulong FindMagicNumber(int sq, int relevant_bits, int piece)
    {
        if (piece != Piece.BISHOP && piece != Piece.ROOK)
        {
            Console.WriteLine("findMagicNumber():\n\tPiece must be either bishop or rook");
            return 0;
        }
        // 4096 = 2^12 (12 is the max relevant bits for both rooks and bishops
        ulong[] usedAttacks = new ulong[4096];
        ulong[] occupancies = new ulong[4096];
        ulong[] attacks = new ulong[4096];
        ulong magicNumber, possibleOccupancy = (piece == Piece.BISHOP) ? Precalculate.GenBishopOccupancy(sq) : Precalculate.GenRookOccupancy(sq);
        int occupancyIndicies = 1 << relevant_bits;
        for (int count = 0; count < occupancyIndicies; count++)
        {
            occupancies[count] = Precalculate.setOccupancy(count, relevant_bits, possibleOccupancy);
            attacks[count] = (piece == Piece.BISHOP) ? Precalculate.bishopAttacks[sq, occupancies[count]] : Precalculate.GenRookAttacks(sq, occupancies[count]);
        }

        for (int randCount = 0; randCount < 100_000_000; randCount++)
        {
            magicNumber = RandomFew64bit();
            if (BitUtil.CountBits((possibleOccupancy * magicNumber) & 0xFF00000000000000) < 6)
                continue;
            Array.Fill<ulong>(usedAttacks, 0);
            int count;
            bool failFlag;
            for (count = 0, failFlag = false; !failFlag && count < occupancyIndicies; count++)
            {
                int magicInd = (int)((occupancies[count] * magicNumber) >> (64 - relevant_bits));
                if (usedAttacks[magicInd] == 0)
                    usedAttacks[magicInd] = attacks[count];
                else if (usedAttacks[magicInd] != attacks[count])
                    failFlag = true;
            }
            if (!failFlag) return magicNumber;
        }
        Console.WriteLine($"Magic FAILED -> {(Squares)sq}");
        return 0;
    }

    public static void InitMagics()
    {
        int square;
        Console.WriteLine("{");
        for (square = 0; square <= 63; ++square)
            Console.WriteLine($"  0x{FindMagicNumber(square, Precalculate.rookRelevantBits[square], Piece.ROOK):X}ul,");
        Console.WriteLine("};\n\n");

        Console.WriteLine("{");
        for (square = 0; square <= 63; ++square)
            Console.WriteLine($"  0x{FindMagicNumber(square, Precalculate.bishopRelevantBits[square], Piece.BISHOP):X}ul,");
        Console.WriteLine("};\n\n");
    }

    public static ulong GetSlidingAttack(int sq, int piece, ulong blockerBoard)
    {
        if (piece != Piece.BISHOP && piece != Piece.ROOK)
        {
            Console.WriteLine("findMagicNumber():\n\tPiece must be either bishop or rook");
            return 0ul;
        }
        ulong output = 0ul;
        if (piece == Piece.BISHOP)
        {
            blockerBoard &= Precalculate.bishopOccMask[sq];
            blockerBoard *= bishopMagics[sq];
            blockerBoard >>= (64 - Precalculate.bishopRelevantBits[sq]);
            output = Precalculate.bishopAttacks[sq, blockerBoard];
        }
        else
        {
            blockerBoard &= Precalculate.rookOccMask[sq];
            blockerBoard *= rookMagics[sq];
            blockerBoard >>= (64 - Precalculate.rookRelevantBits[sq]);
            output = Precalculate.rookAttacks[sq, blockerBoard];
        }
        return output;
    }

    public static ulong GetBishopAttack(int sq, ulong blockerBoard)
    {
        blockerBoard &= Precalculate.bishopOccMask[sq];
        blockerBoard *= bishopMagics[sq];
        blockerBoard >>= (64 - Precalculate.bishopRelevantBits[sq]);
        return Precalculate.bishopAttacks[sq, blockerBoard];
    }

    public static ulong GetRookAttack(int sq, ulong blockerBoard)
    {
        blockerBoard &= Precalculate.rookOccMask[sq];
        blockerBoard *= rookMagics[sq];
        blockerBoard >>= (64 - Precalculate.rookRelevantBits[sq]);
        return Precalculate.rookAttacks[sq, blockerBoard];
    }

    public static ulong GetQueenAttack(int sq, ulong blockBoard) => GetBishopAttack(sq, blockBoard) | GetRookAttack(sq, blockBoard);
}
