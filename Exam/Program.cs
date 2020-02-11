using System;
using System.IO;
using System.Text;
using GeminiLab.Core2.IO;
using GeminiLab.Glug;
using GeminiLab.Glug.AST;
using GeminiLab.Glug.Parser;
using GeminiLab.Glug.Tokenizer;

namespace Exam {
    public static class Program {
        public static void Main(string[] args) {
            var sb = new StringBuilder();

            string s;
            while ((s = Console.ReadLine()) != null) sb.AppendLine(s);

            var tok = new GlugTokenizer(new StringReader(sb.ToString()));
            /*
            while (true) {
                var token = tok.GetToken();
                if (token == null) break;

                Console.WriteLine(token.Type);
            } 
            return;
            */
            var root = GlugParser.Parse(tok);
            new DumpVisitor(new IndentedWriter(Console.Out)).Visit(root);

            var vcv = new VariableCheckVisitor();
            vcv.Visit(root);
        }

        public static T Default<T>() => default;
    }
}
