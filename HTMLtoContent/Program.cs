using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;


namespace HTMLtoContent
{
    class Program
    {
        static public NLP NLPmethods = new NLP();
        static void Main(string[] args)
        {
            string[] query = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(Setting.query)));

            string[] files = Directory.GetFiles(Setting.HTML_DirectoryPath, "*.html");

            int k = 0;
            foreach (string file in files)
            {
                DirectoryInfo di = new DirectoryInfo(file);
                StreamReader sr = new StreamReader(file);

                Console.WriteLine(di.Name);
                string html = sr.ReadToEnd();
                sr.Close();

                HtmlDocument doc = new HtmlDocument();
                HtmlNode.ElementsFlags.Remove("form");
                doc.LoadHtml(html);

                HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//body");
                HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//html//head//title");

                string[] titleTokens = null;
                if (titleNode != null)
                    titleTokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(WebUtility.HtmlDecode(titleNode.InnerText))));

                MainBodyDetector mbd = new MainBodyDetector(bodyNode, Setting.thresholdT);
                TopicBlocks tbs = ExtractBlocks(bodyNode, mbd, titleTokens);

                SplitSentencesAndWriteFile(Setting.outputDirectoryPath + @"\" + di.Name + ".txt", tbs, query.ToArray());

                //暫時先處理一個html就好
                //k++;
                //if(k == 2)
                    //break; 
                //Console.ReadKey();
            }

            Console.WriteLine("Finish!");
            Console.ReadKey();

        }
        static private TopicBlocks ExtractBlocks(HtmlNode node, MainBodyDetector mbd, string[] titleTokens, TopicBlocks tbs = null, bool isRoot = true)
        {
            if (node == null)
                return null;

            if(tbs == null)
                tbs = new TopicBlocks(titleTokens);

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
                    tbs.addNewHeader(hx, subtopicTokens);

                    return tbs;
                }

                if (node.ChildNodes.Count == 0)
                {
                    HtmlNode previousSibling = node.PreviousSibling;

                    string result = "";
                    if (previousSibling != null && Setting.changeLineTags.Contains(previousSibling.Name))
                        result = "\n" + WebUtility.HtmlDecode(node.InnerText.Replace("\n", " ").Replace("\r", " "));
                    else
                        result = WebUtility.HtmlDecode(node.InnerText.Replace("\n", " ").Replace("\r", " "));

                    tbs.addExtractedText(result);
                }
                else
                {
                    HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlNode n in hnc)
                        ExtractBlocks(n, mbd, null, tbs, false);
                }
            }
            if(isRoot)
                tbs.SaveBlock();

            return tbs;
        }
        static private void SplitSentencesAndWriteFile(string outputFileName, TopicBlocks tbs, string[] query)
        {
            StreamWriter sw = new StreamWriter(outputFileName);

            if (tbs == null)
            {
                sw.Close();
                return;
            }

            foreach (Pair<string, double> block in tbs.getBlocksWithWeight(query))
            {
                string[] lines = block.first.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] sentences = NLPmethods.SentDetect(line);
                    foreach (string s in sentences)
                    {
                        string afterProcess = s.Trim(new char[] { '\t', ' ' , '-', '*'});

                        Regex whiteRegex = new Regex("[ \t]+");
                        afterProcess = whiteRegex.Replace(afterProcess, " ");

                        string[] tokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(afterProcess)));

                        int tf = 0;
                        foreach (string token in tokens)
                            if (query.Contains(token))
                                tf++;
                        

                        if (tokens.Length >= 3 && tf >= 1)
                        {
                            sw.WriteLine(tf + "\t" + block.second + "\t" + afterProcess);
                        }
                    }
                }
            }
            sw.Close();
        }
    }
}
