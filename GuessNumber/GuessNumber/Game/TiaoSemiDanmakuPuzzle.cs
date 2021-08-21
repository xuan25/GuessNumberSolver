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
    class TiaoSemiDanmakuPuzzle : SemiDanmakuPuzzleBase
    {
        internal override uint DealerRoomId => 3796382;

        internal override string SendDanmakuPattern => "{0}{1}{2}{3}";

        public TiaoSemiDanmakuPuzzle() : base()
        {

        }

        public TiaoSemiDanmakuPuzzle(CookieCollection playerCookies) : base()
        {
            SetPlayerCookies(playerCookies);
        }
    }
}
