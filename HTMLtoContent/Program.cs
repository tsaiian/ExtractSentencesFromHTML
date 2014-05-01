using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace HTMLtoContent
{
    class Program
    {
        static private List<string> list = new List<string>();
        static string res = "";
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(@".\MC1-E-BSR", "*.html");
            //string[] files = Directory.GetFiles(@".\Myfiles", "*.*",
            //    SearchOption.AllDirectories);

            foreach (string file in files)
            {
                res = "";
                DirectoryInfo di = new DirectoryInfo(file);
                StreamReader sr = new StreamReader(file);

                Console.WriteLine(di.Name);
                string html = sr.ReadToEnd();
                sr.Close();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                if (doc.DocumentNode.SelectSingleNode("//body") == null)
                {
                    doc.LoadHtml("<all>" + html + "</all>");
                    ExtractText(doc.DocumentNode.SelectSingleNode("//all"));
                }
                else
                    ExtractText(doc.DocumentNode.SelectSingleNode("//body"));
                

                

                PreprocessingAndWrite(@".\Converted\" + di.Name + ".txt");

                //StreamWriter sw = new StreamWriter("out.txt");
                //sw.WriteLine(res);
                ////foreach (string s in list)
                //    //sw.WriteLine(s);
                //sw.Close();


                //暫時先處理一個html就好
                break; 
            }

            Console.WriteLine("Finish!");
            Console.ReadKey();

        }
        static private void ExtractText(HtmlAgilityPack.HtmlNode node)
        {
            if (!node.Name.Equals("script") && !node.Name.Equals("noscript") && !node.Name.Equals("style"))
            {
                HtmlAgilityPack.HtmlNodeCollection dnc = node.ChildNodes;
                foreach (HtmlAgilityPack.HtmlNode n in dnc)
                {
                    ExtractText(n);
                }


                if (node.ChildNodes.Count == 0)
                {

                    res += WebUtility.HtmlDecode(node.InnerText);
                    
                }
            }


            
        }
        static private void PreprocessingAndWrite(string outputFileName)
        {
            StreamWriter sw = new StreamWriter(outputFileName);

            string[] lines = res.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string fixedString = line.Trim(new char[] { '\t', ' ' });

                if (fixedString.Length != 0)
                    sw.WriteLine(fixedString);
                
            }
            sw.Close();
        }


    }
}
