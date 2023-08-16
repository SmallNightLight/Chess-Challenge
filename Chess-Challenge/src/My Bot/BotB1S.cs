using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class BotB1S : IChessBot
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

    private int[] _pieceValues = { 0, 100, 300, 325, 500, 900, 10000 };

    //Other Variables
    private Timer _timer;
    private float _timeThisTurn;

    private bool _isEndGame;
    private Dictionary<ulong, (byte, short)> _transpositionTable = new Dictionary<ulong, (byte, short)>();

    public Move Think(Board board, Timer timer)
    {
        _timer = timer;
        Move bestMove = Move.NullMove;

        //Check if reached endgame
        _isEndGame |= board.PlyCount > 30 && (!(board.GetPieceList(PieceType.Queen, true).Any() || board.GetPieceList(PieceType.Queen, false).Any()) || (board.GetPieceList(PieceType.Rook, true).Count() + board.GetPieceList(PieceType.Bishop, true).Count() + board.GetPieceList(PieceType.Knight, true).Count() < 3 && board.GetPieceList(PieceType.Rook, false).Count() + board.GetPieceList(PieceType.Bishop, false).Count() + board.GetPieceList(PieceType.Knight, false).Count() < 3));

        //Calculate time
        float openingsMoves = Math.Min(board.PlyCount, 15);
        float totalTime = timer.GameStartTimeMilliseconds;

        float factor = 0.3f + (0.1f * openingsMoves);
        float timePerMove = totalTime / 60f;
        _timeThisTurn = Math.Min(timer.MillisecondsRemaining / 21, factor * timePerMove);

        //Iterative Deepening
        for (int depth = 1; depth <= 9; depth++)
        {
            Move move = Search(board, depth, EvaluateBoard(board), -10000, 10000, bestMove, false, new HashSet<Square>(), 3).Item1;

            if (timer.MillisecondsElapsedThisTurn < _timeThisTurn)
                bestMove = move;
            else
                break;
        }

        return bestMove;
    }

    private (Move, int) Search(Board board, int depth, int currentEvaluation, int alpha, int beta, Move pvMove, bool quiescenceSearch, HashSet<Square> capturePieces, int quiescenceDepth)
    {
        //Check for depth and time
        if ((_timer.MillisecondsElapsedThisTurn > _timeThisTurn) || (!quiescenceSearch && depth == 0) || (quiescenceSearch && (quiescenceDepth == 0 || capturePieces.Count() == 0)))
            return (Move.NullMove, currentEvaluation);

        //Search in the transposition tables for existing entries
        if (_transpositionTable.TryGetValue(board.ZobristKey, out var storedEntry) && storedEntry.Item1 >= depth)
        {
            if (currentEvaluation >= beta && storedEntry.Item2 >= beta)
                return (Move.NullMove, storedEntry.Item2); //Beta cut-off
            if (currentEvaluation <= alpha && storedEntry.Item2 <= alpha)
                return (Move.NullMove, storedEntry.Item2); //Alpha cut-off
        }

        Move bestMove = Move.NullMove;
        int bestEvaluation = int.MinValue;

        //Get moves
        var moves = quiescenceSearch ? board.GetLegalMoves().Where(move => capturePieces.Contains(move.TargetSquare)).ToArray() : board.GetLegalMoves();

        //Move ordering
        moves = moves.OrderByDescending(move => move.Equals(pvMove) ? 1 : 0).ThenByDescending(move => EvaluateMove(move, board.IsWhiteToMove)).ToArray();

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

            //Update the capture Pieces list
            if (capturePieces.Contains(move.StartSquare))
                capturePieces.Remove(move.StartSquare);

            HashSet<Square> updatedCapturePieces = new HashSet<Square>(capturePieces);

            if (move.IsCapture)
                updatedCapturePieces.Add(move.TargetSquare);

            //Ckeck for a draw and if false do a new search
            evaluation = board.IsDraw() ? -100 : -Search(board, quiescenceSearch ? depth : depth - 1, -currentEvaluation - EvaluateMove(move, !board.IsWhiteToMove), -beta, -alpha, Move.NullMove, depth == 1 || quiescenceSearch, updatedCapturePieces, quiescenceDepth - (quiescenceSearch ? 1 : 0)).Item2;

            board.UndoMove(move);

            //Set best move
            if (evaluation > bestEvaluation)
            {
                bestMove = move;
                bestEvaluation = evaluation;
            }

            alpha = Math.Max(alpha, bestEvaluation);

            //Check if can cut-off
            if (beta <= alpha)
                break;
        }

        if (!quiescenceSearch)
            _transpositionTable[board.ZobristKey] = ((byte)depth, (short)bestEvaluation);

        if (quiescenceSearch && bestMove == Move.NullMove)
            return (Move.NullMove, bestEvaluation != int.MinValue ? bestEvaluation : currentEvaluation);

        return (bestMove, bestEvaluation);
    }


    private int EvaluateBoard(Board board)
    {
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -10001 : 10001;

        PieceList[] pieces = board.GetAllPieceLists();

        int evaluation = 0;

        foreach (PieceList pieceList in pieces)
            foreach (Piece piece in pieceList)
                evaluation += (pieceList.IsWhitePieceList ? 1 : -1) * (_pieceValues[(int)piece.PieceType] + GetPiecePositionEvaluation(piece.PieceType, piece.Square, pieceList.IsWhitePieceList));

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
        return _pieceValues[(int)move.CapturePieceType] + _pieceValues[(int)move.PromotionPieceType] + GetPiecePositionEvaluation(move.MovePieceType, move.TargetSquare, isWhiteMove) - GetPiecePositionEvaluation(move.MovePieceType, move.StartSquare, isWhiteMove);
    }

    private int GetPiecePositionEvaluation(PieceType pieceType, Square square, bool isWhite)
    {
        int index = isWhite ? 63 - square.Index : square.Index;

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