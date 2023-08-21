using ChessChallenge.API;
using ChessChallenge.Example;

namespace Chess_Challenge.src.Evil_Bot
{
    public class EvilBot : IChessBot
    {
        IChessBot _bot = new BotB3S();

        public Move Think(Board board, Timer timer)
        {
            return _bot.Think(board, timer);
        }
    }
}