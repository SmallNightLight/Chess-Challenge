using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class MyBot : IChessBot
{
    //Temp variables
    private Board _board;

    private Move _mainMove;
    private Move _rootMove;

    //Other Variables
    private Timer _timer;
    private float _timeThisTurn;

    private (ulong, short, sbyte)[] _transpositionTable = new (ulong, short, sbyte)[0x800000];

    //Value of pieces (early game -> end game)
    private readonly short[] _pieceValues = { 82, 337, 365, 477, 1025, 20000, 94, 281, 297, 512, 936, 20000};

    private int[] _pieceWeight = { 0, 1, 1, 2, 4, 0 };

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

    public MyBot()
    {
        UnpackedPestoTables = PackedPestoTables.Select(packedTable =>
        {
            int pieceType = 0;
            return decimal.GetBits(packedTable).Take(3).SelectMany(c => BitConverter.GetBytes(c).Select(square => (int)((sbyte)square * 1.461) + _pieceValues[pieceType++])).ToArray();
        }).ToArray();
    }

    public Move Think(Board board, Timer timer)
    {
        _board = board;
        _timer = timer;

        _timeThisTurn = Math.Min(timer.MillisecondsRemaining / 25, (0.6f + (0.04f * Math.Min(board.PlyCount, 15))) * timer.GameStartTimeMilliseconds / 80f);

        for (int depth = 1; depth <= 9; depth++)
        {
             int evaluation = Search(0, depth, -10000, 10000, _rootMove, false, new HashSet<Square>(), 3);

            if (evaluation > 100000)
            {
                _rootMove = _mainMove;
                break;
            }

            if (timer.MillisecondsElapsedThisTurn < _timeThisTurn)
                _rootMove = _mainMove;
            else
                break;
        }

        //Console.WriteLine("BotB1C finished at depth: " + searchDepth + " in: " + timer.MillisecondsElapsedThisTurn + " milliseconds, time left: " + timer.MillisecondsRemaining);

        return _rootMove;
    }

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 9999 };

    private int Search(int ply, int depth, int alpha, int beta, Move pvMove, bool quiescenceSearch, HashSet<Square> capturePieces, int quiescenceDepth)
    {
        int currentEvaluation = Eval();

        //Check for depth and time
        if (_timer.MillisecondsElapsedThisTurn > _timeThisTurn || (!quiescenceSearch && depth == 0) || (quiescenceSearch && (quiescenceDepth == 0 || capturePieces.Count() == 0)))
            return currentEvaluation;

        if (ply != 0 && _board.IsRepeatedPosition()) 
            return -100;

        ref var transposition = ref _transpositionTable[_board.ZobristKey & 0x7FFFFF];

        List<Move> interestingMoves = new List<Move>();
        bool inCheck = _board.IsInCheck();

        if (pvMove != Move.NullMove)
            interestingMoves.Add(pvMove);

        if (transposition.Item1 == _board.ZobristKey && transposition.Item3 >= depth && ((currentEvaluation >= beta && transposition.Item2 >= beta) || (currentEvaluation <= alpha && transposition.Item2 <= alpha)))
            return transposition.Item2; //Alpha or Beta cut-off

        Move bestMove = Move.NullMove;
        int bestEvaluation = int.MinValue;

        var moves = quiescenceSearch ? _board.GetLegalMoves(true).Where(move => capturePieces.Contains(move.TargetSquare)).ToArray() : _board.GetLegalMoves();

        //moves = moves.OrderByDescending(move => interestingMoves.Contains(move)).ThenBy(move => interestingMoves.IndexOf(move)).ThenByDescending(move => 0).ToArray(); //EvaluateMove(board)
        moves = moves.OrderByDescending(move => interestingMoves.Contains(move)).ThenBy(move => interestingMoves.IndexOf(move)).ThenByDescending(move => pieceValues[(int)move.CapturePieceType]).ToArray(); //EvaluateMove(board)

        foreach (Move move in moves)
        {
            _board.MakeMove(move);

            //Update the capture Pieces list
            if (capturePieces.Contains(move.StartSquare))
                capturePieces.Remove(move.StartSquare);

            HashSet<Square> updatedCapturePieces = new HashSet<Square>(capturePieces);

            if (move.IsCapture)
                updatedCapturePieces.Add(move.TargetSquare);

            //_board.IsDraw() ? -100 : 
            int evaluation = -Search(ply + 1, quiescenceSearch ? depth : depth - 1, -beta, -alpha, Move.NullMove, depth == 1 || quiescenceSearch, updatedCapturePieces, quiescenceDepth - (quiescenceSearch ? 1 : 0));

            _board.UndoMove(move);

            //Set best move
            if (evaluation > bestEvaluation)
            {
                bestMove = move;
                bestEvaluation = evaluation;

                if (ply == 0)
                    _mainMove = move;
            }

            //into above?
            //Set alpha
            alpha = Math.Max(alpha, bestEvaluation);

            //Check if can cut-off
            if (beta <= alpha)
                break;
        }

        if (!quiescenceSearch && moves.Length == 0) return inCheck ? ply - 100000 : 0;

        if (!quiescenceSearch)
            transposition = (_board.ZobristKey, (short)bestEvaluation, (sbyte)depth);

        if (quiescenceSearch && bestMove == Move.NullMove)
            return bestEvaluation != int.MinValue ? bestEvaluation : currentEvaluation;
        
        return bestEvaluation;
    }

    private int Eval()
    {
        int middlegame = 0, endgame = 0, gamephase = 0, sideToMove = 2;
        for (; --sideToMove >= 0;)
        {
            for (int piece = -1, square; ++piece < 6;)
                for (ulong mask = _board.GetPieceBitboard((PieceType)piece + 1, sideToMove > 0); mask != 0;)
                {
                    // Gamephase, middlegame -> endgame
                    gamephase += _pieceWeight[piece];

                    // Material and square evaluation
                    square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ 56 * sideToMove;
                    middlegame += UnpackedPestoTables[square][piece];
                    endgame += UnpackedPestoTables[square][piece + 6];
                }
            middlegame = -middlegame;
            endgame = -endgame;
        }
        return (middlegame * gamephase + endgame * (24 - gamephase)) / 24 * (_board.IsWhiteToMove ? 1 : -1);
    }
}