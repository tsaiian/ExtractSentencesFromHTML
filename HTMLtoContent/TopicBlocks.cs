using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HTMLtoContent
{
    class TopicBlocks
    {
        private List<int> beginIndex = null;
        private List<int> endIndex = null;
        private List<List<string[]>> subtopicList = null;

        private List<string[]> subtopicNow = null;
        private int start = 0, now = 0;
        private string fullText = String.Empty;

        public TopicBlocks(string[] titleTokens)
        {
            beginIndex = new List<int>();
            endIndex = new List<int>();
            subtopicList = new List<List<string[]>>();
            subtopicNow = new List<string[]>();
            start = now = 0;

            if (titleTokens == null)
                subtopicNow.Add(null);
            else
                subtopicNow.Add(titleTokens);

            
        }

        public void SaveBlock()
        {
            beginIndex.Add(start);
            endIndex.Add(now);
            subtopicList.Add(subtopicNow);
            start = now;

            subtopicNow = new List<string[]>(subtopicNow);
        }

        public void addNewHeader(int headerLevel, string[] subtopicTokens)
        {
            if (subtopicNow.Count - 1 > headerLevel)
            {
                for (int i = subtopicNow.Count - 1; i >= headerLevel; i--)
                    subtopicNow.RemoveAt(i);
            }
            else if (subtopicNow.Count - 1 < headerLevel)
            {
                for (int i = subtopicNow.Count - 1; i < headerLevel - 1; i++)
                    subtopicNow.Add(null);
            }
            else
                subtopicNow.RemoveAt(headerLevel);

            subtopicNow.Add(subtopicTokens);
        }

        public void addExtractedText(string s)
        {
            now += s.Length;
            fullText += s;
        }

        public Pair<string, string[][]>[] getBlocks()
        {
            List<Pair<string, string[][]>> blocksWithSubtopic = new List<Pair<string,string[][]>>();

            if (fullText.Equals(String.Empty))
                return blocksWithSubtopic.ToArray();

            try
            {
                for (int i = 0; i < subtopicList.Count; i++)
                {
                    string block = fullText.Substring(beginIndex[i], endIndex[i] - beginIndex[i] - 1);

                    List<string[]> simplifySubtopicList = new List<string[]>();
                    foreach (string[] titleTokens in subtopicList[i])
                        if (titleTokens != null)
                            simplifySubtopicList.Add(titleTokens);

                    Pair<string, string[][]> pair = new Pair<string, string[][]>(block, simplifySubtopicList.ToArray());
                    blocksWithSubtopic.Add(pair);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return blocksWithSubtopic.ToArray();
        }
    }
}
