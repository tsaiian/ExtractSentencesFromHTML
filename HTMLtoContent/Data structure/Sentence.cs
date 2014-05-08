using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLtoContent
{
    class Sentence
    {
        public string sentnece;
        public string[] tokens;

        public int tf;
        public double topicWeight;
        public double logRank;
        public double lexRank;

        public Sentence()
        {

        }

        public Sentence(string sen, string[] split, int f, double tw, int rank)
        {
            sentnece = sen;
            tokens = split;
            tf = f;
            topicWeight = tw;
            logRank = (double)1 / Math.Log(rank + 1, 2);
        }


    }
}
