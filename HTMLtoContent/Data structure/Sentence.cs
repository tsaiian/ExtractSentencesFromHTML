using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLtoContent
{
    class Sentence
    {
        public string sentnece;
        public string senWithChunk = "";
        public string[] tokens;
        public string[] stemTokens;

        public int tf;
        public double topicWeight;
        public double logRank;
        public double lexRank;
        public double lda;
        public int searchRank;

        public Sentence(string sen, string[] split, string[] stemSplit, int f, double tw, int rank)
        {
            sentnece = sen;
            tokens = split;
            stemTokens = stemSplit;
            tf = f;
            topicWeight = tw;
            searchRank = rank;
            logRank = (double)1 / Math.Log(rank + 1, 2);

            string[] chunkerTokens = Program.NLPmethods.Chunking(tokens);
            for (int i = 0; i < tokens.Length; i++)
                senWithChunk += tokens[i] + "(" + chunkerTokens[i] + ") ";
        }


    }
}
