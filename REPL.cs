namespace MonkeyLangInterpreter
{
    internal class REPL
    {
        private const string PROMPT = ">> ";
        public bool ContinueRunning = true;
        public void Start()
        {
            Environment env = Environment.NewEnvironment();
            Console.WriteLine("Welcome, feel free to type some commands");
            while (ContinueRunning)
            {
                Console.Write(PROMPT);
                string s = Console.ReadLine();
                if (s == "")
                {
                    continue;
                }
                if (s.ToLower() == "cls")//Simple check to allow for clearing the screen by using the command CLS
                {
                    Console.Clear();
                    continue;
                }
                Lexer lexer = new(s);
                Parser p = new(ref lexer);
                AST.Program? program = p.ParseProgram();

                if (p.errors.Count != 0)
                {
                    PrintParserErrors(p.errors.ToArray());
                    continue;
                }

                IObject? evaluated = Evaluator.Eval(program, env);
                if (evaluated != null)
                {
                    Console.WriteLine(evaluated.Inspect());
                }

            }
        }

        internal static void PrintParserErrors(string[] errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An error occured while parsing:");
            foreach (string s in errors)
                Console.WriteLine("\t" + s);

            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
