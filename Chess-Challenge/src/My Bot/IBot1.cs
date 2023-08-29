using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class IBot1 : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 9999 };

    public Move Think(Board board, Timer timer)
    {
        return Search(board, board.IsWhiteToMove, 3).Move;
    }

    private MoveEvaluation Search(Board board, bool max, int depth)
    {
        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0 || depth == 0)
        {
            // If there are no moves or we've reached the maximum depth, return the evaluation of the current position.
            return new MoveEvaluation(Move.NullMove, GetRawPositionEvaluation(board));
        }

        int bestMoveEvaluation = max ? int.MinValue : int.MaxValue;
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            MoveEvaluation result = Search(board, !max, depth - 1);
            board.UndoMove(move);

            int eval = result.Value;
            if ((max && eval > bestMoveEvaluation) || (!max && eval < bestMoveEvaluation))
            {
                bestMoveEvaluation = eval;
                bestMove = move;
            }
        }

        return new MoveEvaluation(bestMove, bestMoveEvaluation);
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

    private int GetRawPositionEvaluation(Board board)
    {
        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? -10000 : 10000;
        }

        PieceList[] pieces = board.GetAllPieceLists();

        int evaluation = 0;


        Move[] moves = board.GetLegalMoves();

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            evaluation += board.GetLegalMoves().Count();
            board.UndoMove(move);
        }

        foreach (PieceList pieceList in pieces)
        {
            int value = pieceValues[(int)pieceList.TypeOfPieceInList] * pieceList.Count;
            evaluation += pieceList.IsWhitePieceList ? value : -value;
        }

        return evaluation;
    }
}