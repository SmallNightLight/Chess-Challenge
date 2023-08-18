using ChessChallenge.API;
using Raylib_cs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class BotB1C : IChessBot
{
    //Weights

    private int[] _pawnPositionValues = new int[64]
    {
         0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
         5,  5, 10, 25, 25, 10,  5,  5,
         0,  0,  0, 20, 20,  0,  0,  0,
         5, -5,-10,  0,  0,-10, -5,  5,
         5, 10, 10,-20,-20, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

    private int[] _knightPositionValues = new int[64]
    {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50
    };

    private int[] _bishopPositionValues = new int[64]
    {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20
    };

    private int[] _rookPositionValues = new int[64]
    {
         0,  0,  0,  0,  0,  0,  0,  0,
         5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
         0,  0,  0,  5,  5,  0,  0,  0
    };

    private int[] _queenPositionValues = new int[64]
    {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
         -5,  0,  5,  5,  5,  5,  0, -5,
          0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };

    private int[] _kingPositionValuesEarly = new int[64]
    {
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
         20, 20,  0,  0,  0,  0, 20, 20,
         20, 30, 10,  0,  0, 10, 30, 20
    };
    
    private int[] _kingPositionValuesLate = new int[64]
    {
        -50,-40,-30,-20,-20,-30,-40,-50,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -50,-30,-30,-30,-30,-30,-30,-50
    };


    private int[] _iPawnPositionValueE = new int[64] 
    {
         0,   0,   0,   0,   0,   0,  0,   0,
         98, 134,  61,  95,  68, 126, 34, -11,
         -6,   7,  26,  31,  65,  56, 25, -20,
        -14,  13,   6,  21,  23,  12, 17, -23,
        -27,  -2,  -5,  12,  17,   6, 10, -25,
        -26,  -4,  -4, -10,   3,   3, 33, -12,
        -35,  -1, -20, -23, -15,  24, 38, -22,
          0,   0,   0,   0,   0,   0,  0,   0
    };

    private int[] _iKnightPositionValueE = new int[64]
    {
        -167, -89, -34, -49,  61, -97, -15, -107,
         -73, -41,  72,  36,  23,  62,   7,  -17,
         -47,  50,  20,  43,  65, 76,  10,   -20,
          -9,  3,  10,  53,  37,  40,  8,   -5,
         -13,   4,  16,  13,  28,  19,  21,   -8,
         -23,  -9,  12,  10,  19,  17,  25,  -16,
         -29, -53, -12,  -3,  -1,  18, -14,  -19,
        -105, -21, -58, -33, -17, -28, -19,  -23
    };

    private int[] _iBishopPositionValueE = new int[64]
    {
        -29,   4, -82, -37, -25, -42,   7,  -8,
        -26,  16, -18, -13,  30,  59,  18, -47,
        -16,  37,  43,  40,  35,  50,  37,  -2,
         -4,   5,  19,  50,  37,  37,   7,  -2,
         -6,  13,  13,  26,  34,  12,  10,   4,
          0,  15,  15,  15,  14,  27,  18,  10,
          4,  15,  16,   0,   7,  21,  33,   1,
        -33,  -3, -14, -21, -13, -12, -39, -21
    };

    private int[] _iRookPositionValueE = new int[64]
    {
        32,  42,  32,  51, 63,  9,  31,  43,
         27,  32,  58,  62, 80, 67,  26,  44,
         -5,  19,  26,  36, 17, 45,  61,  16,
        -24, -11,   7,  26, 24, 35,  -8, -20,
        -36, -26, -12,  -1,  9, -7,   6, -23,
        -45, -25, -16, -17,  3,  0,  -5, -33,
        -44, -16, -20,  -9, -1, 11,  -6, -31,
        -19, -13,   1,  17, 16, 10, -37, -26
    };

    private int[] _iQueenPositionValueE = new int[64]
    {
        -28,   0,  29,  12,  59,  44,  43,  45,
        -24, -39,  -5,   1, -16,  57,  28,  54,
        -13, -17,   7,   8,  29,  56,  47,  57,
        -27, -27, -16, -16,  -1,  17,  -2,   1,
         -9, -26,  -9, -10,  -2,  -4,   3,  -3,
        -14,   2, -11,  -2,  -5,   2,  14,   5,
        -25,  -8,  11,   2,   8,  15,  -3,   1,
         -1, -18,  -9,  10, -15, -25, -31, -50
    };

    private int[] _iKingPositionValueE = new int[64]
    {
        -65,  23,  16, -15, -56, -34,   2,  13,
         29,  -1, -20,  -7,  -8,  -4, -38, -29,
         -9,  24,   2, -16, -20,   6,  22, -22,
        -17, -20, -12, -27, -30, -25, -14, -36,
        -49,  -1, -27, -39, -46, -44, -33, -51,
        -14, -14, -22, -46, -44, -30, -15, -27,
          1,   7,  -8, -64, -43, -16,   9,   8,
        -15,  36,  12, -54,   8, -28,  24,  14
    };

    private int[] _iPawnPositionValueL = new int[64]
    {
        0,   0,   0,   0,   0,   0,   0,   0,
        178, 173, 158, 134, 147, 132, 165, 187,
         94, 100,  85,  67,  56,  53,  82,  84,
         32,  24,  13,   5,  -2,   4,  17,  17,
         13,   9,  -3,  -7,  -7,  -8,   3,  -1,
          4,   7,  -6,   1,   0,  -5,  -1,  -8,
         13,   8,   8,  10,  13,   0,   2,  -7,
          0,   0,   0,   0,   0,   0,   0,   0
    };

    private int[] _iKnightPositionValueL = new int[64]
    {
        -58, -38, -13, -28, -31, -27, -63, -99,
        -25,  -8, -25,  -2,  -9, -25, -24, -52,
        -24, -20,  10,   9,  -1,  -9, -19, -41,
        -17,   3,  22,  22,  22,  11,   8, -18,
        -18,  -6,  16,  25,  16,  17,   4, -18,
        -23,  -3,  -1,  15,  10,  -3, -20, -22,
        -42, -20, -10,  -5,  -2, -20, -23, -44,
        -29, -51, -23, -15, -22, -18, -50, -64
    };

    private int[] _iBishopPositionValueL = new int[64]
    {
        -14, -21, -11,  -8, -7,  -9, -17, -24,
         -8,  -4,   7, -12, -3, -13,  -4, -14,
          2,  -8,   0,  -1, -2,   6,   0,   4,
         -3,   9,  12,   9, 14,  10,   3,   2,
         -6,   3,  13,  19,  7,  10,  -3,  -9,
        -12,  -3,   8,  10, 13,   3,  -7, -15,
        -14, -18,  -7,  -1,  4,  -9, -15, -27,
        -23,  -9, -23,  -5, -9, -16,  -5, -17
    };

    private int[] _iRookPositionValueL = new int[64]
    {
        13, 10, 18, 15, 12,  12,   8,   5,
        11, 13, 13, 11, -3,   3,   8,   3,
         7,  7,  7,  5,  4,  -3,  -5,  -3,
         4,  3, 13,  1,  2,   1,  -1,   2,
         3,  5,  8,  4, -5,  -6,  -8, -11,
        -4,  0, -5, -1, -7, -12,  -8, -16,
        -6, -6,  0,  2, -9,  -9, -11,  -3,
        -9,  2,  3, -1, -5, -13,   4, -20,
    };

    private int[] _iQueenPositionValueL = new int[64]
    {
        -9,  22,  22,  27,  27,  19,  10,  20,
        -17,  20,  32,  41,  58,  25,  30,   0,
        -20,   6,   9,  49,  47,  35,  19,   9,
          3,  22,  24,  45,  57,  40,  57,  36,
        -18,  28,  19,  47,  31,  34,  39,  23,
        -16, -27,  15,   6,   9,  17,  10,   5,
        -22, -23, -30, -16, -16, -23, -36, -32,
        -33, -28, -22, -43,  -5, -32, -20, -41
    };

    private int[] _iKingPositionValueL = new int[64]
    {
        -74, -35, -18, -18, -11,  15,   4, -17,
        -12,  17,  14,  17,  17,  38,  23,  11,
         10,  17,  23,  15,  20,  45,  44,  13,
         -8,  22,  24,  27,  26,  33,  26,   3,
        -18,  -4,  21,  24,  27,  23,   9, -11,
        -19,  -3,  11,  21,  23,  16,   7,  -9,
        -27, -11,   4,  13,  14,   4,  -5, -17,
        -53, -34, -21, -11, -28, -14, -24, -43
    };

    //Use deciamls - -> max
    //use 12 maps (each piece, early + late)
    //use 8bits per value per quare: 2 decimals per map
    //32 squares per map //symmetry
    //-> 12 maps * 2 decimals * 2 tokens = 48 tokens

    private int _gamePhase;

    private int[] _iPieceValueE = { 0, 82, 337, 365, 477, 1025, 10000 };
    private int[] _iPieceValueL = { 0, 94, 281, 297, 512, 936, 10000 };

    private int[] _pieceValues = { 0, 100, 300, 325, 500, 900, 10000 };
    private int _drawTreshhold = -100;
    private int _minimumDepth = 5;
    private int _maximumDepth = 9;
    private int _quiescenceDepth = 3;

    //Other Variables
    private Timer _timer;
    private float _timeThisTurn;

    private bool _isEndGame;

    private Dictionary<ulong, (byte, short)> _transpositionTable = new Dictionary<ulong, (byte, short)>();

    //14 bytes per entry, likely will align to 16 bytes due to padding (if it aligns to 32, recalculate max TP table size)
    public struct Transposition
    {
        public ulong zobristHash;
        public Move move;
        public short evaluation;
        public sbyte depth;
        public byte flag;
    };

    private Transposition[] _iTranspositionTable;


    private (ulong, short, sbyte)[] _iiTranspositionTable = new(ulong, short, sbyte)[0x800000];

    //Systems
    bool _useMoveEvaluation = true;
    bool _useAlphaBetaPruning = true;
    bool _useQuiescenceSearch = true;
    bool _useIterativeDeepening = true;
    bool _useEvaluateMoveOrdering = true;
    bool _useTimeManagment = true;
    bool _useImprovedPieceValues = true;
    bool _useTranspositionTable = true; //Alpha Beta pruning needs to be true
    bool _useOptimizedTranspositionTable = false; //Alpha Beta pruning needs to be true
    bool _useOptimizedTranspositionTable2 = true; //Alpha Beta pruning needs to be true
    bool _useLiteBlue = true;

    //Debugging
    int _searches;
    int _quiescenceSearches;
    int _newEntries;


    private readonly short[] pvm = { 82, 337, 365, 477, 1025, 20000, // Middlegame
                                     94, 281, 297, 512, 936, 20000};

    private readonly decimal[] PackedPestoTables = {
        63746705523041458768562654720m, 71818693703096985528394040064m, 75532537544690978830456252672m, 75536154932036771593352371712m, 76774085526445040292133284352m, 3110608541636285947269332480m, 936945638387574698250991104m, 75531285965747665584902616832m,
        77047302762000299964198997571m, 3730792265775293618620982364m, 3121489077029470166123295018m, 3747712412930601838683035969m, 3763381335243474116535455791m, 8067176012614548496052660822m, 4977175895537975520060507415m, 2475894077091727551177487608m,
        2458978764687427073924784380m, 3718684080556872886692423941m, 4959037324412353051075877138m, 3135972447545098299460234261m, 4371494653131335197311645996m, 9624249097030609585804826662m, 9301461106541282841985626641m, 2793818196182115168911564530m,
        77683174186957799541255830262m, 4660418590176711545920359433m, 4971145620211324499469864196m, 5608211711321183125202150414m, 5617883191736004891949734160m, 7150801075091790966455611144m, 5619082524459738931006868492m, 649197923531967450704711664m,
        75809334407291469990832437230m, 78322691297526401047122740223m, 4348529951871323093202439165m, 4990460191572192980035045640m, 5597312470813537077508379404m, 4980755617409140165251173636m, 1890741055734852330174483975m, 76772801025035254361275759599m,
        75502243563200070682362835182m, 78896921543467230670583692029m, 2489164206166677455700101373m, 4338830174078735659125311481m, 4960199192571758553533648130m, 3420013420025511569771334658m, 1557077491473974933188251927m, 77376040767919248347203368440m,
        73949978050619586491881614568m, 77043619187199676893167803647m, 1212557245150259869494540530m, 3081561358716686153294085872m, 3392217589357453836837847030m, 1219782446916489227407330320m, 78580145051212187267589731866m, 75798434925965430405537592305m,
        68369566912511282590874449920m, 72396532057599326246617936384m, 75186737388538008131054524416m, 77027917484951889231108827392m, 73655004947793353634062267392m, 76417372019396591550492896512m, 74568981255592060493492515584m, 70529879645288096380279255040m,
    };

    private readonly int[][] UnpackedPestoTables = new int[64][];

    public BotB1C()
    {
        _iTranspositionTable = new Transposition[0x800000];


        UnpackedPestoTables = PackedPestoTables.Select(packedTable =>
        {
            int pieceType = 0;
            return decimal.GetBits(packedTable).Take(3).SelectMany(c => BitConverter.GetBytes(c).Select(square => (int)((sbyte)square * 1.461) + pvm[pieceType++])).ToArray();
        }).ToArray();
    }

    public Move Think(Board board, Timer timer)
    {
        _timer = timer;
        Move bestMove = Move.NullMove;

        if (_useImprovedPieceValues)
            _gamePhase = GetGamePhase(board);
        else
            _isEndGame = IsEndGame(board);

        _searches = 0;
        _quiescenceSearches = 0;
        _newEntries = 0;
        int searchDepth = 0;

        if (_useIterativeDeepening)
        {
            if (_useTimeManagment)
            {
                float openingsMoves = Math.Min(board.PlyCount, 15);
                float totalTime = timer.GameStartTimeMilliseconds;

                float factor = 0.6f + (0.04f * openingsMoves);
                float timePerMove = totalTime / 80f;
                _timeThisTurn = Math.Min(timer.MillisecondsRemaining / 25, factor * timePerMove);
            }
            else
            {
                _timeThisTurn = timer.GameStartTimeMilliseconds / 60f;
            }

            Console.WriteLine("Time this turn: " + _timeThisTurn  + "   Time left: " + timer.MillisecondsRemaining);

            List<Move> bestMoveLine;

            for (int depth = 1; depth <= _maximumDepth; depth++)
            {
                List<Move> moveLine = Search(board, depth, _useMoveEvaluation ? EvaluateBoard(board) : 0, -10000, 10000, bestMove, false, new HashSet<Square>(), _quiescenceDepth).Item1;

                if (timer.MillisecondsElapsedThisTurn < _timeThisTurn && moveLine.Count != 0)
                {
                    Move move = moveLine[0];
                    bestMove = move;
                    bestMoveLine = moveLine;
                    searchDepth++;
                }
                else
                {
                    //Console.WriteLine("Break with: " + timer.MillisecondsElapsedThisTurn);
                    break;
                }
            }
        }
        else
        {
            List<Move> bestMoveLine = Search(board, _minimumDepth, _useMoveEvaluation ? EvaluateBoard(board) : 0, -10000, 10000, bestMove, false, new HashSet<Square>(), _quiescenceDepth).Item1;
            bestMove = bestMoveLine[0];
        }

        Console.WriteLine("BotB1C finished at depth: " + searchDepth + " in: " + timer.MillisecondsElapsedThisTurn + " milliseconds, time left: " + timer.MillisecondsRemaining);
        //Console.WriteLine("BotB1C finished at depth: " + searchDepth + " in: " + timer.MillisecondsElapsedThisTurn + " milliseconds, " + " with searches: " + _searches + " and quiescnceSearhces: "  + _quiescenceSearches + " and new entries: " + _newEntries);
        return bestMove;
    }

    private (List<Move>, int) Search(Board board, int depth, int currentEvaluation, int alpha, int beta, Move pvMove, bool quiescenceSearch, HashSet<Square> capturePieces, int quiescenceDepth)
    {
        //Debugging
        if (quiescenceSearch)
            _quiescenceSearches++;
        else
            _searches++;

        //Check for depth and time
        if ((_useTimeManagment && _timer.MillisecondsElapsedThisTurn > _timeThisTurn) || (!quiescenceSearch && depth == 0) || (quiescenceSearch && (quiescenceDepth == 0 || capturePieces.Count() == 0)))
            return (new List<Move>(), currentEvaluation);

        ref Transposition transposition = ref _iTranspositionTable[board.ZobristKey & 0x7FFFFF];
        int startingAlpha = alpha;

        ref var iTransposition = ref _iiTranspositionTable[board.ZobristKey & 0x7FFFFF];

        List<Move> interestingMoves = new List<Move>();

        if (pvMove != Move.NullMove)
            interestingMoves.Add(pvMove);

        if (_useTranspositionTable)
        {
            if (_useOptimizedTranspositionTable)
            {
                if (transposition.zobristHash == board.ZobristKey && transposition.depth >= depth)
                {
                    ////If we have an "exact" score (a < score < beta) just use that
                    //if (transposition.flag == 1)
                    //    //return (new List<Move> { transposition.move }, transposition.evaluation);

                    ////If we have a lower bound better than beta, use that
                    //if (transposition.flag == 2 && transposition.evaluation >= beta)
                    //    return (new List<Move> { transposition.move }, transposition.evaluation);

                    ////If we have an upper bound worse than alpha, use that
                    //if (transposition.flag == 3 && transposition.evaluation <= alpha)
                    //    return (new List<Move> { transposition.move}, transposition.evaluation);

                    if (currentEvaluation >= beta && transposition.evaluation >= beta)
                        return (new List<Move>(), transposition.evaluation); //Beta cut-off
                    if (currentEvaluation <= alpha && transposition.evaluation <= alpha)
                        return (new List<Move>(), transposition.evaluation); //Alpha cut-off
                }
                
                if (transposition.zobristHash == board.ZobristKey)
                {
                    if (!interestingMoves.Contains(transposition.move))
                        interestingMoves.Add(transposition.move);
                }
            }
            else if (_useOptimizedTranspositionTable2)
            {
                if (iTransposition.Item1 == board.ZobristKey && iTransposition.Item3 >= depth)
                {
                    if (currentEvaluation >= beta && iTransposition.Item2 >= beta)
                        return (new List<Move>(), iTransposition.Item2); //Beta cut-off
                    if (currentEvaluation <= alpha && iTransposition.Item2 <= alpha)
                        return (new List<Move>(), iTransposition.Item2); //Alpha cut-off
                }
            }
            else if (_transpositionTable.TryGetValue(board.ZobristKey, out var storedEntry) && storedEntry.Item1 >= depth)
            {
                if (currentEvaluation >= beta && storedEntry.Item2 >= beta)
                    return (new List<Move>(), storedEntry.Item2); //Beta cut-off
                if (currentEvaluation <= alpha && storedEntry.Item2 <= alpha)
                    return (new List<Move>(), storedEntry.Item2); //Alpha cut-off
            }
        }

        Move bestMove = Move.NullMove;
        int bestEvaluation = int.MinValue;
        List<Move> bestMoveLine = new List<Move>();

        Move[] moves;
        
        if (quiescenceSearch)
            moves = board.GetLegalMoves().Where(move => capturePieces.Contains(move.TargetSquare)).ToArray();
        else
            moves = board.GetLegalMoves();


        if (_useEvaluateMoveOrdering)
        {
            //Order moves with Evaluate function
            moves = moves.OrderByDescending(move => interestingMoves.Contains(move)).ThenBy(move => interestingMoves.IndexOf(move)).ThenByDescending(move => EvaluateMove(move, board.IsWhiteToMove)).ToArray();
        }
        else
        {
            //Order moves with simple pieceValue evaluation
            moves = moves.OrderByDescending(move => interestingMoves.Contains(move)).ThenBy(move => interestingMoves.IndexOf(move)).ThenByDescending(move => _pieceValues[(int)move.CapturePieceType]).ToArray();
        }

       
        foreach (Move move in moves)
        {
            board.MakeMove(move);

            //Check for a checkmate
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                bestMove = move;
                bestEvaluation = 10001;
                break;
            }

            int evaluation;
            List<Move> moveLine = new List<Move>();

            //Update the capture Pieces list
            if (capturePieces.Contains(move.StartSquare))
                capturePieces.Remove(move.StartSquare);

            HashSet<Square> updatedCapturePieces = new HashSet<Square>(capturePieces);

            if (move.IsCapture)
                updatedCapturePieces.Add(move.TargetSquare);

            //Ckeck for a draw
            if (board.IsDraw())
            {
                evaluation = _drawTreshhold;
            }
            else
            {
                //Do a new Search
                (moveLine, evaluation)  = Search(board, quiescenceSearch ? depth : depth - 1, -currentEvaluation - EvaluateMove(move, !board.IsWhiteToMove), -beta, -alpha, Move.NullMove, _useQuiescenceSearch && (depth == 1 || quiescenceSearch), updatedCapturePieces, quiescenceDepth - (quiescenceSearch ? 1 : 0));
                evaluation *= -1;
            }

            board.UndoMove(move);

            //Set best move
            if (evaluation > bestEvaluation)
            {
                bestMove = move;
                bestEvaluation = evaluation;
                bestMoveLine = moveLine;
            }

            if (_useAlphaBetaPruning)
            {
                //Set alpha
                alpha = Math.Max(alpha, bestEvaluation);

                //Check if can cut-off
                if (beta <= alpha)
                {
                    //Cut-off: This position is already worse, so dont search further
                    break;
                }
            }
        }

        if (!quiescenceSearch && _useTranspositionTable)
        {
            _newEntries++;
            if (_useOptimizedTranspositionTable)
            {
                transposition.evaluation = (short)bestEvaluation;
                transposition.zobristHash = board.ZobristKey;
                transposition.move = bestMove;
                if (bestEvaluation < startingAlpha)
                    transposition.flag = 3; //upper bound
                else if (bestEvaluation >= beta)
                {
                    transposition.flag = 2; //lower bound
                }
                else transposition.flag = 1; //"exact" score
                transposition.depth = (sbyte)depth;
            }
            else if (_useOptimizedTranspositionTable2)
            {
                iTransposition = (board.ZobristKey, (short)bestEvaluation, (sbyte)depth);
            }
            else
            {
                _transpositionTable[board.ZobristKey] = ((byte)depth, (short)bestEvaluation);
            }
        }

        if (quiescenceSearch && bestMove == Move.NullMove)
        {
            return (new List<Move>(), bestEvaluation != int.MinValue ? bestEvaluation : currentEvaluation);
        }
        else
        {
            bestMoveLine.Insert(0, bestMove);
            return (bestMoveLine, bestEvaluation);
        }
    }


    private int EvaluateBoard(Board board)
    {
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -10001 : 10001;

        PieceList[] pieces = board.GetAllPieceLists();

        int evaluation = 0;

        foreach (PieceList pieceList in pieces)
            foreach (Piece piece in pieceList)
                evaluation += (pieceList.IsWhitePieceList ? 1 : -1) * ((_useImprovedPieceValues ? GetTaperedEvaluation(_iPieceValueE[(int)piece.PieceType], _iPieceValueL[(int)piece.PieceType]) : _pieceValues[(int)piece.PieceType]) + GetPiecePositionEvaluation(piece.PieceType, piece.Square, pieceList.IsWhitePieceList));

        return board.IsWhiteToMove ? evaluation : -evaluation;
    }

    /// <summary>
    /// Function to get the values of a move. Use this function for optimization instead of EvaluateBoard()
    /// </summary>
    /// <param name="newBoard">The board where the move is already made on</param>
    /// <param name="move">The move that needs to be evaluated</param>
    /// <returns></returns>
    private int EvaluateMove(Move move, bool isWhiteMove)
    {
        return (_useImprovedPieceValues ? GetTaperedEvaluation(_iPieceValueE[(int)move.CapturePieceType], _iPieceValueL[(int)move.CapturePieceType]) : _pieceValues[(int)move.CapturePieceType]) + (_useImprovedPieceValues ? GetTaperedEvaluation(_iPieceValueE[(int)move.PromotionPieceType], _iPieceValueL[(int)move.PromotionPieceType]) : _pieceValues[(int)move.PromotionPieceType]) + GetPiecePositionEvaluation(move.MovePieceType, move.TargetSquare, isWhiteMove) - GetPiecePositionEvaluation(move.MovePieceType, move.StartSquare, isWhiteMove);
    }

    private int GetPiecePositionEvaluation(PieceType pieceType, Square square, bool isWhite)
    {
        int index = isWhite ? square.Index ^56 : square.Index;

        if (_useLiteBlue)
        {
            return ((UnpackedPestoTables[index][(int)pieceType - 1] * (256 - _gamePhase)) + (UnpackedPestoTables[index][(int)pieceType + 6 - 1] * _gamePhase)) / 256;
        }
        else if (_useImprovedPieceValues)
        {
            switch (pieceType)
            {
                case PieceType.Pawn:
                    return GetTaperedEvaluation(_iPawnPositionValueE[index], _iPawnPositionValueL[index]);
                case PieceType.Knight:
                    return GetTaperedEvaluation(_iKnightPositionValueE[index], _iKnightPositionValueL[index]);
                case PieceType.Bishop:
                    return GetTaperedEvaluation(_iBishopPositionValueE[index], _iBishopPositionValueL[index]);
                case PieceType.Rook:
                    return GetTaperedEvaluation(_iRookPositionValueE[index], _iRookPositionValueL[index]);
                case PieceType.Queen:
                    return GetTaperedEvaluation(_iQueenPositionValueE[index], _iQueenPositionValueL[index]);
                case PieceType.King:
                    return GetTaperedEvaluation(_iKingPositionValueE[index], _iKingPositionValueL[index]);
                default:
                    return 0;
            }
        }
        else
        {
            switch (pieceType)
            {
                case PieceType.Pawn:
                    return _pawnPositionValues[index];
                case PieceType.Knight:
                    return _knightPositionValues[index];
                case PieceType.Bishop:
                    return _bishopPositionValues[index];
                case PieceType.Rook:
                    return _rookPositionValues[index];
                case PieceType.Queen:
                    return _queenPositionValues[index];
                case PieceType.King:
                    return _isEndGame ? _kingPositionValuesLate[index] : _kingPositionValuesEarly[index];
                default:
                    return 0;
            }
        }
    }

    private bool IsEndGame(Board board)
    {
        if (board.PlyCount > 30) 
        {
            bool whiteQueen = board.GetPieceList(PieceType.Queen, true).Count() != 0;
            bool blackQueen = board.GetPieceList(PieceType.Queen, false).Count() != 0;
            
            if (!whiteQueen && !blackQueen)
                return true;

            int extraPiecesWhite = board.GetPieceList(PieceType.Rook, true).Count() + board.GetPieceList(PieceType.Bishop, true).Count() + board.GetPieceList(PieceType.Knight, true).Count();
            int extraPiecesBlack = board.GetPieceList(PieceType.Rook, false).Count() + board.GetPieceList(PieceType.Bishop, false).Count() + board.GetPieceList(PieceType.Knight, false).Count();

            bool endGameWhite = !whiteQueen || (whiteQueen && extraPiecesWhite < 3);
            bool endGameBlack = !blackQueen || (blackQueen && extraPiecesBlack < 3);

            return endGameWhite && endGameBlack;
        }
            
        return false;
    }

    private int GetTaperedEvaluation(int eEvaluation, int lEvaluation)
    {
        return ((eEvaluation * (256 - _gamePhase)) + (lEvaluation * _gamePhase)) / 256;
    }

    private int GetGamePhase(Board board)
    {
        int totalPhase = 24;

        int phase = totalPhase;

        phase -= board.GetPieceList(PieceType.Knight, true).Count() * 1;
        phase -= board.GetPieceList(PieceType.Bishop, true).Count() * 1;
        phase -= board.GetPieceList(PieceType.Rook, true).Count() * 2;
        phase -= board.GetPieceList(PieceType.Queen, true).Count() * 4;

        phase -= board.GetPieceList(PieceType.Knight, false).Count() * 1;
        phase -= board.GetPieceList(PieceType.Bishop, false).Count() * 1;
        phase -= board.GetPieceList(PieceType.Rook, false).Count() * 2;
        phase -= board.GetPieceList(PieceType.Queen, false).Count() * 4;

        return (phase * 256 + (totalPhase / 2)) / totalPhase;
    }

    /// <summary>
    /// Do -128 in evaluation
    /// </summary>
    private decimal[] CompressMap(int[] map)
    {
        byte[] array = new byte[32];
        decimal[] result = new decimal[6];

        for (int i = 0; i < map.Length; i++)
            array[i] = (byte)Math.Max(0, Math.Min(256, map[i] + 128));


        BigInteger b = new BigInteger(array);
        //decimal[] d = BigIntegerToDecimalArray(string.Concat(array.Select(b => $"{b:X2}")));

        //for (int i = 0, currentValue = 0; i < 32; i++)
        //{
        //    int mod = 4 - (currentValue % 4);
        //    Console.WriteLine($"{i}: {currentValue} and {currentValue + 4 - mod}");
        //    currentValue += (i % 4 == 3) ? 5 : 1;
        //}
        //
        //
        //for (int i = 0; i < 32; i++)
        //{
        //    array[i] = (byte)Math.Max(0, Math.Min(256, map[i] + 128));
        //    array2[i + (i % 4)] = (byte)Math.Max(0, Math.Min(256, map[i] + 128));
        //}

        string hexString = string.Concat(array.Select(b => $"{b:X2}"));

        ulong[] combinedUlong = new ulong[8];
        
        //for (int i = 63, u = 8; i >= 0; i--)
        //{
        //    if ((i + 1) % 8 == 0)
        //        u--;
        //
        //    combinedUlong[u] = (combinedUlong[u] << 8) | (byte)map[i];
        //}

        //12 Tables
        //ULONG 64 squares: 8 * 12 * 2 = 192 tokens
        //DECIMAL 32 squares: 32 * 2 = 64 tokens
        //ULONG 32 squares: 48 * 2 = 96 tokens



        //string endString = "";
        //string totalString = string.Concat(map);
        //
        //for (int j = 0; j < 384; j += 4)
        //    endString += totalString.Substring(j, 4) + new string(totalString.Substring(j, 4).Reverse().ToArray());


        byte[] byteArrayRev = BitConverter.GetBytes(map[0]);

        //Concatenate the bytes of the other ulongs to the byteArrayRev
        for (int i = 1; i < map.Length; i++)
            byteArrayRev = byteArrayRev.Concat(BitConverter.GetBytes(map[i])).ToArray();


        BigInteger b3 = new BigInteger(combinedUlong[0]);



        return result;
        //return new decimal((int)(combinedUlong[0] & 0xFFFFFFFF), (int)(combinedUlong[0] >> 32), 0, false, 0);
    }

    //private decimal[] BigIntegerToDecimalArray(string strin)
    //{
    //    decimal[] decimals = new decimal[6];
    //    decimals[0] = strin.ToString().Substring(0, 32);
    //}

    //private int[] DecodeDecimals(decimal[] data)
    //{
    //    string hexString = string.Concat(array.Select(b => $"{b:X2}"));
    //}
}