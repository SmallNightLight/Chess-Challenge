﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

public class PieceTableGenerator
{
    public enum ScoreType
    {
        PawnMG, KnightMG, BishopMG, RookMG, QueenMG, KingMG,
        PawnEG, KnightEG, BishopEG, RookEG, QueenEG, KingEG
    }

    private static int[] mg_pawn_table =  {
0, 0, 0, 0, 0, 0, 0, 0,
66, 97, 83, 102, 104, 83, 82, 66,
24, 32, 40, 52, 54, 64, 52, 30,
-6, -3, 0, 3, 23, 25, 20, -6,
-24, -10, -13, 26, 28, -4, 6, -24,
-10, -15, -14, 14, 12, -11, 24, -10,
5, 2, -8, -27, -30, 20, 22, 8,
0, 0, 0, 0, 0, 0, 0, 0,
 };

    private static int[] eg_pawn_table = {
0, 0, 0, 0, 0, 0, 0, 0,
179, 173, 168, 123, 116, 129, 175, 186,
115, 121, 89, 69, 59, 45, 89, 90,
44, 34, 15, 12, 10, 0, 34, 44,
20, 17, -1, -4, -6, -4, 6, 2,
14, 15, -2, 10, 3, 0, 4, -4,
19, 19, 8, 13, 14, 5, 3, -3,
0, 0, 0, 0, 0, 0, 0, 0,
    };

    private static int[] mg_knight_table = {
-162, -101, -40, -14, 30, -56, -95, -132,
-24, 2, 48, 44, 47, 48, 9, 21,
4, 39, 54, 70, 86, 80, 66, 29,
4, 6, 37, 61, 40, 48, 8, -8,
-12, 3, 18, 20, 29, 25, 23, -5,
-29, -7, 9, 10, 20, 11, 15, -16,
-42, -31, -14, -2, -1, 1, -12, -15,
-86, -30, -48, -31, -28, -16, -29, -81,
    };

    private static int[] eg_knight_table = {
-65, -28, -13, -18, -21, -34, -21, -101,
-28, -9, -7, -2, -16, -27, -17, -46,
-16, -2, 18, 17, -1, -5, -13, -25,
-4, 18, 28, 31, 32, 25, 18, -13,
-1, 8, 31, 31, 34, 23, 7, -8,
-17, 2, 10, 24, 23, 8, -4, -17,
-26, -12, -2, 4, 2, -4, -20, -19,
-34, -44, -15, -11, -11, -21, -39, -42,
    };

    private static int[] mg_bishop_table = {
-12, -34, -23, -64, -65, -37, -10, -48,
-6, 25, 11, 0, 28, 36, 19, 16,
6, 32, 40, 54, 46, 73, 53, 40,
-3, 10, 36, 46, 42, 38, 14, 1,
-7, 8, 13, 34, 32, 15, 9, -1,
7, 13, 13, 15, 17, 12, 13, 19,
7, 11, 19, -1, 5, 20, 28, 11,
-24, 4, -9, -19, -16, -18, 8, -20,
    };

    private static int[] eg_bishop_table = {
-24, 4, 3, 17, 14, 4, -2, -20,
-15, 2, 7, 9, 2, -1, 7, -21,
12, 7, 16, 7, 11, 13, 6, 3,
9, 26, 19, 33, 24, 22, 19, 5,
3, 20, 29, 24, 25, 24, 16, -6,
1, 13, 20, 21, 24, 21, 4, -8,
-2, -4, -4, 10, 13, 1, 2, -22,
-19, -2, -23, 0, -3, -4, -17, -27,
    };

    private static int[] mg_rook_table = {
32, 24, 34, 39, 58, 79, 54, 70,
10, 7, 31, 31, 33, 30, 28, 36,
-11, 11, 11, 18, 45, 27, 22, 24,
-29, -12, -8, 0, 3, 6, 12, 8,
-46, -43, -34, -20, -19, -34, -9, -20,
-53, -42, -33, -34, -29, -31, 5, -18,
-57, -42, -29, -31, -27, -24, -5, -40,
-36, -34, 14, 26, 24, 15, -13, -31,
    };

    private static int[] eg_rook_table = {
27, 32, 38, 35, 26, 14, 18, 15,
27, 39, 40, 32, 31, 18, 13, 1,
28, 29, 31, 28, 15, 10, 2, -2,
30, 27, 35, 31, 19, 14, 8, 2,
23, 27, 30, 28, 23, 22, 8, 3,
18, 18, 18, 22, 18, 10, 10, 10,
5, 17, 19, 20, 12, 8, -2, 5,
2, 8, 26, 25, 17, 12, 8, 2,
    };

    private static int[] mg_queen_table = {
-20, 15, 30, 50, 55, 50, 15, -20,
10, -19, -15, -25, -19, 31, 9, 20,
0, -1, 3, 18, 24, 65, 69, 64,
2, -4, -5, -6, 2, 8, -4, 2,
-5, -2, -4, -8, -5, -7, 5, -5,
-15, -5, -12, -10, -8, -1, 11, 3,
-17, -9, 0, 1, 0, 9, 14, 21,
-17, -27, -19, -3, -14, -29, -12, -14,
    };

