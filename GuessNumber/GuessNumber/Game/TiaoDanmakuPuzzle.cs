using GuessNumber.Game.DanmakuGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuessNumber.Game
{
    class TiaoDanmakuPuzzle : DanmakuPuzzleBase
    {
        internal override uint DealerRoomId => 3796382;

        internal override uint DealerId => 8212729;

        internal override string SendDanmakuPattern => "{0}{1}{2}{3}";
        internal override string ReceiveIdPattern => "{0}{1}{2}{3}";

        private Regex receiveDanmakuMatcher = new Regex("(?<Id>\\d{4}) A: (?<A>\\d) B: (?<B>\\d)");
        internal override Regex ReceiveDanmakuMatcher => receiveDanmakuMatcher;

        public TiaoDanmakuPuzzle() : base()
        {

        }

        public TiaoDanmakuPuzzle(CookieCollection playerCookies) : base()
        {
            SetPlayerCookies(playerCookies);
        }
    }
}
