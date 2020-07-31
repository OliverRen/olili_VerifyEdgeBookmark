using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentConsole.Library;

using HtmlAgilityPack;

using SharpOTP;

namespace VerifyEdgeBookmark
{
    public class CheckServer
    {
        private static readonly Actor server = GenServer.Start<CheckServer>("check",32);
        private static Uri proxy = new Uri("http://127.0.0.1:1081");
        private static HttpClient httpClient = new HttpClient(handler: new HttpClientHandler() { Proxy = new WebProxy(proxy, false) }) { Timeout = TimeSpan.FromSeconds(30) };

        public async Task<bool> HandleCall(HtmlNode node)
        {
            var href = node.GetAttributeValue("href", "");
            if (string.IsNullOrEmpty(href))
                return false;

            HttpResponseMessage result = null;
            try
            {
                result = await httpClient.GetAsync(new Uri(href), HttpCompletionOption.ResponseContentRead);
            }
            catch (Exception e)
            {
                LogServer.WriteLine($"{node.InnerText} ({href}) {Environment.NewLine} task occur an exception,msg:{e.Message}");
                return false;
            }

            if (result.StatusCode != HttpStatusCode.OK &&
                result.StatusCode != HttpStatusCode.Forbidden)
            {
                var respData = result.Content.ReadAsStringAsync().Result;
                var msg = respData.Substring(0, Math.Min(250,respData.Length));
                try
                {
                    var respDoc = new HtmlDocument();
                    respDoc.LoadHtml(respData);
                    var titleNode = respDoc.DocumentNode.Descendants("title").FirstOrDefault();
                    var title = titleNode?.InnerText;

                    LogServer.WriteLine($"{node.InnerText} ({href}) {Environment.NewLine} result:{result.StatusCode} {Environment.NewLine} title:{title} {Environment.NewLine} {msg}");
                }
                catch (Exception e)
                {
                    LogServer.WriteLine($"{node.InnerText} ({href}) {Environment.NewLine} result:{result.StatusCode} {Environment.NewLine} {msg}");
                    return false;
                }
            }

            return true;
        }

        public static Task<bool> Check(HtmlNode node)
        {
            return server.Call<bool>(node);
        }
    }

    public class LogServer
    {
        private static readonly Actor server = GenServer.Start<LogServer>();
        private static int color = (int)ConsoleColor.Black;

        private static ConsoleColor GetColor()
        {
            Interlocked.Increment(ref color);
            return (ConsoleColor)((color % (int)ConsoleColor.White) + 1);
        }

        public async Task HandleCall(string str)
        {
            str.WriteLine(GetColor(),lineBreaks:1);
        }

        public static void WriteLine(string str)
        {
            server.Call(str);
        }
    }
}