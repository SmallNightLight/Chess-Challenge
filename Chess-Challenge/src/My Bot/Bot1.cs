using System;
using System.Collections.Generic;
using ChessChallenge.API;
using System.Linq;

public class Bot1 : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        //Easy Bot
        return EasyBot(board, timer);
    }

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    int[] countRecursion = { 50, 45, 45, 40, 20, 6, 5, 5, 4,4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3};

    private Move EasyBot(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0)
        {
            return Move.NullMove;
        }

        List<MoveEvaluation> moveEvaluation = new List<MoveEvaluation>();

        foreach (Move move in moves)
        {
            //Google en passant
            if (move.IsEnPassant)
                return move;

            int count = GetTotalPieces(board);
            int recursions = countRecursion[count - 1];
            int value = GetMoveValue(board, move, 0, recursions, true);

            MoveEvaluation evaluation = new MoveEvaluation(move, value);
            moveEvaluation.Add(evaluation);
        }

        moveEvaluation = moveEvaluation.OrderByDescending(x => x.Value).ToList();

        return moveEvaluation[0].Move;
    }

    struct MoveEvaluation
    {
        public Move Move;
        public int Value;

        public MoveEvaluation(Move move, int value)
        {
            Move = move;
            Value = value;
        }
    }

    private Move GetBestMove(Board board, int recursionLevel)
    {
        Move[] moves = board.GetLegalMoves();
        if(moves.Length == 0)
        {
            return Move.NullMove;
        }
        Move bestMove = moves[0];
        int bestMoveValue = 0;

        foreach (Move move in moves)
        {
            //Google en passant
            if (move.IsEnPassant)
                return move;

            int value = GetMoveValue(board, move, 0, recursionLevel, true);
            if (value > bestMoveValue)
            {
                bestMoveValue = value;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int GetMoveValue(Board board, Move move, int currentValue, int recursionLevel, bool isMyTurn)
    {
        //WHAT IF NO moves?
        int moveValue = 10000;

        int newValue = GetCurrentMoveValue(board, move);

        board.MakeMove(move);
        
        int currentMoveValue = currentValue + (isMyTurn ? newValue : -newValue);

        if (recursionLevel > 0)
        {
            //Check others turn
            Move otherBestMove = GetBestMove(board, recursionLevel - 1);
            if (otherBestMove != Move.NullMove)
                currentMoveValue = GetMoveValue(board, otherBestMove, currentMoveValue, recursionLevel - 1, !isMyTurn);
        }

        if (currentMoveValue < moveValue)
            moveValue = currentMoveValue;

        board.UndoMove(move);

        return moveValue;
    }

    private int GetCurrentMoveValue(Board board, Move move)
    {
        Piece capturedPiece = board.GetPiece(move.TargetSquare);
        int pieceEval = pieceValues[(int)capturedPiece.PieceType];

        board.MakeMove(move);

        if (board.IsInCheckmate())
        {
            board.UndoMove(move);
            return 100000;
        }

        board.UndoMove(move);
        return pieceEval;
    }

    private int GetRawPositionEvaluation(Board board, bool forWhite)
    {
        if (board.IsInCheckmate())
        {
            return 10000;
        }

        PieceList[] pieces = board.GetAllPieceLists();

        int evaluation = 0;

        foreach (PieceList pieceList in pieces)
        {
            int value = pieceValues[(int)pieceList.TypeOfPieceInList] * pieceList.Count;
            evaluation += pieceList.IsWhitePieceList ^ forWhite ?  value : -value;
        }

        return evaluation;
    }

    private int GetTotalPieces(Board board)
    {
        PieceList[] pieces = board.GetAllPieceLists();

        int count = 0;

        foreach (PieceList pieceList in pieces)
            count += pieceList.Count;

        return count;
    }
}