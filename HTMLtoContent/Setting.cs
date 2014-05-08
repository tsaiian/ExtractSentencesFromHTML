using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLtoContent
{
    class Setting
    {
        public const int numOfSentencesEachQ = 2000;

        //lexRank related
        public const double d = 0.85;
        public const double linkThreshold = 0.2;
        public const double convergenceThreshold = 0.0000000001;

        //main body detector related
        public const double thresholdT = 0.8;

        //tag related
        static public readonly string[] changeLineTags = { "p", "div", "marquee", "hr", "br", "img", "table", "frameset", "address", "body", "code", "ol", "option", "pre", "span", "ul" };
        static public readonly string[] garnishTags = { "a", "b", "i", "u", "ins", "strike", "s", "del", "kbd", "tt", "font", "var" };
        static public readonly string[] ignoreTags = { "script", "noscript", "style", "#comment" };

        //path related
        public const string queryListFile = "MC1-E-Queries.tsv";
        public const string HTML_DirectoryPath = @".\MC1-E-BSR";
        public const string outputDirectoryPath = @".\Converted";
    }
}
