using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class Bot3
{
    public Move Think(Board board, Timer timer)
    {
        MoveEvaluation bestEvaluation = Search(board, board.IsWhiteToMove, _depth, -10000, 10000);
        _transpositionTable.Clear();
        return bestEvaluation.Move;
    }

    int _depth = 5;
    int[] _pieceValues = { 0, 100, 300, 300, 500, 900, 9999 };

    private Dictionary<ulong, TranspositionEntry> _transpositionTable = new Dictionary<ulong, TranspositionEntry>();

    struct TranspositionEntry
    {
        public int Depth;
        public int Value;
    }

    int[,] pawnMap = new int[,]
    {
        { 90, 90, 90, 90, 90, 90, 90, 90 },
        { 50, 50, 50, 50, 50, 50, 50, 50 },
        { 10, 10, 20, 30, 30, 20, 10, 10 },
        { 0, 0, 10, 20, 25, 10, 5, 5 },
        { 0, 0, 0, 20, 20, 0, 0, 0 },
        { 5, -5, -10, 0, 0, -10, -5, 5 },
        { 5, 10, 10, -20, -20, 10, 10, 5 },
        { 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    private MoveEvaluation Search(Board board, bool max, int depth, int alpha, int beta)
    {
        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0 || depth == 0)
        {
            //If there are no moves or we've reached the maximum depth, return the evaluation of the current position.
            return new MoveEvaluation(Move.NullMove, GetRawPositionEvaluation(board));
        }

        if (_transpositionTable.TryGetValue(board.ZobristKey, out TranspositionEntry entry) && entry.Depth >= depth)
        {
            //Use the stored evaluation if the entry's depth is equal to or greater than the current depth.
            if (entry.Value <= alpha) return new MoveEvaluation(Move.NullMove, alpha);
            if (entry.Value >= beta) return new MoveEvaluation(Move.NullMove, beta);
        }

        Move bestMove = moves[0];
        int bestMoveEvaluation = max ? int.MinValue : int.MaxValue;

        moves = moves.OrderByDescending(move => _pieceValues[(int)move.CapturePieceType]).ToArray();

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                _transpositionTable[board.ZobristKey] = new TranspositionEntry { Depth = depth, Value = bestMoveEvaluation };
                return new MoveEvaluation(move, max ? 10001 : -10001);
            }

            MoveEvaluation result;

            if (board.IsDraw())
            {
                int evaluation = GetRawPositionEvaluation(board);
                if (evaluation < 100 && evaluation > -100)
                    evaluation = -99;
                else
                    evaluation *= -1;

                result = new MoveEvaluation(Move.NullMove, evaluation);
            }
            else
                result = Search(board, !max, depth - 1, alpha, beta);

            board.UndoMove(move);

            int eval = result.Value;
            if ((max && eval > bestMoveEvaluation) || (!max && eval < bestMoveEvaluation))
            {
                bestMoveEvaluation = eval;
                bestMove = move;
            }

            if (max)
                alpha = Math.Max(alpha, bestMoveEvaluation);
            else
                beta = Math.Min(beta, bestMoveEvaluation);

            if (beta <= alpha)
            {
                _transpositionTable[board.ZobristKey] = new TranspositionEntry { Depth = depth, Value = bestMoveEvaluation };
                return new MoveEvaluation(bestMove, bestMoveEvaluation);
            }
        }

        _transpositionTable[board.ZobristKey] = new TranspositionEntry { Depth = depth, Value = bestMoveEvaluation };
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
            return board.IsWhiteToMove ? -10001 : 10001;

        PieceList[] pieces = board.GetAllPieceLists();

        int evaluation = 0;

        foreach (PieceList pieceList in pieces)
        {
            foreach (Piece piece in pieceList)
            {
                int value = _pieceValues[(int)piece.PieceType] + GetPositionEvaluation(piece);
                evaluation += pieceList.IsWhitePieceList ? value : -value;
            }
        }

        return evaluation;
    }

    private int GetPositionEvaluation(Piece piece)
    {
        int rank = piece.IsWhite ? 7 - piece.Square.Rank : piece.Square.Rank;
        int file = piece.Square.File;

        switch (piece.PieceType)
        {
            case PieceType.Pawn:
                return pawnMap[rank, file];
            default:
                return 0;
        }
    }
}