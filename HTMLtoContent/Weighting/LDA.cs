using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTMLtoContent
{
    class LDA
    {
        private int topicCount;
        public LDA(int n = 5)
        {
            topicCount = n;
        }

        public void training(List<string[]>tokens)
        {
            if (!Directory.Exists("LDAmodel"))
                Directory.CreateDirectory("LDAmodel");
            StreamWriter sw = new StreamWriter("LDAmodel\\trn.txt");
            sw.WriteLine(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                for (int j = 0; j < tokens[i].Length; j++)
                    sw.Write(tokens[i][j] + " ");
                sw.WriteLine();
            }
            sw.Close();

            //string cmd = "-est -alpha 25 -beta 1 -ntopics 2 -niters 1000 -dir t -dfile trn.txt";
            jgibblda.LDA.main(new string[] { "-est", "-alpha", (50 / topicCount).ToString(), "-beta", "1", "-ntopics", topicCount.ToString(), "-niters", "1000", "-dir", "LDAmodel", "-dfile", "trn.txt" });

            Console.WriteLine("[Info] LDA finish");
       }
        public void testing(string[] query, Sentence[] sentences)
        {
            /*write file*/
            StreamWriter sw = new StreamWriter("LDAmodel\\test.txt");
            sw.WriteLine(sentences.Length + 1);

            //write query
            for (int j = 0; j < query.Length; j++)
                sw.Write(query[j] + " ");
            sw.WriteLine();

            //write other sentences
            foreach(Sentence s in sentences)
            {
                foreach(string t in s.stemTokens)
                    sw.Write(t + " ");
                sw.WriteLine();
            }
            sw.Close();

            //test
            //jgibblda.LDA.main(new string[] { "-inf", "-model", "model-final", "-niters", "200", "-dir", "LDAmodel", "-dfile", "test.txt" });

            Dictionary<string, int> wordmap = new Dictionary<string, int>();
            StreamReader sr = new StreamReader("LDAmodel\\wordmap.txt");
            int count = Convert.ToInt16(sr.ReadLine());
            for(int i = 0 ; i < count ; i++)
            {
                string line = sr.ReadLine();
                string token = line.Substring(0, line.LastIndexOf(' '));
                int index = Convert.ToInt16(line.Substring(line.LastIndexOf(' ') + 1));

                wordmap.Add(token, index);
            }
            sr.Close();

            List<List<double>> phi = new List<List<double>>();
            sr = new StreamReader("LDAmodel\\model-final.phi");
            for (int i = 0; i < topicCount; i++)
            {
                string line = sr.ReadLine();
                List<double> temp = new List<double>();

                foreach (string s in line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    temp.Add(Convert.ToDouble(s));
                }

                phi.Add(temp);
            }

            //calc sim()
            List<double> sim = new List<double>();
            double sum = 0;
            for (int i = 0; i < topicCount; i++)
            {
                double score = 0;
                foreach (string q in query)
                {
                    if (wordmap.ContainsKey(q))
                    {
                        score += phi[i][wordmap[q]];
                        sum += phi[i][wordmap[q]];
                    }
                }
                sim.Add(score);
            }

            for (int i = 0; i < sim.Count; i++)
                sim[i] /= sum;

            for (int i = 0; i < sim.Count; i++)
                Console.Write(sim[i] + " ");

            foreach (Sentence sen in sentences)
            {
                double score = 0, score2 = 0;
                int containNum = 0, containNum2 = 0;
                foreach (string t in sen.stemTokens)
                {
                    if (wordmap.ContainsKey(t))
                    {
                        containNum++;
                        for (int i = 0; i < topicCount; i++)
                            score += phi[i][wordmap[t]] * sim[i];

                        if (!query.Contains(t))
                        {
                            containNum2++;
                            for (int i = 0; i < topicCount; i++)
                                score2 += phi[i][wordmap[t]] * sim[i];
                        }
                    }
                }
                sen.lda = score / (double)containNum;
                sen.lda2 = score2 / (double)containNum2;


            }

            Console.WriteLine("[Info] LDA test finish");
        }
    }
}
