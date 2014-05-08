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
            //read query list file
            List<string[]> queryList = new List<string[]>();
            StreamReader sr = new StreamReader(Setting.queryListFile);
            while (!sr.EndOfStream)
            {
                string query = sr.ReadLine().Substring(10);
                string[] queryTokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(query)));

                queryList.Add(queryTokens);
            }
            sr.Close();

            //foreach html
            if (Directory.Exists(Setting.HTML_DirectoryPath))
            {
                for (int qId = 1; qId <= 50; qId++)
                {
                    string[] files = Directory.GetFiles(Setting.HTML_DirectoryPath, "MC-E-" + String.Format("{0:D4}", qId) + "-*.html");
                    int HTMLcountInThisQuestion = files.Length;

                    int alreadyGetSentencesFromQid = 0, fileCount = 0;
                    for (int i = 1; alreadyGetSentencesFromQid < Setting.numOfSentencesEachQ && fileCount < HTMLcountInThisQuestion; i++)
                    {
                        string file = Setting.HTML_DirectoryPath + "\\MC-E-" + String.Format("{0:D4}", qId) + "-" + i + ".html";
                        DirectoryInfo di = new DirectoryInfo(file);

                        if (!File.Exists(file))
                            continue;
                        else
                            fileCount++;

                        Console.WriteLine(di.Name);

                        sr = new StreamReader(file);
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

                        Pair<string, double>[] parseResult = null;
                        parseResult = QA.ExtractBlocks(doc);
                        if (parseResult == null)
                        {
                            MainBodyDetector mbd = new MainBodyDetector(bodyNode, Setting.thresholdT);
                            TopicBlocks tbs = ExtractBlocks(bodyNode, mbd, titleTokens);
                            parseResult = (tbs == null ? null : tbs.getBlocksWithWeight(queryList[qId - 1].ToArray()));
                        }

                        alreadyGetSentencesFromQid +=SplitSentencesAndWriteFile(Setting.outputDirectoryPath + @"\" + di.Name + ".txt", parseResult, queryList[qId - 1].ToArray());
                    }
                }
            }

            Console.WriteLine("Finish!");
            Console.ReadKey();

        }
        static private string ExtractText(HtmlNode node)
        {
            string result = "";
            if (node.Name.Equals("script") || node.Name.Equals("noscript") || node.Name.Equals("style") || node.Name.Equals("#comment"))
                return " ";
            else if (node.ChildNodes.Count == 0)
                return WebUtility.HtmlDecode(node.InnerText.Replace("\n", " ").Replace("\r", " "))  + " ";
            else if (node.ChildNodes.Count > 0)
            {
                HtmlNodeCollection hnc = node.ChildNodes;
                foreach (HtmlNode n in hnc)
                    result += ExtractText(n);
            }

            return result;
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
                if (HeadRegex.IsMatch(node.Name) && node.Name.Length == 2)
                {
                    //save
                    tbs.SaveBlock();

                    //change header
                    int hx = Convert.ToInt16(node.Name.Substring(1));
                    string[] subtopicTokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(WebUtility.HtmlDecode(ExtractText(node)))));
                    tbs.addNewHeader(hx, subtopicTokens);

                    return tbs;
                }

                if (node.ChildNodes.Count == 0)
                {
                    tbs.addExtractedText(WebUtility.HtmlDecode(node.InnerText.Replace("\n", " ").Replace("\r", " ")));
                }
                else
                {
                    HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlNode n in hnc)
                    {
                        if (Setting.changeLineTags.Contains(n.Name))
                            tbs.addExtractedText("\n");

                        ExtractBlocks(n, mbd, null, tbs, false);

                        if (Setting.changeLineTags.Contains(n.Name))
                            tbs.addExtractedText("\n");

                    }
                }
            }
            if(isRoot)
                tbs.SaveBlock();

            return tbs;
        }
        static private int SplitSentencesAndWriteFile(string outputFileName, Pair<string, double>[] blocksAndWeight, string[] query)
        {
            if (!Directory.Exists(Setting.outputDirectoryPath))
                Directory.CreateDirectory(Setting.outputDirectoryPath);
            
            StreamWriter sw = new StreamWriter(outputFileName);

            if (blocksAndWeight == null)
            {
                sw.Close();
                return 0;
            }

            int result = 0;
            foreach (Pair<string, double> block in blocksAndWeight)
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

                        if (!NLPmethods.isQuestion(afterProcess))
                        {
                            string[] tokens = NLPmethods.Stemming(NLPmethods.FilterOutStopWords(NLPmethods.Tokenization(afterProcess)));

                            int tf = 0;
                            foreach (string token in tokens)
                                if (query.Contains(token))
                                    tf++;


                            if (tokens.Length >= 3 && tf >= 1)
                            {
                                result++;
                                sw.WriteLine(tf + "\t" + block.second + "\t" + afterProcess);
                            }
                        }
                    }
                }
            }
            sw.Close();
            return result;
        }
    }
}
