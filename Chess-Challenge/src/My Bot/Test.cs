/**
using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess_Challenge.src.My_Bot
{
    internal class Test
    {
        int razor_margin = 100;
        int extd_futil_margin = 300;
        int futil_margin = 800;

        int search(Board board, int alpha, int beta, int depth)
        {
            int extend, fmax, fscore, tt_hit;

            //declare the local variables that require constant initialization
            bool prune = false;
            int fpruned_moves = 0;
            int score = int.MinValue;

            //execute the opponent's move and determine how to extend the search
            //Loop through all moves

            Move move = Move.NullMove; //Legal move
            board.MakeMove(move);

            int evaluation = Evaluate(board);
            bool inCheck = board.IsInCheck();
            extend = Extend(board);
            depth += extend;

            //decide about limited razoring at the pre-pre-frontier nodes
            fscore = evaluation + razor_margin;
            if (extend == 0&& (depth == 3) && (fscore <= alpha))
            { 
                prune = true; 
                score = fmax = fscore; 
            }

            //decide about extended futility pruning at pre-frontier nodes
            fscore = evaluation + extd_futil_margin;

            if (extend == 0&& (depth == 2) && (fscore <= alpha))
            { 
                prune = true; 
                score = fmax = fscore; 
            }

            //decide about selective futility pruning at frontier nodes
            fscore = evaluation + futil_margin;

            if (!inCheck && (depth == 1) && (fscore <= alpha))
            { 
                prune = true; 
                score = fmax = fscore; 
            }


            //Transpositions
            //tt_hit = probe_transposition_tables(current, depth, &tt_ref);
            //if (tt_hit) {} else {}


            //try the adaptive null-move search with a minimal window around
            //"beta" only if it is allowed, desired, and really promises to cut off
            bool canDoNull = true;
            if (!prune && !inCheck && canDoNull && board.TrySkipTurn())
            {
                int null_score = -search(board, -beta, -beta + 1, depth - R_adpt(current, depth) - 1);

                if (null_score >= beta) 
                    return null_score;
                
                //fail-high null-move cutoff
                //...
            }

            //now continue as usual but prune all futile moves if "fprune == 1"
            foreach(Move newMove in board.GetLegalMoves())
            {
                if (!prune || check(move) || (fmax + mat_gain(move) > alpha))
                {
                    
                }
                else 
                    fpruned_moves++;   
            }

            //"fpruned_moves > 0" => the search was selective at the current node
            //

            return score;
        }

        int Extend(Board board)
        {
            //Calculate depth extension
            return 0;
        }

        int Evaluate(Board board)
        {
            //return Evaluation
            return 0;
        }
    }
}
/**/