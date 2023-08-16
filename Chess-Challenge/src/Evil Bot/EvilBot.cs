using ChessChallenge.API;
using ChessChallenge.Example;

namespace Chess_Challenge.src.Evil_Bot
{
    public class EvilBot : IChessBot
    {
        IChessBot _bot = new BotB1C2();

        public Move Think(Board board, Timer timer)
        {
            return _bot.Think(board, timer);
        }
    }
}