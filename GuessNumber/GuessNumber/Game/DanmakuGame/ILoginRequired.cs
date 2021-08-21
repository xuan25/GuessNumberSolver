using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game.DanmakuGame
{
    interface ILoginRequired
    {
        void SetPlayerCookies(CookieCollection playerCookies);
    }
}
