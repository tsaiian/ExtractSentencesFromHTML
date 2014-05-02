using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HTMLtoContent
{
    class Program
    {
        private const double thresholdT = 0.8;
        static private List<string> list = new List<string>();
        static public NLP NLPmethods = new NLP();
        static void Main(string[] args)
        {
            List<string> query = new List<string>(NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization("java vs python text processing"))));

            string[] files = Directory.GetFiles(@".\MC1-E-BSR", "*.html");

            int k = 0;
            foreach (string file in files)
            {
                DirectoryInfo di = new DirectoryInfo(file);
                StreamReader sr = new StreamReader(file);

                Console.WriteLine(di.Name);
                string html = sr.ReadToEnd();
                sr.Close();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                HtmlNode.ElementsFlags.Remove("form");
                doc.LoadHtml(html);
                HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//body");
                //NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(titleNode.InnerText)))
                HtmlNode titlNode = doc.DocumentNode.SelectSingleNode("//html//head//title");
                string[] titleTokens = null;
                if (titlNode != null)
                    titleTokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(WebUtility.HtmlDecode(titlNode.InnerText))));

                TopicBlocks tbs = new TopicBlocks(titleTokens);
                if (bodyNode != null)
                {
                    MainBodyDetector mbd = new MainBodyDetector(bodyNode, thresholdT);
                    Traversal(bodyNode, mbd, tbs);
                }

                PreprocessingAndWrite(@".\Converted\" + di.Name + ".txt", tbs, query);

                //暫時先處理一個html就好
                //k++;
                //if(k == 2)
                    //break; 
                //Console.ReadKey();
            }

            Console.WriteLine("Finish!");
            Console.ReadKey();

        }
        static private void Traversal(HtmlAgilityPack.HtmlNode node, MainBodyDetector mbd, TopicBlocks tbs)
        {
            if (node.Name.Equals("script") || node.Name.Equals("noscript") || node.Name.Equals("style") || node.Name.Equals("#comment") || !mbd.isMainBody(node))
                tbs.addExtractedText("\n");
            else
            {
                Regex HeadRegex = new Regex("h[1-6]");
                if (HeadRegex.IsMatch(node.Name))
                {
                    //save
                    tbs.SaveBlock();

                    //change header
                    int hx = Convert.ToInt16(node.Name.Substring(1));
                    string[] subtopicTokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(WebUtility.HtmlDecode(node.InnerText))));
                        //Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    tbs.addNewHeader(hx, subtopicTokens);
                }

                if (node.ChildNodes.Count == 0)
                {
                    string result = WebUtility.HtmlDecode(node.InnerText);
                    tbs.addExtractedText(result);
                }
                else
                {
                    HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlNode n in hnc)
                        Traversal(n, mbd, tbs);
                }
            }
        }
        static private void PreprocessingAndWrite(string outputFileName, TopicBlocks tbs, List<string> query)
        {
            StreamWriter sw = new StreamWriter(outputFileName);

            foreach (Pair<string, string[][]> block in tbs.getBlocks())
            {
                string[] lines = block.first.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] sentences = NLPmethods.SentDetect(line);
                    foreach (string s in sentences)
                    {
                        string afterTrim = s.Trim(new char[] { '\t', ' ' });
                        string[] tokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(afterTrim)));

                        int tf = 0;
                        foreach (string token in tokens)
                        {
                            if (query.Contains(token))
                                tf++;
                        }

                        if (tokens.Length >= 3 && tf >= 1)
                        {
                            sw.Write("Subtopic:\t");
                            for (int i = 0; i < block.second.Length; i++)
                            {
                                for (int j = 0; j < block.second[i].Length; j++)
                                    sw.Write(block.second[i][j] + " ");
                                sw.Write("|");
                            }

                            sw.WriteLine();
                            sw.WriteLine(tf + "\t" + afterTrim);
                        }
                    }
                }
            }
            sw.Close();
        }
    }
}
