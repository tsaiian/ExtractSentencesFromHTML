using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;

namespace HTMLtoContent
{
    class QA
    {
        static public Pair<string, double>[] ExtractBlocks(HtmlDocument doc)
        {
            Pair<string, double>[] result = null;
            if (isStackOverflow(doc))
            {
                Console.WriteLine("[Info] Stack Overflow page");
                result = parseStackOverflow(doc);
            }
            else if (isYahooAnswers(doc))
            {
                Console.WriteLine("[Info] Yahoo Answers page");
                result = parseYahooAnswers(doc);
            }

            return result;
        }

        #region Stack Overflow related function
        static private bool isStackOverflow(HtmlDocument doc)
        {
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//html//head//title");
            if (titleNode != null && titleNode.InnerText.EndsWith(" - Stack Overflow"))
                return true;
            return false;
        }

        static private Pair<string, double>[] parseStackOverflow(HtmlDocument doc)
        {
            List<string> answerContent = new List<string>();
            List<int> answerVoteCount = new List<int>();
            List<Pair<string, double>> result = new List<Pair<string, double>>();
            string question = String.Empty;
            try
            {
                //question
                question = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[@class='question']//div[@class='post-text']").InnerText);

                //answer
                HtmlNodeCollection hnc = doc.DocumentNode.SelectNodes("//div[@class='answer']");
                foreach (HtmlNode n in hnc)
                {
                    string content = WebUtility.HtmlDecode(n.SelectSingleNode(".//div[@class='post-text']").InnerText);
                    int voteCount = Convert.ToInt16(n.SelectSingleNode(".//span[@class='vote-count-post ']").InnerText);

                    answerContent.Add(content);
                    answerVoteCount.Add(voteCount);
                }
            }
            catch (Exception)
            {
                return null;
            }

            //create question-weight pair
            result.Add(new Pair<string, double>(question, 1.0));

            //create answer-weight pair
            int totalVoteCount = 0;
            foreach (int i in answerVoteCount)
                totalVoteCount += i;

            double averageVoteCount = (double)totalVoteCount / (double)answerVoteCount.Count;
            
            for (int i = 0; i < answerContent.Count; i++)
            {
                double weight = Math.Pow((answerVoteCount[i] / averageVoteCount), Math.Log(totalVoteCount, 10));
                result.Add(new Pair<string, double>(answerContent[i], weight));
            }

            return result.ToArray();
        }
        #endregion

        #region Yahoo Answers related function
        static private bool isYahooAnswers(HtmlDocument doc)
        {
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//html//meta[@name='title']");
            if (titleNode != null && titleNode.GetAttributeValue("content", "").EndsWith(" - Yahoo Answers"))
                return true;
            return false;
        }

        static private Pair<string, double>[] parseYahooAnswers(HtmlDocument doc)
        {
            List<string> answerContent = new List<string>();
            List<Pair<string, double>> result = new List<Pair<string, double>>();
            string question = String.Empty;
            string bestAnswer = String.Empty;
            try
            {
                //question
                question = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[@id='yan-question']//div[@class='qa-container']/div[2]").InnerText);
                question = question.Replace("<!-- Adding details to question -->", "");
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                //best answer
                bestAnswer = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[@id='ya-ba']//div[@class='content']").InnerText);
                answerContent.Add(bestAnswer);
            }
            catch (Exception)
            {
            }

            try
            {
                //other answer
                HtmlNodeCollection hnc = doc.DocumentNode.SelectNodes("//div[@class='mod']//li[@class='first-child']");
                foreach (HtmlNode n in hnc)
                {
                    string content = WebUtility.HtmlDecode(n.SelectSingleNode(".//div[@class='ya-oa-cont']/div[1]").InnerText);
                    answerContent.Add(content);
                }
            }
            catch (Exception)
            {
            }

            //create question-weight pair
            result.Add(new Pair<string, double>(question, 1.0));

            //create answer-weight pair
            double averageRank = (double)(answerContent.Count + 1)/2;
            for (int i = 0; i < answerContent.Count; i++)
            {
                double weight = (answerContent.Count -  i) / averageRank;
                result.Add(new Pair<string, double>(answerContent[i], weight));
            }

            return result.ToArray();
        }
        #endregion
    }
}
