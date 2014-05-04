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
            if (start != now)
            {
                beginIndex.Add(start);
                endIndex.Add(now);
                subtopicList.Add(subtopicNow);

                start = now;
                subtopicNow = new List<string[]>(subtopicNow);
            }
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

        public Pair<string, double>[] getBlocksWithWeight(string[] queryTokens)
        {
            List<Pair<string, double>> result = new List<Pair<string, double>>();
            foreach (Pair<string, string[][]> block in getBlocks())
            {
                double subtopicWeight = 1.0;
                List<bool> preMatchList = new List<bool>();
                for (int i = 0; i < block.second.Length; i++)
                {
                    //print subtopic
                    //for (int j = 0; j < block.second[i].Length; j++)
                    //    Console.Write(block.second[i][j] + " ");
                    //Console.WriteLine("|");
                    //Console.ReadKey();

                    //calc weight
                    List<bool> matchList = new List<bool>();
                    foreach (string q in queryTokens)
                    {
                        if (block.second[i].Contains(q))
                            matchList.Add(true);
                        else
                            matchList.Add(false);
                    }

                    if (i != 0)
                    {
                        int appear = 0, disappear = 0;
                        for (int k = 0; k < queryTokens.Length; k++)
                        {
                            if (matchList[k] && !preMatchList[k])
                                appear++;
                            else if (!matchList[k] && preMatchList[k])
                                disappear++;
                        }

                        if (block.second[i].Contains("answer"))
                            appear = disappear = 0;


                        subtopicWeight *= Math.Pow(1.1, appear);
                        subtopicWeight *= Math.Pow(0.9, disappear);
                    }
                    preMatchList = new List<bool>(matchList);
                }
                result.Add(new Pair<string, double>(block.first, subtopicWeight));
            }

            return result.ToArray();
        }
    }
}
