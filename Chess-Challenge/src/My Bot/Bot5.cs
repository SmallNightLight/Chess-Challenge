﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class Bot5
{
    public Move Think(Board board, Timer timer)
    {
        if (!_started)
            SetupMaps();

        MoveEvaluation bestEvaluation = Search(board, board.IsWhiteToMove, 0, _minimumDepth, -10000, 10000);
        _transpositionTable.Clear();
        return bestEvaluation.Move;
    }

    bool _started;

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
        //Pawn Map
        55555663654555576667777899990m,

        //Knight Map
        12222356256625682568256723551m,

        //Bishop map
        34444755466645664555455645553m,

        //Rook Map
        55564555455545554555455566665m,

        //Queen Map
        34454555466655664566456645553m,

        //King Map
        78657755433334444445444544454m,
    };

    int[][] _maps = new int[10][];

    //The first digit is only a placeholder, but important!
    //Total of 10 Maps possible
    decimal _endPiecesMap = 1000222444555445445m;

    private void SetupMaps()
    {
        _started = true;

        for (int i = 0; i < _rawMaps.Count(); i++)
        {
            int[] map = new int[64];
            string mainString = _rawMaps[i] + _endPiecesMap.ToString().Substring(i * 3 + 1, 3);

            for (int j = 0, mapIndex = 0; j < 32; j += 4)
            {
                string subString = mainString.Substring(j, 4);
                foreach (char c in subString + new string(subString.Reverse().ToArray()))
                    map[mapIndex++] = _numberValue[int.Parse(c.ToString())];
            }
            _maps[i] = map;
        }
    }

    public int[][] GetMaps()
    {
        SetupMaps();
        return _maps;
    }


    private MoveEvaluation Search(Board board, bool max, int currentDepth, int currentMaxDepth, int alpha, int beta)
    {
        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0 || currentDepth == currentMaxDepth)
        {
            //If there are no moves or we've reached the maximum depth, return the evaluation of the current position.
            return new MoveEvaluation(Move.NullMove, GetRawPositionEvaluation(board));
        }

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
                int value = pieceValues[(int)piece.PieceType] + GetPositionEvaluation(piece);
                evaluation += pieceList.IsWhitePieceList ? value : -value;
            }
        }

        return evaluation;
    }

    private int GetPositionEvaluation(Piece piece)
    {
        int index = piece.IsWhite ? piece.Square.Index : 63 - piece.Square.Index;

        switch (piece.PieceType)
        {
            case PieceType.Pawn:
                return _maps[0][index];
            case PieceType.Knight:
                return _maps[1][index];
            case PieceType.Bishop:
                return _maps[2][index];
            case PieceType.Rook:
                return _maps[3][index];
            default:
                return 0;
        }
    }
}