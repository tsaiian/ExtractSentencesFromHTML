using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using opennlp.tools.tokenize;
using opennlp.tools.sentdetect;
using opennlp.tools.util;
using System.IO;

namespace HTMLtoContent
{
    class NLP
    {
        private SentenceDetectorME sentenceDetector;
        private TokenizerME tokenizer;
        private HashSet<string> stopwords = new HashSet<string>();

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

            //loading stop words list
            StreamReader sr = new StreamReader("english.stop.txt");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                stopwords.Add(Stemming(line));
                stopwords.Add(line);
            }
        }

        public string[] SentDetect(string str)
        {
            return sentenceDetector.sentDetect(str);
        }

        public string[] Tokenization(string str)
        {
            Span[] tokenSpans = tokenizer.tokenizePos(str);	
			List<String> list = new List<String>();

			foreach(Span span in tokenSpans)
                list.Add(str.Substring(span.getStart(), span.getEnd() - span.getStart()));
			
            return list.ToArray();
        }

        public string Stemming(string str)
        {
            return Stemmer.go(str);
        }

        public string[] Stemming(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = Stemming(words[i]);

            }
            return words;
        }

        public bool IsStopWord(string word)
        {
            return stopwords.Contains(word);
        }

        public string[] FilterOutStopWords(string[] words)
        {
            List<string> result = new List<string>();

            foreach (string word in words)
                if (!IsStopWord(word))
                    result.Add(word);

            return result.ToArray();
        }
    }
}
