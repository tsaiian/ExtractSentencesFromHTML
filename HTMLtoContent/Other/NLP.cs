using System;
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
using opennlp.tools.parser;
using opennlp.tools.cmdline.parser;

namespace HTMLtoContent
{
    class NLP
    {
        private SentenceDetectorME sentenceDetector;
        private TokenizerME tokenizer;
        private POSTaggerME tagger;
        private ChunkerME chunker;
        private Parser parser;
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

            modelInpStream = new java.io.FileInputStream("Resources\\en-parser-chunking.bin");
            ParserModel parserModel = new ParserModel(modelInpStream);
            parser = ParserFactory.create(parserModel);

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
            string lowerCase = word.ToLower();

            //過濾掉完全沒有包含英文的token
            bool ok = false;
            foreach (char c in lowerCase)
            {
                if (c >= 'a' && c <= 'z')
                {
                    ok = true;
                    break;
                }
            }

            if (ok)
                return stopwords.Contains(lowerCase);
            else
                return true;
        }

        public string[] FilterOutStopWords(string[] words)
        {
            List<string> result = new List<string>();

            foreach (string word in words)
                if (!IsStopWord(word))
                    result.Add(word);

            return result.ToArray();
        }

        public string Parser(string sentence)
        {
            if (sentence.Length <= 0)
                return "";

            string result = String.Empty;
            Parse[] topParses = ParserTool.parseLine(sentence, parser, 1);
            foreach (Parse p in topParses)
                result += getPhrase(p);

            return result;
        }

        public string[] getNP(string sentence)
        {
            if (sentence.Length <= 0)
                return null;

            temp = new List<string>();
            Parse[] topParses = ParserTool.parseLine(sentence, parser, 1);
            foreach (Parse p in topParses)
                recursiveTraversalTree2(p);

            return temp.ToArray();

        }

        public string[] getS(string sentence)
        {
            if (sentence.Length <= 0)
                return null;

            temp = new List<string>();
            Parse[] topParses = ParserTool.parseLine(sentence, parser, 1);
            foreach (Parse p in topParses)
                recursiveTraversalTree(p);

            return temp.ToArray();
        }

        private List<string> temp;
        private void recursiveTraversalTree(Parse p)
        {
            if (p.getChildCount() != 0)
            {
                if (p.getType() == "S")
                {
                    string spanContent = p.getText().Substring(p.getSpan().getStart(), p.getSpan().getEnd() - p.getSpan().getStart());
                    string[] dividedByComma = spanContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach(string t in dividedByComma)
                        temp.Add(t);
                }

                foreach (Parse l in p.getChildren())
                    recursiveTraversalTree(l);
            }
        }

        private bool containNP(Parse p)
        {
            if (p.getType() == "NP")
            {
                if (p.getChildCount() != 0)
                {
                    foreach (Parse o in p.getChildren())
                        if (o.getType() != "NN" && o.getType() != "NNP")
                            return false;

                    return true;
                }
            }

            return false;

        }

        private void recursiveTraversalTree2(Parse p)
        {
            if (p.getChildCount() != 0)
            {
                if (p.getType() == "NP")
                {
                    string spanContent = p.getText().Substring(p.getSpan().getStart(), p.getSpan().getEnd() - p.getSpan().getStart());

                    if (containNP(p))
                        temp.Add(spanContent);
                    
                    
                }

                foreach (Parse l in p.getChildren())
                    recursiveTraversalTree2(l);
            }
        }

        private string getPhrase(Parse p, int deep = 0)
        {
            string result = String.Empty;
            for (int i = 0; i < deep; i++)
                result += "  ";

            if (p.getChildCount() == 0)
            {
                string spanContent = p.getText().Substring(p.getSpan().getStart(), p.getSpan().getEnd() - p.getSpan().getStart());
                result += "(" + p.getType() + " " + spanContent + ")\n";
            }
            else
            {
                result += "(" + p.getType() + "\n";
                foreach (Parse l in p.getChildren())
                    result += getPhrase(l, deep + 1);

                for (int i = 0; i < deep; i++)
                    result += "  ";

                result += ")\n";
            }
            return result;
        }

        public bool isQuestion(string line)
        {
            //判斷句子是否為問句, by 5W1H + "?" , case sensitive
            string[] star = { "What", "Who", "Which", "When", "Where", "Why", "How" };
            string[] verb = { "is ", "was ", "were ", "are ", "do ", "does ", "did ", "can ", "could ", "will ", "would ", "should" };
            if (line.EndsWith("?"))
                return true;
            foreach (string s in star)
            {
                //For case :what's / who's
                if (line.StartsWith(s + "'", true, new CultureInfo("en-US")))
                    return true;
                else if (line.StartsWith(s + " to", true, new CultureInfo("en-US")))
                    return true;
                foreach (string v in verb)
                {
                    //v 開頭
                    if (line.StartsWith(v, true, new CultureInfo("en-US"))) 
                        return true;
                    //疑問詞+動詞
                    if (line.StartsWith(s + " " + v, true, new CultureInfo("en-US"))) 
                        return true;
                }
            }
            return false;
        }
    }
}
