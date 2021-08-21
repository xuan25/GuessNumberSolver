using GuessNumber.Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game
{
    class MyPuzzle : PuzzleBase
    {
        private static Random random = new Random();

        private Guess _answer = new Guess();
        private bool _isSolved = false;

        public MyPuzzle()
        {
            do
            {
                for (int i = 0; i < Config.Slots; i++)
                {
                    _answer[i] = random.Next(0, Config.Range);
                }
            } while (!_answer.IsValid());
            _isSolved = false;
        }

        public override bool IsSolved
        {
            get
            {
                return _isSolved;
            }
        }

        public override Response Guess(Guess guess)
        {
            if (!guess.IsValid())
            {
                Console.WriteLine("Guess not Valid!");
                Response tmp = new Response();
                tmp.A = -1; tmp.B = -1;
                return tmp;
            }
            Response result = _answer.GetResult(guess);
            if (result.A == Config.Slots && result.B == 0)
            {
                _isSolved = true;
            }
            return result;
        }

        public override string GetFormatedAnswer()
        {
            return _answer.ToString();
        }
    }
}
