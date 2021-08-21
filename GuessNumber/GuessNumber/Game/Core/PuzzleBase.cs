using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game.Core
{
    abstract class PuzzleBase
    {
        public abstract bool IsSolved
        {
            get;
        }

        public abstract Response Guess(Guess guess);

        public abstract string GetFormatedAnswer();
    }
}
