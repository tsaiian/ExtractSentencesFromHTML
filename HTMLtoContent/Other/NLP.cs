﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using opennlp.tools.tokenize;
using opennlp.tools.sentdetect;
using opennlp.tools.util;
using System.Globalization;
using opennlp.tools.postag;
using opennlp.tools.chunker;
using System.IO;

namespace HTMLtoContent
{
    class NLP
    {
        private SentenceDetectorME sentenceDetector;
        private TokenizerME tokenizer;
        private POSTaggerME tagger;
        private ChunkerME chunker;
        private HashSet<string> stopwords = new HashSet<string>();


        public NLP()
        {
            //loading sentence detector model
            java.io.FileInputStream modelInpStream = new java.io.FileInputStream("Resources\\en-sent.bin");
            SentenceModel sentenceModel = new SentenceModel(modelInpStream);
            sentenceDetector = new SentenceDetectorME(sentenceModel);

            //loading tokenizer model
            modelInpStream = new java.io.FileInputStream("Resources\\en-token.bin");
            TokenizerModel tokenizerModel = new TokenizerModel(modelInpStream);
            tokenizer = new TokenizerME(tokenizerModel);

            modelInpStream = new java.io.FileInputStream("Resources\\en-pos-maxent.bin");
            POSModel posModel = new POSModel(modelInpStream);
            tagger = new POSTaggerME(posModel);

            modelInpStream = new java.io.FileInputStream("Resources\\en-chunker.bin");
            ChunkerModel chunkerModel = new ChunkerModel(modelInpStream);
            chunker = new ChunkerME(chunkerModel);

            //loading stop words list
            StreamReader sr = new StreamReader("Resources\\english.stop.txt");
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

        public string[] Chunking(string[] tokens)
        {
            string[] pos = tagger.tag(tokens);
            return Chunking(tokens, pos);
        }

        public string[] Chunking(string[] tokens, string[] pos)
        {
            return chunker.chunk(tokens, pos);
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

        public bool isQuestion(string line)
        {
            //判斷句子是否為問句, by 5W1H + "?" , case sensitive
            string[] star = { "What", "Who", "Which", "When", "Where", "Why", "How" };
            string[] verb = { "is ", "was ", "were ", "are ", "do ", "does ", "did ", "can ", "could ", "will ", "would ", "should" };
            string[] token = { " ", "'" };
            if (line.EndsWith("?"))
                return true;
            foreach (string s in star)
            {
                //For case :what's / who's
                if (line.StartsWith(s + token[1], true, new CultureInfo("en-US"))) 
                    return true;
                foreach (string v in verb)
                {
                    //v 開頭
                    if (line.StartsWith(v, true, new CultureInfo("en-US"))) 
                        return true;
                    //疑問詞+動詞
                    if (line.StartsWith(s + token[0] + v, true, new CultureInfo("en-US"))) 
                        return true;
                }
            }
            return false;
        }
    }
}
