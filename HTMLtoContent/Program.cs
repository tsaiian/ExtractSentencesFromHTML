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
                
                string strResult = "";
                if (doc.DocumentNode.SelectSingleNode("//body") != null)
                {
                    length = new Dictionary<HtmlNode, string>();
                    score = new Dictionary<HtmlNode, double>();

                    MainBodyDetector mbd = new MainBodyDetector(doc.DocumentNode.SelectSingleNode("//body"), thresholdT);
                    strResult = ExtractText(doc.DocumentNode.SelectSingleNode("//body"), mbd, doc.DocumentNode.SelectSingleNode("//body"));
                    Traversal(doc.DocumentNode.SelectSingleNode("//body"), mbd, doc.DocumentNode.SelectSingleNode("//body"));
                }
                else
                    strResult = "";


                PreprocessingAndWrite(@".\Converted\" + di.Name + ".txt", strResult, query);

                //暫時先處理一個html就好
                //k++;
                //if(k == 2)
                    //break; 
                //Console.ReadKey();
            }

            Console.WriteLine("Finish!");
            Console.ReadKey();

        }

        static private void Traversal(HtmlNode node, MainBodyDetector mbd, HtmlNode root)
        {
            if (node.Name.Equals("script") || node.Name.Equals("noscript") || node.Name.Equals("style") || node.Name.Equals("#comment") || !mbd.isMainBody(node))
                return;
            else
            {
                if (node.ChildNodes.Count == 0)
                {
                    return ;
                }
                else
                {
                    HtmlNode parentNode = node.ParentNode;
                    Regex blockRegex = new Regex(@"\[[0-9\]]+");
                    string xPath = blockRegex.Replace(parentNode.XPath, String.Empty).Replace("/#text", "").Replace("/#comment", "");

                    double aver, totalLength = 0;
                    HtmlNodeCollection hnc = null;
                    try
                    {
                        hnc = root.SelectNodes(xPath);
                        foreach (HtmlNode n in hnc)
                        {
                            if (length.ContainsKey(n))
                                totalLength += length[n].Length;
                        }
                    }
                    catch (Exception e)
                    {
                        aver = 0.0;
                    }


                    
                    if (totalLength != 0)
                        aver = (double)length[node].Length / ((double)totalLength / (double)hnc.Count);
                    else
                        aver = 0;

                    //Console.WriteLine(aver);
                    //Console.ReadKey();

                    score.Add(node, aver);

                    hnc = node.ChildNodes;
                    foreach (HtmlAgilityPack.HtmlNode n in hnc)
                        Traversal(n, mbd, root);
                    

                }
            }
        }
        static private Dictionary<HtmlNode, string> length = null;
        static private Dictionary<HtmlNode, double> score = null;
        static private string ExtractText(HtmlNode node, MainBodyDetector mbd, HtmlNode root)
        {
            if (node.Name.Equals("script") || node.Name.Equals("noscript") || node.Name.Equals("style") || node.Name.Equals("#comment") || !mbd.isMainBody(node))
            {
                return "\n";
            }
            else
            {
                string result = "";
                if (node.ChildNodes.Count == 0)
                {
                    result = WebUtility.HtmlDecode(node.InnerText);
                }
                else
                {
                    HtmlAgilityPack.HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlAgilityPack.HtmlNode n in hnc)
                    {
                        result += ExtractText(n, mbd, root);
                    }
                }
                length.Add(node, result);

                return result;
            }
        }
        static private double FindMaxScoreNode(string line)
        {
            double max = 0;
            foreach (KeyValuePair<HtmlNode, string> kvp in length)
            {
                if (kvp.Value.Contains(line))
                {
                    if (score.ContainsKey(kvp.Key) && score[kvp.Key] > max)
                        max = score[kvp.Key];
                }
            }

            return max;
        }

        static private void PreprocessingAndWrite(string outputFileName, string str, List<string> query)
        {
            StreamWriter sw = new StreamWriter(outputFileName);

            string[] lines = str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
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
                        sw.WriteLine(tf + "\t" + FindMaxScoreNode(afterTrim) + "\t" + afterTrim);
                }
            }
            sw.Close();
        }


    }
}
