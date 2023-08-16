using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class Bot6
{
    public Move Think(Board board, Timer timer)
    {
        float progression = (Math.Max(5, board.GetAllPieceLists().Sum(pieceList => pieceList.Count)) - 5) / 27f;

        //Setup Maps
        for (int i = 0, mapIndex = 1; i < 7; i++)
        {
            string mainString = GetRawString(i);
            string secondString = (i == 0 || i == 6) ? GetRawString(i++ + 1) : GetRawString(i);

            _maps[mapIndex++] = Enumerable.Range(0, 32).Where(index => index % 4 == 0).SelectMany(a => Enumerable.Range(0, 8).Select(b => (int)(progression * GetNumberValue(mainString, a, b) + (1 - progression) * GetNumberValue(secondString, a, b)))).ToArray();
        }

        MoveEvaluation bestEvaluation = Search(board, board.IsWhiteToMove, 0, _minimumDepth, -10000, 10000);

        _transpositionTable.Clear();
        return bestEvaluation.Move;
    }

    int _minimumDepth = 5;
    int _maximumDepth = 5; //uneven
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 9999 };

    private Dictionary<ulong, TranspositionEntry> _transpositionTable = new Dictionary<ulong, TranspositionEntry>();

    struct TranspositionEntry
    {
        public int Depth;
        public int Value;
    }

    int[] _numberValue = new int[] { 900, -50, -35, -20, -10, 0, 10, 20, 35, 50 };

    decimal[] _rawMaps = new decimal[]
    {
        //Pawn Map - Start
        55555663654555576667777899990m,

        //Pawn Map - End
        55556666666677778888888899990m,

        //Knight Map
        12222356256625672567256623552m,

        //Bishop map
        34444655466645664555455645552m,

        //Rook Map
        55564555455545554555455566665m,

        //Queen Map
        34454555466655664566456645552m,

        //King Map - Start
        78657755433334444445444544454m,

        //King Map - End
        12222255247724882488247723452m
    };

    int[][] _maps = new int[10][];

    private string GetRawString(int index)
    {
        return _rawMaps[index] + new string(_rawMaps[index].ToString()[28], 3);
    }

    private int GetNumberValue(string mainString, int stringIndex, int digit)
    {
        return _numberValue[int.Parse((mainString.Substring(stringIndex, 4) + new string(mainString.Substring(stringIndex, 4).Reverse().ToArray()))[digit].ToString())];
    }

    private MoveEvaluation Search(Board board, bool max, int currentDepth, int currentMaxDepth, int alpha, int beta)
    {
        Move[] moves = board.GetLegalMoves();

        //If there are no moves or we've reached the maximum depth, return the evaluation of the current position.
        if (moves.Length == 0 || currentDepth == currentMaxDepth)
            return new MoveEvaluation(Move.NullMove, GetRawPositionEvaluation(board));

        if (_transpositionTable.TryGetValue(board.ZobristKey, out TranspositionEntry entry) && entry.Depth < currentDepth)
        {
            //Use the stored evaluation if the entry's depth is equal to or greater than the current depth.
            if (entry.Value <= alpha)
                return new MoveEvaluation(Move.NullMove, alpha);

            if (entry.Value >= beta)
                return new MoveEvaluation(Move.NullMove, beta);
        }

        Move bestMove = moves[0];
        int bestMoveEvaluation = max ? int.MinValue : int.MaxValue;

        moves = moves.OrderByDescending(move => pieceValues[(int)move.CapturePieceType]).ToArray();

        foreach (Move move in moves)
        {
            if (currentDepth + 1 == currentMaxDepth && currentMaxDepth < _maximumDepth && board.IsInCheck())
                currentMaxDepth++;

            board.MakeMove(move);

            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                _transpositionTable[board.ZobristKey] = new TranspositionEntry { Depth = currentDepth, Value = bestMoveEvaluation };
                return new MoveEvaluation(move, max ? 10001 : -10001);
            }

            if (currentDepth + 1 == currentMaxDepth && currentMaxDepth < _maximumDepth && (board.IsInCheck() || move.IsCapture || move.IsPromotion))
                currentMaxDepth++;

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
                result = Search(board, !max, currentDepth + 1, currentMaxDepth, alpha, beta);

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
                _transpositionTable[board.ZobristKey] = new TranspositionEntry { Depth = currentDepth, Value = bestMoveEvaluation };
                return new MoveEvaluation(bestMove, bestMoveEvaluation);
            }
        }

        _transpositionTable[board.ZobristKey] = new TranspositionEntry { Depth = currentDepth, Value = bestMoveEvaluation };
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
                int value = pieceValues[(int)piece.PieceType] + _maps[(int)piece.PieceType][piece.IsWhite ? piece.Square.Index : 63 - piece.Square.Index];
                evaluation += pieceList.IsWhitePieceList ? value : -value;
            }
        }

        return evaluation;
    }
}