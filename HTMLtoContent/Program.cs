using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace HTMLtoContent
{
    class Program
    {
        private const double thresholdT = 0.8;
        static private List<string> list = new List<string>();
        static private NLP NLPmethods = new NLP();
        static void Main(string[] args)
        {
            List<string> query = new List<string>(NLPmethods.tokenization(NLPmethods.stemming("java vs python text processing")));

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
                    strResult = ExtractText(doc.DocumentNode.SelectSingleNode("//body"));
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
        static private bool isUnderLinkNode(HtmlAgilityPack.HtmlNode node)
        {
            HtmlAgilityPack.HtmlNode tempNode = node;
            while (tempNode.ParentNode != null)
            {
                if (tempNode.Name.Equals("a"))
                    return true;
                
                tempNode = tempNode.ParentNode;
            }

            return false;
        }

        static private bool isNotMainBody(HtmlAgilityPack.HtmlNode node)
        {
            Pair<int, int> result = findNumberOf_AllToken_And_LinkedToken(node);
            int numOfAllTokens = result.first;
            int numOfLinkedTokens = result.second;


            if (isUnderLinkNode(node))
                return false;
            else if (numOfAllTokens == 0 || (double)(numOfLinkedTokens / numOfAllTokens) >= thresholdT)
                return true;
            else
                return false;
        }

        static private Pair<int, int> findNumberOf_AllToken_And_LinkedToken(HtmlAgilityPack.HtmlNode node)
        {
            if (!node.Name.Equals("script") && !node.Name.Equals("noscript") && !node.Name.Equals("style") && !node.Name.Equals("#comment"))
            {
                bool isUnderLink = isUnderLinkNode(node);
                if (node.ChildNodes.Count == 0)
                {
                    if (isUnderLink)
                    {
                        int count = NLPmethods.tokenization(node.InnerText).Length;
                        return new Pair<int, int>(count, count);
                    }
                    else
                    {
                        int count = NLPmethods.tokenization(node.InnerText).Length;
                        return new Pair<int, int>(count, 0);
                    }
                }
                else
                {
                    int numOfAllTokens = 0, numOfLinkedTokens = 0;
                    HtmlAgilityPack.HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlAgilityPack.HtmlNode n in hnc)
                    {
                        Pair<int, int> result = findNumberOf_AllToken_And_LinkedToken(n);
                        numOfAllTokens += result.first;
                        numOfLinkedTokens += result.second;
                    }
                    return new Pair<int, int>(numOfAllTokens, numOfLinkedTokens);
                }
            }
            return new Pair<int, int>(0, 0);
        }
        static private string ExtractText(HtmlAgilityPack.HtmlNode node)
        {
            if (isNotMainBody(node) || node.Name.Equals("script") || node.Name.Equals("noscript") || node.Name.Equals("style") || node.Name.Equals("#comment"))
            {
                return "\n";
            }
            else
            {
                if (node.ChildNodes.Count == 0)
                {
                    return WebUtility.HtmlDecode(node.InnerText);
                }
                else
                {
                    string result = "";
                    HtmlAgilityPack.HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlAgilityPack.HtmlNode n in hnc)
                    {
                        result += ExtractText(n);
                    }
                    return result;
                }
            }
        }
        static private void PreprocessingAndWrite(string outputFileName, string str, List<string> query)
        {
            StreamWriter sw = new StreamWriter(outputFileName);

            string[] lines = str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] sentences = NLPmethods.sentDetect(line);
                foreach (string s in sentences)
                {
                    string afterTrim = s.Trim(new char[] { '\t', ' ' });
                    string[] tokens = NLPmethods.tokenization(afterTrim);

                    int tf = 0;
                    foreach (string token in tokens)
                    {
                        if (query.Contains(token))
                            tf++;
                    }


                    if (tokens.Length >= 3 && tf >= 1)
                        sw.WriteLine(tf + "\t" + afterTrim);
                }


                
            }
            sw.Close();
        }


    }
}
