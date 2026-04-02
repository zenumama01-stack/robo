    [BenchmarkCategory(Categories.Engine, Categories.Public)]
    public class Parsing
        public Ast UsingStatement()
            const string Script = @"
                using module moduleA
                using Assembly assemblyA
                using namespace System.IO";
            return Parser.ParseInput(Script, out _, out _);
