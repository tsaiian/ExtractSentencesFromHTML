using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLtoContent
{
    class OpenNLP
    {
        private opennlp.tools.sentdetect.SentenceDetectorME sentenceDetector;
        public OpenNLP()
        {
            //loading sentence detector model
            java.io.FileInputStream modelInpStream = new java.io.FileInputStream("model\\en-sent.bin");
            opennlp.tools.sentdetect.SentenceModel sentenceModel = new opennlp.tools.sentdetect.SentenceModel(modelInpStream);
            sentenceDetector = new opennlp.tools.sentdetect.SentenceDetectorME(sentenceModel);
        }
        public string[] sentDetect(string str)
        {
            return sentenceDetector.sentDetect(str);
        }

    }
}
