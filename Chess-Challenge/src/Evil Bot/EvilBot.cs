using ChessChallenge.API;

namespace Chess_Challenge.src.Evil_Bot
{
    public class EvilBot : IChessBot
    {
        IChessBot _bot = new V6();

        public Move Think(Board board, Timer timer)
        {
            return _bot.Think(board, timer);
        }
    }
}