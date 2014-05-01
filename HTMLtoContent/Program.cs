using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace HTMLtoContent
{
    class Program
    {
        static private List<string> list = new List<string>();
        static private NLP NLPmethods = new NLP();
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(@".\MC1-E-BSR", "*.html");

            int k = 0;
            foreach (string file in files)
            {
                DirectoryInfo di = new DirectoryInfo(file);
                StreamReader sr = new StreamReader(file);

                Console.WriteLine(di.Name);
                string html = sr.ReadToEnd();
                sr.Close();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                HtmlNode.ElementsFlags.Remove("form");
                doc.LoadHtml(html);
                
                string strResult = "";
                if (doc.DocumentNode.SelectSingleNode("//body") != null)
                    strResult = ExtractText(doc.DocumentNode.SelectSingleNode("//body"));
                else
                    strResult = "";
                

                PreprocessingAndWrite(@".\Converted\" + di.Name + ".txt", strResult);

                //暫時先處理一個html就好
                //k++;
                //if(k == 2)
                    //break; 
            }

            Console.WriteLine("Finish!");
            Console.ReadKey();


        }
        static private string ExtractText(HtmlAgilityPack.HtmlNode node)
        {
            if (!node.Name.Equals("script") && !node.Name.Equals("noscript") && !node.Name.Equals("style") && !node.Name.Equals("#comment"))
            {
                if (node.ChildNodes.Count == 0)
                {
                    return WebUtility.HtmlDecode(node.InnerText);
                }
                else
                {
                    string result = "";
                    HtmlAgilityPack.HtmlNodeCollection dnc = node.ChildNodes;
                    foreach (HtmlAgilityPack.HtmlNode n in dnc)
                    {
                        result += ExtractText(n);
                    }
                    return result;
                }
            }
            return String.Empty;
        }
        static private void PreprocessingAndWrite(string outputFileName, string str)
        {
            StreamWriter sw = new StreamWriter(outputFileName);

            string[] lines = str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] sentences = NLPmethods.sentDetect(line);
                foreach (string s in sentences)
                {
                    string afterTrim = s.Trim(new char[] { '\t', ' ' });
                    if (afterTrim.Length != 0)
                        sw.WriteLine(afterTrim);
                }


                
            }
            sw.Close();
        }


    }
}
