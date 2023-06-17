namespace MonkeyLangInterpreter
{
    internal class Program
    {
        /// <summary>
        /// Main entry-point into the program
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            bool runTests = true;

            if (args.Length > 0)
            {
                RunProgram(args[0]);
            }
            else
            { 
                if (runTests)
                {
                    Tests.RunAllTests();
                    return;
                }

                REPL MainLoop = new();
                MainLoop.Start();
            }
        }

        /// <summary>
        /// Takes any unhandeld excetion, prints it to the console and exits the application
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(((Exception)e.ExceptionObject).GetBaseException().ToString());
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Run an input program. Bypasses the need for the REPL
        /// </summary>
        /// <param name="filePath">the path to the file to run. Can be any file extension, as long 
        /// as the file contains valid code</param>
        private static void RunProgram(string filePath)
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            string input = File.ReadAllText(filePath);

            Environment env = Environment.NewEnvironment();
            Lexer lexer = new(input);
            Parser p = new(ref lexer);
            AST.Program? program = p.ParseProgram();

            if (p.errors.Count != 0)
            {
                REPL.PrintParserErrors(p.errors.ToArray());
                return;
            }

            IObject? evaluated = Evaluator.Eval(program, env);
            if (evaluated != null)
            {
                Console.WriteLine(evaluated.Inspect());
            }
        }

    }
}