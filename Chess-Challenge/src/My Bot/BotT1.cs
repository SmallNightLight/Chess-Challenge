using ChessChallenge.API;
using System;
using System.Linq;

public class BotT1 : IChessBot
{
    //Temp variables
    private Board _board;

    private Move _mainMove;
    private Move _rootMove;

    //Other Variables
    private Timer _timer;
    private float _timeThisTurn;

    private (ulong, short, sbyte, Move, int)[] _transpositionTable = new (ulong, short, sbyte, Move, int)[0x800000];
    private int[,,] _historyTable;

    //Value of pieces (early game -> end game)
    private readonly short[] _pieceValues = { 82, 337, 365, 497, 1025, 20000, 94, 281, 297, 512, 936, 20000 };

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

    public BotT1()
    {
        UnpackedPestoTables = PackedPestoTables.Select(packedTable =>
        {
            int pieceType = 0;
            return decimal.GetBits(packedTable).Take(3).SelectMany(c => BitConverter.GetBytes(c).Select(square => (int)((sbyte)square * 1.461) + _pieceValues[pieceType++])).ToArray();
        }).ToArray();
    }

    public Move Think(Board board, Timer timer)
    {
        _historyTable = new int[2, 7, 64];

        _board = board;
        _timer = timer;

        _timeThisTurn = Math.Min(timer.MillisecondsRemaining / 28, timer.IncrementMilliseconds + (0.6f + (0.04f * Math.Min(board.PlyCount, 15))) * timer.GameStartTimeMilliseconds / 72f);

        for (int depth = 0; ;)
        {
            int evaluation = Negamax(++depth, 0, -100000, 100000, true);

            if (evaluation > 10000)
            {
                _rootMove = _mainMove;
                break;
            }

            if (timer.MillisecondsElapsedThisTurn < _timeThisTurn)
                _rootMove = _mainMove;
            else
                break;
        }

        return _rootMove;
    }

    private int Negamax(int depth, int ply, int alpha, int beta, bool do_null)
    {
        if (ply != 0 && _board.IsRepeatedPosition())
            return 0;

        bool quiescenceSearch = depth <= 0, in_check = _board.IsInCheck(), pv_node = beta - alpha > 1, can_futility_prune = false;
        int best_score = -200000, turn = _board.IsWhiteToMove ? 1 : 0;
        ulong key = _board.ZobristKey;

        if (in_check) 
            depth++;

        ref var transposition = ref _transpositionTable[_board.ZobristKey & 0x7FFFFF];

        if (transposition.Item1 == _board.ZobristKey && ply != 0 && transposition.Item3 >= depth && (transposition.Item5 == 1 || (transposition.Item5 == 0 && transposition.Item2 <= alpha) || (transposition.Item5 == 2 && transposition.Item2 >= beta)))
            return transposition.Item2;

        int currentScore = Eval();

        //Delta Pruning
        if (quiescenceSearch)
        {
            best_score = currentScore;

            if (best_score >= beta) 
                return beta;

            alpha = Math.Max(alpha, best_score);
        }
        else if (!pv_node && !in_check)
        {
            //Reverse Futility Pruning
            int factor = currentScore - 85 * depth;

            if (factor >= beta) 
                return factor;

            // Null Move Pruning
            if (do_null && depth >= 2)
            {
                _board.TrySkipTurn();
                int score = -Negamax(depth - 3 - depth / 6, ply + 1, -beta, 1 - beta, false);
                _board.UndoSkipTurn();

                if (score >= beta) 
                    return score;
            }

            // Futility Pruning Check
            can_futility_prune = depth <= 8 && currentScore + 40 + 60 * depth <= alpha;
        }

        Move transpositionMove = transposition.Item4;

        // Move Ordering
        Move[] moves = _board.GetLegalMoves(quiescenceSearch && !in_check).OrderByDescending(
            move =>
                move == transpositionMove ? 1000000 :
                move.IsCapture ? 1000 * (int)move.CapturePieceType - (int)move.MovePieceType :
                _historyTable[turn, (int)move.MovePieceType, move.TargetSquare.Index]
        ).ToArray();

        Move best_move = Move.NullMove;
        int start_alpha = alpha;
        for (int i = 0, new_score = 0; i < moves.Length; i++)
        {
            Move move = moves[i];

            bool tactical = pv_node || move.IsCapture || move.IsPromotion || in_check;

            // Futility Pruning
            if (can_futility_prune && !tactical && i > 0) 
                continue;

            _board.MakeMove(move);
            // Using local method to simplify multiple similar calls to Negamax
            int Search(int next_alpha, int R = 1) => -Negamax(depth - R, ply + 1, -next_alpha, -alpha, do_null);
            // PVS + LMR (Saves tokens, I will not explain, ask Tyrant)
            if (i == 0 || quiescenceSearch) new_score = Search(beta);
            else if ((new_score = tactical || i < 8 || depth < 3 ?
                                    alpha + 1 :
                                    Search(alpha + 1, 3)) > alpha &&
                (new_score = Search(alpha + 1)) > alpha)
                new_score = Search(beta);
            _board.UndoMove(move);

            if (new_score > best_score)
            {
                best_score = new_score;
                best_move = move;

                // Update bestmove
                if (ply == 0) 
                    _mainMove = move;

                // Improve alpha
                alpha = Math.Max(alpha, best_score);
                // Beta Cutoff
                if (alpha >= beta)
                {
                    if (!quiescenceSearch && !move.IsCapture) _historyTable[turn, (int)move.MovePieceType, move.TargetSquare.Index] += depth * depth;
                        break;
                }
            }

            // Check if time is expired
            if (_timer.MillisecondsElapsedThisTurn > _timeThisTurn)
                return 200000;
        }
        // If there are no moves return either checkmate or draw
        if (!quiescenceSearch && moves.Length == 0) 
            return in_check ? ply - 100000 : 0;

        transposition = (key, (short)best_score, (sbyte)depth, best_move, best_score >= beta ? 2 : best_score > start_alpha ? 1 : 0);

        return best_score;
    }

    private int Eval()
    {
        int middlegame = 0, endgame = 0, gamephase = 0, sideToMove = 2;

        for (; --sideToMove >= 0;)
        {
            for (int piece = -1, square; ++piece < 6;)
                for (ulong mask = _board.GetPieceBitboard((PieceType)piece + 1, sideToMove > 0); mask != 0;)
                {
                    //Gamephase, middlegame -> endgame
                    gamephase += _pieceWeight[piece];

                    //Material and square evaluation
                    square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ 56 * sideToMove;
                    middlegame += UnpackedPestoTables[square][piece];
                    endgame += UnpackedPestoTables[square][piece + 6];

                    // Bishop pair bonus
                    if (piece == 2 && mask != 0)
                    {
                        middlegame += 22;
                        endgame += 30;
                    }
                }
            middlegame = -middlegame;
            endgame = -endgame;
        }
        return (middlegame * gamephase + endgame * (24 - gamephase)) / 24 * (_board.IsWhiteToMove ? 1 : -1);
    }
}