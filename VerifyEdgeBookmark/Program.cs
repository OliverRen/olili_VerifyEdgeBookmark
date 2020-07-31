using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentConsole.Library;

using HtmlAgilityPack;

namespace VerifyEdgeBookmark
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(30, 30).WriteLine();
            ThreadPool.SetMaxThreads(200, 2000).WriteLine();
            
            "Enter Microsoft Edge Bookmark filepath:".WriteLine(
                new WriteOptions()
                {
                    ForeColor = ConsoleColor.Red,
                }, new FluentConsoleSettings()
                {
                    LineWrapOption = LineWrapOption.Auto,
                });

            var filepath=Console.ReadLine();
            if (string.IsNullOrEmpty(filepath))
                filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "bookmark.html");

            var doc = new HtmlDocument();
            doc.Load(filepath);
            
            var datas = doc.DocumentNode.Descendants("a").ToArray();
            var tasklist = new List<Task<bool>>();
            foreach (var node in datas)
            {
                tasklist.Add(CheckServer.Check(node));
            }

            Task.WhenAll(tasklist).Wait();
            "finish!".WriteLineWait(ConsoleColor.Red);
        }
    }
}