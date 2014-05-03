using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HTMLtoContent
{
    class MainBodyDetector
    {
        private Dictionary<HtmlNode, bool> eachNodeIsMainBody = new Dictionary<HtmlNode, bool>();
        private double threshold;
        public MainBodyDetector(HtmlNode node, double thresholdT)
        {
            if (node != null)
            {
                threshold = thresholdT;

                //build dictionary
                DetectEachNodeIsMainBody(node);
            }
        }
        public bool isMainBody(HtmlNode node)
        {
            return eachNodeIsMainBody[node];
        }
        private Pair<int, int> DetectEachNodeIsMainBody(HtmlNode node)
        {//Pair<int, int> : <numOfAllTokens, numOfLinkedTokens>
            if (!Setting.ignoreTags.Contains(node.Name))
            {
                bool isUnderLink = isUnderLinkNode(node);
                if (node.ChildNodes.Count == 0)
                {
                    eachNodeIsMainBody.Add(node, true);
                    int count = Program.NLPmethods.Tokenization(node.InnerText).Length;
                    if (isUnderLink)
                        return new Pair<int, int>(count, count);
                    else
                        return new Pair<int, int>(count, 0);
                    
                }
                else
                {
                    int numOfAllTokens = 0, numOfLinkedTokens = 0;
                    HtmlNodeCollection hnc = node.ChildNodes;
                    foreach (HtmlNode n in hnc)
                    {
                        Pair<int, int> result = DetectEachNodeIsMainBody(n);
                        numOfAllTokens += result.first;
                        numOfLinkedTokens += result.second;
                    }

                    eachNodeIsMainBody.Add(node, IsMainBody(numOfAllTokens, numOfLinkedTokens, isUnderLink, node.Name));
                    return new Pair<int, int>(numOfAllTokens, numOfLinkedTokens);
                }
            }
            return new Pair<int, int>(0, 0);
        }
        private bool isUnderLinkNode(HtmlNode node)
        {
            HtmlNode tempNode = node;
            while (tempNode.ParentNode != null)
            {
                if (tempNode.Name.Equals("a"))
                    return true;

                tempNode = tempNode.ParentNode;
            }

            return false;
        }
        private bool IsMainBody(int numOfAllTokens, int numOfLinkedTokens, bool underLink, string nodeName)
        {
            if (underLink || Setting.garnishTags.Contains(nodeName))
                return true;
            else if (numOfAllTokens == 0 || ((double)numOfLinkedTokens / (double)numOfAllTokens) >= threshold)
                return false;
            else
                return true;
        }

    }
}
