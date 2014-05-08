using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTMLtoContent
{
    class LexRank
    {
        private const double d = 0.85;
        private const double linkThreshold = 0.2;
        private const double convergenceThreshold = 0.0000000001;

        static private NLP _NLP = Program.NLPmethods;
        static public void getScore(Sentence[] sentences)
        {
            int N = sentences.Length;
            Console.WriteLine(N);

            List<int> link = new List<int>();
            double[,] bMatrix = new double[N, N];

            // consine similarity
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine( i + " / " + N);
                int a = -1;
                for (int j = 0; j < N; j++)
                {
                    
                    double simTemp = cosineSimilarity(sentences[i].tokens, sentences[j].tokens);
                    bMatrix[i, j] = simTemp;
                    if (simTemp >= linkThreshold)
                        a += 1;
                }
                link.Add(a);
            }

            double[] lexRank = new double[N];
            double[] tmpRank = new double[N];

            //init
            for (int i = 0; i < N; i++)
            {
                lexRank[i] = (double)1 / (double)N;
                tmpRank[i] = 0;
            }
            
            while (!isConvergence(lexRank, tmpRank))
            {
                for (int k = 0; k < N; k++)
                    lexRank[k] = tmpRank[k];
                
                for (int i = 0; i < N; i++)
                {
                    tmpRank[i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        if (bMatrix[i, j] >= linkThreshold && i != j)
                        {
                            if (link[j] != 0)
                                tmpRank[i] += ((double)lexRank[j] / (double)link[j]);
                        }
                    }
                    tmpRank[i] = tmpRank[i] * (1 - d) + d / N;
                }

                for (int k = 0; k < N; k++)
                    Console.Write(tmpRank[k] + "\t");
                Console.WriteLine();
            }



            List<double> result = new List<double>();
            for (int i = 0; i < N; i++)
                sentences[i].lexRank = lexRank[i] * N;

            //StreamWriter sw = new StreamWriter("out.txt");
            //for (int i = 0; i < N; i++)
            //{
            //    sw.WriteLine(lexRank[i] * N + "\t" + oriSsentences[i]);
            //}
            //sw.Close();

            Console.WriteLine("end!!");
            //return result.ToArray();
        }
        static private bool isConvergence(double[] p1, double[] p2)
        {
            for(int i= 0 ; i < p1.Length ; i++)
                if (Math.Abs(p1[i] - p2[i]) > convergenceThreshold)
                    return false;

            return true;
        }

        static private double cosineSimilarity(string[] s1, string[] s2)
        {
            if (s1.Length == 0 || s2.Length == 0)
                return 0.0;

            Dictionary<string, int> s1Dict = new Dictionary<string, int>();
            Dictionary<string, int> s2Dict = new Dictionary<string, int>();
            HashSet<string> hashTokens = new HashSet<string>();

            foreach (string s in s1)
            {
                hashTokens.Add(s);
                if (s1Dict.ContainsKey(s))
                    s1Dict[s]++;
                else
                    s1Dict.Add(s, 1);
            }

            foreach (string s in s2)
            {
                hashTokens.Add(s);
                if (s2Dict.ContainsKey(s))
                    s2Dict[s]++;
                else
                    s2Dict.Add(s, 1);
            }

            int numerator = 0;
            foreach (string s in hashTokens)
                if (s1Dict.ContainsKey(s) && s2Dict.ContainsKey(s))
                    numerator += s1Dict[s] * s2Dict[s];

            int denominatorA = 0;
            foreach (KeyValuePair<string, int> kvp in s1Dict)
                denominatorA += kvp.Value * kvp.Value;

            int denominatorB = 0;
            foreach (KeyValuePair<string, int> kvp in s2Dict)
                denominatorB += kvp.Value * kvp.Value;

            double similarity = numerator / Math.Sqrt(denominatorA) / Math.Sqrt(denominatorB);

            //Console.WriteLine("!!" + numerator + " " + Math.Sqrt(denominatorA) + " " + Math.Sqrt(denominatorB) );
            //Console.WriteLine("!!" + similarity );

            return similarity;
        }
    }
}
