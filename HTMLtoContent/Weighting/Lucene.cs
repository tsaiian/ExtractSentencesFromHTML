using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.apache.lucene.analysis.en;
using org.apache.lucene.index;
using org.apache.lucene.store;
using org.apache.lucene.document;
using org.apache.lucene.search;
using org.apache.lucene.queryParser;

namespace HTMLtoContent
{
    class Lucene
    {
        static public void indexing(Sentence[] sentences, string indexPath = "luceneIndex")
        {
            if (System.IO.Directory.Exists(indexPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(indexPath);
                di.Delete(true);
            }

            java.io.File indexDir = new java.io.File(indexPath);
            EnglishAnalyzer luceneAnalyzer = new EnglishAnalyzer(org.apache.lucene.util.Version.LUCENE_35);
            IndexWriterConfig config = new IndexWriterConfig(org.apache.lucene.util.Version.LUCENE_35, luceneAnalyzer);
            FSDirectory indexFSDir = new SimpleFSDirectory(indexDir);
            IndexWriter writer = new IndexWriter(indexFSDir, config);

            foreach (Sentence s in sentences)
            {
                Document doc = new Document();
                doc.add(new Field("text", s.sentnece, Field.Store.YES, Field.Index.ANALYZED));
                writer.addDocument(doc);
            }
            writer.close();
        }

        static public void query(Sentence[] sentences, string query, string indexPath = "luceneIndex")
        {
            Dictionary<string, Sentence> map = new Dictionary<string, Sentence>();
            foreach (Sentence s in sentences)
            {
                if (!map.ContainsKey(s.sentnece))
                    map.Add(s.sentnece, s);
            }

            java.io.File indexDir = new java.io.File(indexPath);
            FSDirectory indexFSDir = new SimpleFSDirectory(indexDir);
            IndexSearcher searcher = new IndexSearcher(IndexReader.open(indexFSDir));
            EnglishAnalyzer luceneAnalyzer = new EnglishAnalyzer(org.apache.lucene.util.Version.LUCENE_35);
            QueryParser qp = new QueryParser(org.apache.lucene.util.Version.LUCENE_35, "text", luceneAnalyzer);

            Query q = qp.parse(query);
            TopDocs tdocs = searcher.search(q, 99999999);
            ScoreDoc[] sdocs = tdocs.scoreDocs;
            for (int i = 0; i < sdocs.Length; i++)
            {
                ScoreDoc sd = sdocs[i];
                Document res = searcher.doc(sd.doc);

                string docText = res.get("text");
                float score = sd.score;

                map[docText].lucene = score;
            }
            searcher.close();

        }


    }
}
