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
    abstract class DanmakuPuzzleBase : PuzzleBase, ILoginRequired
    {
        internal abstract uint DealerRoomId { get; }
        internal abstract uint DealerId { get; }
        internal abstract string SendDanmakuPattern { get; }
        internal abstract string ReceiveIdPattern { get; }
        internal abstract Regex ReceiveDanmakuMatcher { get; }

        CookieCollection playerCookies = null;
        string playerCsrf = string.Empty;


        private bool _isSolved = false;
        private BiliLiveListener LiveListener { get; set; }

        public DanmakuPuzzleBase()
        {
            _isSolved = false;
            LiveListener = new BiliLiveListener(DealerRoomId, BiliLiveListener.Protocols.Tcp);
            LiveListener.ItemsRecieved += LiveListener_ItemsRecieved;
            LiveListener.Connect();
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

        private string requestingGuess = string.Empty;
        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private Response lastResult = null;

        private void LiveListener_ItemsRecieved(BiliLiveJsonParser.IItem[] items)
        {
            foreach(BiliLiveJsonParser.IItem item in items)
            {
                switch (item.Cmd)
                {
                    case BiliLiveJsonParser.Cmds.DANMU_MSG:
                        BiliLiveJsonParser.Danmaku danmaku = (BiliLiveJsonParser.Danmaku)item;
                        ParseDanmaku(danmaku);
                        break;
                }
            }
        }

        private void ParseDanmaku(BiliLiveJsonParser.Danmaku danmaku)
        {
            if (danmaku.Sender.Id != DealerId)
            {
                return;
            }

            Match match = ReceiveDanmakuMatcher.Match(danmaku.Message);
            if (!match.Success)
            {
                return;
            }

            string guess = match.Groups["ID"].Value;
            if (guess != requestingGuess)
            {
                return;
            }

            int a = int.Parse(match.Groups["A"].Value);
            int b = int.Parse(match.Groups["B"].Value);

            Response result = new Response(a, b);
            lastResult = result;

            manualResetEvent.Set();
        }

        private void PostGuessDanmaku(Guess guess)
        {
            requestingGuess = string.Format(ReceiveIdPattern, guess.Select(x => x.ToString()).ToArray());

            string danmaku = string.Format(SendDanmakuPattern, guess.Select(x => x.ToString()).ToArray());

            string msg = HttpUtility.UrlEncode(danmaku);
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