    private static int[] eg_queen_table = {
29, 46, 58, 47, 42, 36, -3, 35,
3, 39, 74, 96, 109, 70, 52, 13,
8, 27, 63, 65, 84, 59, 22, 8,
19, 45, 55, 79, 94, 81, 65, 41,
22, 44, 53, 78, 70, 61, 43, 30,
4, 18, 47, 39, 45, 37, 17, 3,
4, 7, 2, 11, 13, -15, -41, -63,
-1, 1, 4, -12, 2, 0, -24, -28,
    };

    private static int[] mg_king_table = {
-120, -102, -100, -90, -90, -80, -90, -104,
-64, -9, -47, -90, -90, 80, -80, -50,
-78, 27, -52, -58, -42, -32, -20, -34,
-62, -70, -77, -121, -117, -83, -77, -109,
-55, -64, -94, -129, -127, -98, -94, -123,
-23, -9, -67, -81, -76, -72, -27, -44,
60, 18, 1, -38, -37, -17, 24, 34,
50, 78, 51, -54, 15, -27, 58, 59,
    };

    private static int[] eg_king_table = {
-63, -33, -31, -2, -13, -6, 1, -45,
-9, 16, 24, 13, 27, 39, 37, 11,
4, 38, 39, 45, 47, 43, 44, 19,
-6, 37, 43, 55, 56, 52, 44, 22,
-18, 34, 38, 53, 54, 43, 30, 14,
-24, 2, 22, 34, 34, 27, 8, -5,
-43, -15, -2, 11, 14, 5, -15, -34,
-73, -57, -38, -18, -44, -20, -48, -76,
    };

    // None, Pawn, Knight, Bishop, Rook, Queen, King 
    private static readonly short[] PieceValues = { 77, 302, 310, 434, 890, 0, // Middlegame
                                                    109, 331, 335, 594, 1116, 0, }; // Endgame

    public static void Generate()
    {
        /*
        Important note!
        Due to certain specifics which I don't fully understand, 
        king PST values (or any PST values where the piece value is also zero, for that matter)
        will throw an exception when unpacking! 
        Please sanitize your inputted king square tables by changing all zeroes to either 1 or -1 to minimize error without raising any exceptions
        */

        List<int[]> table = new()
        {
            mg_pawn_table,
            mg_knight_table,
            mg_bishop_table,
            mg_rook_table,
            mg_queen_table,
            mg_king_table,

            eg_pawn_table,
            eg_knight_table,
            eg_bishop_table,
            eg_rook_table,
            eg_queen_table,
            eg_king_table
        };

        Console.WriteLine("Packed table:\n");
        decimal[] packedData = PackData(table);

        Console.Write(packedData);

        Console.WriteLine("Unpacked table:\n");
        int[][] unpackedData = UnpackData(packedData);

        PrintUnpackedData(unpackedData);
    }

    private const int tableSize = 64;
    private const int tableCount = 12;

    // Packs data in the following form
    // Square data in the first 12 bytes of each decimal (1 byte per piece type, 6 per gamephase)
    private static decimal[] PackData(List<int[]> tablesToPack)
    {
        decimal[] packedData = new decimal[tableSize];

        for (int square = 0; square < tableSize; square++)
        {
            // Pack all sets for this square into a byte array
            byte[] packedSquares = new byte[tableCount];
            for (int set = 0; set < tableCount; set++)
            {
                int[] setToPack = tablesToPack[set];
                sbyte valueToPack = (sbyte)Math.Round(setToPack[square] / 2f);
                packedSquares[set] = (byte)(valueToPack & 0xFF);
            }

            // Create a new decimal based on the packed values for this square
            int[] thirds = new int[4];
            for (int i = 0; i < 3; i++)
            {
                thirds[i] = BitConverter.ToInt32(packedSquares, i * 4);
            }
            packedData[square] = new(thirds);
        }

        // Print the newly created table
        Console.Write("{ ");
        for (int square = 0; square < tableSize; square++)
        {
            if (square % 8 == 0)
                Console.WriteLine();
            Console.Write(packedData[square] + "m, ");
        }
        Console.WriteLine("\n};");

        return packedData;
    }

    // Unpacks a packed square table to be accessed with
    // pestoUnpacked[square][pieceType]
    private static int[][] UnpackData(decimal[] tablesToUnpack)
    {
        var pestoUnpacked = tablesToUnpack.Select(packedTable =>
        {
            int pieceType = 0;
            return new System.Numerics.BigInteger(packedTable).ToByteArray().Take(12)
                    .Select(square => (int)((sbyte)square * 2f))
                .ToArray();
        }).ToArray();

        return pestoUnpacked;
    }

    private static void PrintUnpackedData(int[][] unpackedData)
    {
        // Print all of the unpacked values
        for (int type = 0; type < tableCount; type++)
        {
            Console.WriteLine("\n\nTable for type: " + (ScoreType)type);
            for (int square = 0; square < tableSize; square++)
            {
                if (square % 8 == 0)
                    Console.WriteLine();

                Console.Write(unpackedData[square][type] + ", ");
            }
            Console.WriteLine();
        }
    }
}