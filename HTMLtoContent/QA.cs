using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HTMLtoContent
{
    class QA
    {
        static public Pair<string, double>[] ExtractBlocks(HtmlDocument doc)
        {
            Pair<string, double>[] result = null;
            if (isStackOverflow(doc))
                result = parseStackOverflow(doc);
            


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
                question = doc.DocumentNode.SelectSingleNode("//div[@class='question']//div[@class='post-text']").InnerText;

                //answer
                HtmlNodeCollection hnc = doc.DocumentNode.SelectNodes("//div[@class='answer']");
                foreach (HtmlNode n in hnc)
                {
                    string content = n.SelectSingleNode(".//div[@class='post-text']").InnerText;
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
    }
}
