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

namespace GuessNumber.Game.DanmakuGame
{
    abstract class SemiDanmakuPuzzleBase : PuzzleBase, ILoginRequired, ISetExternalResponse
    {
        internal abstract uint DealerRoomId { get; }
        internal abstract string SendDanmakuPattern { get; }

        CookieCollection playerCookies = null;
        string playerCsrf = string.Empty;


        private bool _isSolved = false;

        public SemiDanmakuPuzzleBase()
        {
            _isSolved = false;
        }

        public void SetPlayerCookies(CookieCollection playerCookies)
        {
            this.playerCookies = playerCookies;

            foreach (Cookie cookie in playerCookies)
            {
                if (cookie.Name == "bili_jct")
                {
                    playerCsrf = cookie.Value;
                    break;
                }
            }
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
            PostGuessDanmaku(guess);
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

        private void PostGuessDanmaku(Guess guess)
        {
            string requestingGuess = string.Format(SendDanmakuPattern, guess.Select(x => x.ToString()).ToArray());

            string msg = HttpUtility.UrlEncode(requestingGuess);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.live.bilibili.com/msg/send");
            httpWebRequest.Method = "POST";
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(playerCookies);
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Accept = "application/json, text/javascript, */*; q=0.01";
            httpWebRequest.Referer = $"https://live.bilibili.com/{DealerRoomId}";

            Dictionary<string, string> dataDict = new Dictionary<string, string>{
                { "color", "16777215"},
                { "fontsize", "25"},
                { "mode", "1"},
                { "msg", msg},
                { "rnd", "0"},
                { "roomid", DealerRoomId.ToString()},
                { "bubble", "0"},
                { "csrf_token", playerCsrf},
                { "csrf", playerCsrf}
            };
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in dataDict)
            {
                stringBuilder.Append('&');
                stringBuilder.Append(keyValuePair.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(keyValuePair.Value);
            }
            string dataStr = stringBuilder.ToString(1, stringBuilder.Length - 1);
            byte[] data = Encoding.UTF8.GetBytes(dataStr);

            httpWebRequest.ContentLength = data.Length;

            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        string responseContent = streamReader.ReadToEnd();
                        Console.WriteLine($"{msg} - {responseContent}");
                    }
                }
            }
            else
            {
                throw new Exception(httpWebResponse.StatusDescription);
            }
        }
    }
}
