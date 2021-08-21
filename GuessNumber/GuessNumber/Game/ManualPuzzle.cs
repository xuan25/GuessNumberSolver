using BiliLive;
using GuessNumber.Game.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GuessNumber.Game
{
    class ManualPuzzle : PuzzleBase, ISetExternalResponse
    {
        private bool _isSolved = false;

        public ManualPuzzle()
        {
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

            lastResult = null;

            manualResetEvent.Reset();
            // Get external response manually
            manualResetEvent.WaitOne();

            Response result = lastResult;
            if (result.A == Config.Slots && result.B == 0)
            {
                _isSolved = true;
            }
            return result;
        }

        public override string GetFormatedAnswer()
        {
            throw new InvalidOperationException();
        }

        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private Response lastResult = null;

        public void SetExternalResponse(int a, int b)
        {
            Response result = new Response(a, b);
            lastResult = result;

            manualResetEvent.Set();
        }
    }
}
