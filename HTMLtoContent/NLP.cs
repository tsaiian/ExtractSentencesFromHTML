using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using opennlp.tools.tokenize;
using opennlp.tools.sentdetect;
using opennlp.tools.util;

namespace HTMLtoContent
{
    class NLP
    {
        private SentenceDetectorME sentenceDetector;
        private TokenizerME tokenizer;

        public NLP()
        {
            //loading sentence detector model
            java.io.FileInputStream modelInpStream = new java.io.FileInputStream("model\\en-sent.bin");
            SentenceModel sentenceModel = new SentenceModel(modelInpStream);
            sentenceDetector = new SentenceDetectorME(sentenceModel);

            //loading tokenizer model
            modelInpStream = new java.io.FileInputStream("model\\en-token.bin");
            TokenizerModel tokenizerModel = new TokenizerModel(modelInpStream);
            tokenizer = new TokenizerME(tokenizerModel);
        }

        public string[] sentDetect(string str)
        {
            return sentenceDetector.sentDetect(str);
        }

        public string[] tokenization(string str)
        {
            Span[] tokenSpans = tokenizer.tokenizePos(str);	
			List<String> list = new List<String>();

			foreach(Span span in tokenSpans)
                list.Add(str.Substring(span.getStart(), span.getEnd() - span.getStart()));
			
            return list.ToArray();
        }

        public string stemming(string str)
        {
            return Stemmer.go(str);
        }

    }
}
