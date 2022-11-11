namespace Nuclear.src;

public enum Direction : int
{
    N = 8,
    S = -8,
    W = -1,
    E = 1,
    NE = 9,      // NORTH + EAST
    NW = 7,      // NORTH + WEST
    SE = -7,     // SOUTH + EAST
    SW = -9,     // SOUTH + WEST
    NE_N = 17,   // 2(NORTH) + EAST -> 'KNIGHT ONLY'
    NE_E = 10,   // NORTH + 2(EAST) -> 'KNIGHT ONLY'
    NW_N = 15,   // 2(NORTH) + WEST -> 'KNIGHT ONLY'
    NW_W = 6,    // NORTH + 2(WEST) -> 'KNIGHT ONLY'
    SE_S = -15,  // 2(SOUTH) + EAST -> 'KNIGHT ONLY'
    SE_E = -6,   // SOUTH + 2(EAST) -> 'KNIGHT ONLY'
    SW_S = -17,  // 2(SOUTH) + WEST -> 'KNIGHT ONLY'
    SW_W = -10,  // SOUTH + 2(WEST) -> 'KNIGHT ONLY'
}
