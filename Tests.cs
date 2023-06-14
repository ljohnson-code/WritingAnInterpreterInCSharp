namespace MonkeyLangInterpreter
{
    internal class Tests
    {
        private struct letTest
        {
            public string input;
            public string expectedIdentifier;
            public object expectedValue;
            public letTest(string i, string e, object ex)
            {
                input = i;
                expectedIdentifier = e;
                expectedValue = ex;
            }
        }
        
        private struct prefTest
        {
            public string input;
            public string op;
            public long intval;
        }

        private struct infixTest
        {
            public string input;
            public long leftValue;
            public string op;
            public long rightValue;
            public infixTest(string i, long l, string o, long r)
            {
                input = i;
                leftValue = l;
                op = o;
                rightValue = r;
            }
        }

        private struct precedenceTest
        {
            public string input;
            public string expected;
            public precedenceTest(string i, string e)
            {
                input = i;
                expected = e;
            }
        }

        private struct testLiteralExpression
        {
            public AST.IExpression exp;
            public object expected;
            public testLiteralExpression(AST.IExpression e, object o)
            {
                exp = e;
                expected = o;
            }
        }

        private struct FnLiteralTest
        {
            public string input;
            public string[] expectedParams;
        }

        private struct intEvalTest
        {
            public string input;
            public long expected;
            public intEvalTest(string s, int e)
            {
                input = s;
                expected = e;
            }
            public intEvalTest(string s, long e)
            {
                input = s;
                expected = e;
            }
        }
        
        private struct boolEvalTest
        {
            public string input;
            public bool expected;
            public boolEvalTest(string s, bool e)
            {
                input = s;
                expected = e;
            }

        }

        private struct bangOperatorTest
        {
            public string input;
            public bool expected;
            public bangOperatorTest(string s, bool b)
            {
                input = s;
                expected = b;
            }
        }

        private struct HashIndexTest
        {
            public string input;
            public object expected;
            public HashIndexTest(string s, object o)
            {
                input = s;
                expected = o;
            }
        }

        private struct ArrayIndexTest
        {
            public string input;
            public object expected;
            public ArrayIndexTest(string s, object o)
            {
                input = s;
                expected = o;
            }
        }
        
        private struct FuncTestParam
        {
            public string input;
            public long expected;
            public FuncTestParam(string i, long e)
            {
                input = i;
                expected = e;
            }
        }
        
        private struct letStmtTest
        {
            public string input;
            public long expected;
            public letStmtTest(string i, long e)
            {
                input = i;
                expected = e;
            }
        }
        
        private struct ErrorTest
        {
            public string input;
            public string expectedMessage;
            public ErrorTest(string i, string e)
            {
                input = i;
                expectedMessage = e;
            }
        }
     
        private struct ReturnStatementTest
        {
            public string input;
            public long expected;
            public ReturnStatementTest(string s, long e)
            {
                input = s;
                expected = e;
            }
        }

        private struct integerExpressionTest
        {
            private string input;
            private long expected;
            public integerExpressionTest(string s, long l)
            {
                input = s;
                expected = l;
            }
        }

        private struct IfElseTest
        {
            public string input;
            public object expected;
            public IfElseTest(string s, object o)
            {
                input = s;
                expected = o;
            }
        }

        private static void CheckParseErrors(Parser p)
        {
            if (p.errors.Count == 0)
                return;
            Console.WriteLine("Parser had {0} errors", p.errors.Count);
            foreach (string error in p.errors)
            {
                Console.WriteLine("parser error: " + error);
            }
        }
       
        private static void TestLetStatements()
        {

            letTest[] letTests = new letTest[]
            {
                new letTest("let x = 5;", "x", (long)5),
                new letTest("let y = true;", "y", true),
                new letTest("let foobar = y;", "foobar", "y")
            };

            foreach (letTest test in letTests)
            {
                Lexer l = new(test.input);
                Parser p = new(l);
                AST.Program? program = p.ParseProgram();

                CheckParseErrors(p);

                if (program == null)
                {
                    Console.WriteLine("ParseProgram() returned null");
                }
                if (program.Value.Statements.Length != 1)
                {
                    Console.WriteLine($"program.Statements does not contain 1 statements got {program.Value.Statements.Length}");
                }
                AST.IStatement? stmt = program.Value.Statements[0];
                if (!TestLetStatement(stmt, test.expectedIdentifier))
                {
                    return;
                }
                AST.IExpression? val = ((AST.LetStatement)stmt).Value;
                if (!TestLiteralExpression(val, test.expectedValue))
                {
                    return;
                }
            }

        }

        private static void TestReturnStatement()
        {
            string input = """
                return 5;
                return 10;
                return 993322;
                """;

            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();

            CheckParseErrors(p);

            if (program == null)
            {
                throw new Exception("ParseProgram() returned null");
            }
            if (program.Value.Statements.Length != 3)
            {
                throw new Exception($"program.Statements does not contain 3 statements got {program.Value.Statements.Length}");
            }
            AST.IStatement[]? vals = program.Value.Statements;

            foreach (AST.IStatement? v in vals)
            {
                if (v.TokenLiteral() != "return")
                {
                    throw new Exception("Token Literal not 'return'. got =" + v.TokenLiteral());
                }
            }
        }

        private static void TestString()
        {
            AST.Program prog = new()
            {
                Statements = new AST.IStatement[]
                {
                    new AST.LetStatement()
                    {
                        Token = new Token(){TokenType = TokenType.LET, Literal = "let"},
                        Name = new AST.Identifier()
                        {
                            Token = new Token(){TokenType = TokenType.IDENT, Literal = "myVar"},
                            Value = "myVar"
                        },
                        Value = new AST.Identifier()
                        {
                            Token = new Token(){TokenType = TokenType.IDENT, Literal= "anotherVar"},
                            Value = "anotherVar"
                        }
                    }
                }
            };
            if (!(prog.String() == "let myVar = anotherVar;"))
                throw new Exception("program.String() wrong. got= " + prog.String());
            Console.WriteLine();
        }

        private static void TestIdentifierExpression()
        {
            string input = "foobar;";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);

            if (program.Value.Statements.Length != 1)
            {
                throw new Exception("Program does not have enough statments. got: " + program.Value.Statements.Length);
            }
            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
            }
            catch
            {
                throw new Exception("program.Statements[0] is not ast.ExpressionStatment. got: " + program.Value.Statements[0].GetType());
            }
            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];

                AST.Identifier ident = (AST.Identifier)stmt.Expression;
                if (ident.Value != "foobar")
                {
                    throw new Exception("ident.value not foobar. got: " + ident.Value);
                }
                if (ident.TokenLiteral() != "foobar")
                {
                    throw new Exception("ident.TokenLiteral not foobar. got: " + ident.TokenLiteral());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("exp not * ast.Identifier. got: " + program.Value.Statements[0].GetType());
            }

        }

        private static void TestIntegerLiteralExpression()
        {
            string input = "5;";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);
            if (program.Value.Statements.Length != 1)
            {
                throw new Exception("Program does not have enough statments. got: " + program.Value.Statements.Length);
            }
            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];

            }
            catch
            {
                throw new Exception("program.Statements[0] is not ast.ExpressionStatment. got: " + program.Value.Statements[0].GetType());
            }
            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
                AST.IntegerLiteral literal = (AST.IntegerLiteral)stmt.Expression;
                if (literal.Value != 5)
                {
                    throw new Exception("Error calue is not 5");
                }
                if (literal.TokenLiteral() != "5")
                {
                    throw new Exception("literal.TokenLiteral not correct");
                }


            }
            catch
            {
                throw new Exception("" + program.Value.Statements[0].GetType());
            }

        }

        private static bool TestInfixExpression(AST.IExpression exp, object left, string op, object right)
        {
            try
            {
                AST.InfixExpression opExp = (AST.InfixExpression)exp;
                if (!TestLiteralExpression(opExp.Left, left))
                    return false;
                if (opExp.Operator != op)
                    return false;
                if (!TestLiteralExpression(opExp.Right, right))
                    return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not convert to infix expression");
                return false;
            }
            return true;

        }

        private static void TestParsingPrefixExpressions()
        {
            prefTest[] prefixTests = new prefTest[]
            {
                new prefTest(){input = "!5;",op ="!", intval = 5},
                new prefTest(){input = "-15;",op ="-", intval = 15},
            };

            foreach (prefTest test in prefixTests)
            {
                Lexer l = new(test.input);
                Parser p = new(l);
                AST.Program? program = p.ParseProgram();
                CheckParseErrors(p);
                if (program.Value.Statements.Length != 1)
                {
                    throw new Exception("not enough statements");
                }
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
                AST.PrefixExpression exp = (AST.PrefixExpression)stmt.Expression;
                if (exp.Operator != test.op)
                {
                    throw new Exception("invalid operator");
                }
                if (!TestintegerLiteral(exp.Right, test.intval))
                {
                    throw new Exception();
                }
                Console.WriteLine(stmt.String());
            }

        }

        private static void TestParsingInfixExpressions()
        {
            infixTest[] infixTests = new infixTest[]
            {

                new infixTest("5 + 5;", 5, "+", 5),
                new infixTest("5 - 5;", 5, "-", 5),
                new infixTest("5 * 5;", 5, "*", 5),
                new infixTest("5 / 5;", 5, "/", 5),
                new infixTest("5 > 5;", 5, ">", 5),
                new infixTest("5 < 5;", 5, "<", 5),
                new infixTest("5 == 5;", 5, "==", 5),
                new infixTest("5 != 5;", 5, "!=", 5)
            };

            foreach (infixTest test in infixTests)
            {
                Lexer l = new(test.input);
                Parser p = new(l);
                AST.Program? program = p.ParseProgram();
                CheckParseErrors(p);
                if (program.Value.Statements.Length != 1)
                {
                    throw new Exception("not enough statements");
                }
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
                AST.InfixExpression exp = (AST.InfixExpression)stmt.Expression;
                if (!TestintegerLiteral(exp.Left, test.leftValue))
                {
                    return;
                }
                if (!TestintegerLiteral(exp.Right, test.rightValue))
                {
                    return;
                }
                if (exp.Operator != test.op)
                {
                    return;
                }


            }
        }

        private static void TestOperatorPrecendenceParsing()
        {
            precedenceTest[] tests = new precedenceTest[]
            {
                new precedenceTest(
                "-a * b",
                "((-a) * b)"
                ),
                new precedenceTest(
                "!-a",
                "(!(-a))"
                ),
                new precedenceTest(
                "a + b + c",
                "((a + b) + c)"
                ),
                new precedenceTest(
                "a + b - c",
                "((a + b) - c)"
                ),
                new precedenceTest(
                "a * b * c",
                "((a * b) * c)"
                ),
                new precedenceTest(
                "a * b / c",
                "((a * b) / c)"
                ),
                new precedenceTest(
                "a + b / c",
                "(a + (b / c))"
                ),
                new precedenceTest(
                "a + b * c + d / e - f",
                "(((a + (b * c)) + (d / e)) - f)"
                ),
                new precedenceTest(
                "3 + 4; -5 * 5",
                "(3 + 4)((-5) * 5)"
                ),
                new precedenceTest(
                "5 > 4 == 3 < 4",
                "((5 > 4) == (3 < 4))"
                ),
                new precedenceTest(
                "5 < 4 != 3 > 4",
                "((5 < 4) != (3 > 4))"
                ),
                new precedenceTest(
                "3 + 4 * 5 == 3 * 1 + 4 * 5",
                "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"
                ),
                new precedenceTest("true","true"),
                new precedenceTest("false","false"),
                new precedenceTest("3 > 5 == false","((3 > 5) == false)"),
                new precedenceTest("3 < 5 == true","((3 < 5) == true)"),
                new precedenceTest("1 + (2 + 3) + 4","((1 + (2 + 3)) + 4)"),
                new precedenceTest("(5 + 5) * 2","((5 + 5) * 2)"),
                new precedenceTest("2 / (5 + 5)","(2 / (5 + 5))"),
                new precedenceTest("-(5 + 5)","(-(5 + 5))"),
                new precedenceTest("!(true == true)","(!(true == true))"),
                new precedenceTest("a + add(b * c) + d","((a + add((b * c))) + d)"),
                new precedenceTest("add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))","add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))"),
                new precedenceTest("add(a + b + c * d / f + g)","add((((a + b) + ((c * d) / f)) + g))"),
                new precedenceTest("a * [1, 2, 3, 4][b * c] * d","((a * ([1, 2, 3, 4][(b * c)])) * d)"),
                new precedenceTest("add(a * b[2], b[1], 2 * [1, 2][1])","add((a * (b[2])), (b[1]), (2 * ([1, 2][1])))"),


            };
            foreach (precedenceTest test in tests)
            {
                Lexer l = new(test.input);
                Parser p = new(l);
                AST.Program? program = p.ParseProgram();
                CheckParseErrors(p);
                string actual = program.Value.String();
                if (actual != test.expected)
                {
                    Console.WriteLine("got: " + actual + " expected: " + test.expected);
                }
                else
                {
                    Console.WriteLine("Sucess");
                }
            }
        }


        private static bool TestBooleanLiteral(AST.IExpression exp, bool value)
        {
            try
            {
                AST.Boolean bo = (AST.Boolean)exp;
                if (bo.Value != value)
                {
                    Console.WriteLine($"Error value not {value}. got {bo.Value} instead");
                    return false;
                }
                if (bo.TokenLiteral() != value.ToString().ToLower())
                {
                    Console.WriteLine($"Error bo.TokenLiteral not {value.ToString().ToLower()}: got {bo.TokenLiteral()} instead");
                    return false;
                }
            }
            catch (Exception)
            { 
                return false;
            }
            return true;
        }

        private static bool TestintegerLiteral(AST.IExpression il, long value)
        {
            AST.IntegerLiteral integ = (AST.IntegerLiteral)il;
            if (integ.Value != value)
            {
                return false;
            }

            if (integ.TokenLiteral() != $"{value}")
            {
                return false;
            }
            return true;
        }

        private static bool TestIdentifier(AST.IExpression exp, string value)
        {
            try
            {
                AST.Identifier ident = (AST.Identifier)exp;
                if (ident.Value != value)
                {
                    Console.WriteLine($"ident.Value not {value}. got {ident.Value} instead");
                    return false;
                }
                if (ident.TokenLiteral() != value)
                {
                    Console.WriteLine($"ident.TokenLiteral not {value}. got {ident.TokenLiteral} instead");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exp not ast.Identifier. got T=" + exp.GetType().ToString());
                return false;
            }
            return true;
        }

        private static bool TestLetStatement(AST.IStatement s, string name)
        {
            if (s.TokenLiteral() != "let")
            {
                Console.WriteLine("s.TokenLiteral not let. got: " + s.TokenLiteral());
                return false;
            }


            AST.LetStatement letStmt = (AST.LetStatement)s;
            if (letStmt.Name.Value != name)
            {
                Console.WriteLine($"Error name.value is not correct. Got{letStmt.Name.Value} Expected: {name} ");
                return false;
            }

            if (letStmt.Name.TokenLiteral() != name)
            {
                Console.WriteLine($"Error TokenLiteral not correct. Got: {letStmt.Name.TokenLiteral()} Expected: {name}");
                return false;
            }
            return true;
        }
       
        private static bool TestLiteralExpression(AST.IExpression exp, object expected)
        {
            Type v = expected.GetType();
            if (v == typeof(int))
                return TestintegerLiteral(exp, (long)expected);
            else if (v == typeof(long))
                return TestintegerLiteral(exp, (long)expected);
            else if (v == typeof(string))
                return TestIdentifier(exp, (string)expected);
            else if (v == typeof(bool))
                return TestBooleanLiteral(exp, (bool)expected);

            return true;
        }

        private static void TestIfExpression()
        {

            //string input = "if (x < y) { x }";
            string input = "if (x < y) { x } else { y }";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);
            if (program.Value.Statements.Length != 1)
            {
                throw new Exception("Program does not have enough statments. got: " + program.Value.Statements.Length);
            }
            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
            }
            catch
            {
                throw new Exception("program.Statements[0] is not ast.IfExpression. got: " + program.Value.Statements[0].GetType());
            }
            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
                AST.IfExpression exp = (AST.IfExpression)stmt.Expression;
                if (!TestInfixExpression(exp.Condition, "x", "<", "y"))
                    return;
                if (exp.Consequence.Value.Statements.Length != 1)
                {
                    Console.WriteLine("consequence is not 1 statement. got " + exp.Consequence.Value.Statements.Length + " instead");
                    return;
                }

                AST.ExpressionStatement consequence = (AST.ExpressionStatement)exp.Consequence.Value.Statements[0];
                if (!TestIdentifier(consequence.Expression, "x"))
                {
                    return;
                }
                if (exp.Alternative == null)
                {
                    Console.WriteLine("exp.Alternative.statements was not null got: " + exp.Alternative);
                }
            }
            catch
            {
                throw new Exception("exp not * ast.Identifier. got: " + program.Value.Statements[0].GetType());
            }
        }

       
        private static void TestFunctionLiteralParsing()
        {

            FnLiteralTest[] fnLiteralTests = new FnLiteralTest[]
            {
                new FnLiteralTest(){input ="fn() {};", expectedParams= new string[]{ } },
                new FnLiteralTest(){input = "fn(x) {};", expectedParams= new string[]{ "x" } },
                new FnLiteralTest(){input ="fn(x, y, z) {};", expectedParams= new string[]{ "x", "y", "z" } }
            };

            foreach (FnLiteralTest test in fnLiteralTests)
            {
                Lexer l = new(test.input);
                Parser p = new(l);
                AST.Program? program = p.ParseProgram();
                CheckParseErrors(p);
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
                AST.FunctionLiteral function = (AST.FunctionLiteral)stmt.Expression;

                if (function.Parameters.Length != test.expectedParams.Length)
                {
                    Console.WriteLine($"Error parameter lengths wrong Expected: {test.expectedParams.Length}\tGot: {function.Parameters.Length}");
                }

                for (int i = 0; i < test.expectedParams.Length; i++)
                {
                    _ = TestLiteralExpression(function.Parameters[i], test.expectedParams[i]);
                }

            }
        }

        private static void TestCallExpressionParsing()
        {
            string input = "add(1, 2 * 3, 4 + 5);";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);


            if (program.Value.Statements.Length != 1)
            {
                Console.WriteLine($"Error program statements does not contain the proper number of statements. Got: {program.Value.Statements.Length} Expected 1");
            }

            AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
            AST.CallExpression exp = (AST.CallExpression)stmt.Expression;
            if (!TestIdentifier(exp.Function, "add"))
            {
                Console.WriteLine("error with test identifier");
                return;
            }

            if (exp.Arguments.Length != 3)
            {
                Console.WriteLine($"Error: wrong number of arguments. Got: {exp.Arguments.Length} Expected {3}");
            }
            _ = TestLiteralExpression(exp.Arguments[0], (long)1);
            _ = TestInfixExpression(exp.Arguments[1], (long)2, "*", (long)3);
            _ = TestInfixExpression(exp.Arguments[2], (long)4, "+", (long)5);

        }

        private static void TestEvalIntegerExpression()
        {
            intEvalTest[] tests = new intEvalTest[]
            {
                new intEvalTest("5", 5),
                new intEvalTest("10", 10),
                new intEvalTest("-5", -5),
                new intEvalTest("-10", -10),
                new intEvalTest("5 + 5 + 5 + 5 - 10", 10),
                new intEvalTest("2 * 2 * 2 * 2 * 2", 32),
                new intEvalTest("-50 + 100 + -50", 0),
                new intEvalTest("5 * 2 + 10", 20),
                new intEvalTest("5 + 2 * 10", 25),
                new intEvalTest("20 + 2 * -10", 0),
                new intEvalTest("50 / 2 * 2 + 10", 60),
                new intEvalTest("2 * (5 + 10)", 30),
                new intEvalTest("3 * 3 * 3 + 10", 37),
                new intEvalTest("3 * (3 * 3) + 10", 37),
                new intEvalTest("(5 + 10 * 2 + 15 / 3) * 2 + -10", 50)
            };
            foreach (intEvalTest test in tests)
            {
                IObject? evaluated = TestEval(test.input);
                if (testIntegerObject(evaluated, test.expected))
                    Console.WriteLine("Sucess!");
            }
        }

        private static IObject TestEval(string input)
        {
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            Environment env = Environment.NewEnvironment();

            return Evaluator.Eval(program.Value, env);
        }

        private static bool testIntegerObject(IObject obj, long expected)
        {
            Integer result = (Integer)obj;
            if (result.Value != expected)
            {
                Console.WriteLine("Error object has wrong value. Got " + result.Value + " Expected: " + expected);
                return false;
            }
            return true;
        }

        private static void TestEvalBooleanExpression()
        {
            boolEvalTest[] tests = new boolEvalTest[]
            {
                new boolEvalTest("true", true),
                new boolEvalTest("false", false),
                new boolEvalTest("1 < 2", true),
                new boolEvalTest("1 > 2", false),
                new boolEvalTest("1 < 1", false),
                new boolEvalTest("1 > 1", false),
                new boolEvalTest("1 == 1", true),
                new boolEvalTest("1 != 1", false),
                new boolEvalTest("1 == 2 ", false),
                new boolEvalTest("1 != 2 ", true),
                new boolEvalTest("true == true", true),
                new boolEvalTest("false == false", true),
                new boolEvalTest("true == false", false),
                new boolEvalTest("true != false", true),
                new boolEvalTest("false != true", true),
                new boolEvalTest("(1 < 2) == true", true),
                new boolEvalTest("(1 < 2) == false", false),
                new boolEvalTest("(1 > 2) == true", false),
                new boolEvalTest("(1 > 2) == false", true)
            };
            foreach (boolEvalTest test in tests)
            {
                IObject evaluated = TestEval(test.input);
                if (TestBooleanObject(evaluated, test.expected))
                    Console.WriteLine("Sucess!");
                else
                    Console.Write(": " + test.input + "\n");
            }
        }

        private static bool TestBooleanObject(IObject obj, bool expected)
        {
            try
            {
                Boolean result = (Boolean)obj;
                if (result.Value != expected)
                {
                    Console.WriteLine("Error object has wrong value. Expected: " + expected);
                    return false;

                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error result is not bool type is " + obj.GetType());
                return false;
            }
        }

        private static void TestBangOperator()
        {
            bangOperatorTest[] tests = new bangOperatorTest[]
            {
                new bangOperatorTest("true", true),
                new bangOperatorTest("false", false),
                new bangOperatorTest("1 < 2", true),
                new bangOperatorTest("1 > 2", false),
                new bangOperatorTest("1 < 1", false),
                new bangOperatorTest("1 > 1", false),
                new bangOperatorTest("1 == 1", true),
                new bangOperatorTest("1 != 1", false),
                new bangOperatorTest("1 == 2 ", false),
                new bangOperatorTest("1 != 2 ", true)
            };

            foreach (bangOperatorTest test in tests)
            {
                IObject? evaluated = TestEval(test.input);
                if (TestBooleanObject(evaluated, test.expected))
                    Console.WriteLine("Sucess!");
            }
        }

        private static void TestIfElseExpression()
        {
            IfElseTest[] tests = new IfElseTest[]
            {
                new IfElseTest("if (true) { 10 }", 10),
                new IfElseTest("if (false) { 10 }", null),
                new IfElseTest("if (1) { 10 }", 10),
                new IfElseTest("if (1 < 2) { 10 }", 10),
                new IfElseTest("if (1 > 2) { 10 }", null),
                new IfElseTest("if (1 > 2) { 10 } else { 20 }", 20),
                new IfElseTest("if (1 < 2) { 10 } else { 20 }", 10)
            };

            foreach (IfElseTest test in tests)
            {
                IObject? evaluated = TestEval(test.input);
                int integer;
                try
                {
                    integer = (int)test.expected;
                    _ = testIntegerObject(evaluated, integer);
                }
                catch
                {
                    _ = TestNullObject(evaluated);
                }
            }

        }
       
        private static bool TestNullObject(IObject obj)
        {
            if (obj.Type() != ObjectType.NULL_OBJ)
            {
                Console.WriteLine("Error: object is not null. got " + obj.GetType());
                return false;
            }
            return true;
        }

        private static void TestReturnStatements()
        {
            ReturnStatementTest[] tests = new ReturnStatementTest[]
            {
                //new ReturnStatementTest("return 10;", 10),
                new ReturnStatementTest("return 10; 9;", 10),
                new ReturnStatementTest("return 2 * 5; 9;", 10),
                new ReturnStatementTest("9; return 2 * 5; 9;", 10),
                new ReturnStatementTest("""
                                        if (10 > 1)
                                                    {
                                                        if (10 > 1)
                                                        {
                                                            return 10;
                                                        }
                                                        return 1;
                                                    }
                                        """, 10)
            };

            foreach (ReturnStatementTest test in tests)
            {
                try
                {
                    IObject? evaluated = TestEval(test.input);
                    _ = testIntegerObject(evaluated, test.expected);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
        }


        private static void TestErrorHandling()
        {
            ErrorTest[] tests = new ErrorTest[]
            {
                new ErrorTest(
                "5 + true;",
                "type mismatch: INTEGER_OBJ + BOOLEAN_OBJ"
                ),
                new ErrorTest(
                "5 + true; 5;",
                "type mismatch: INTEGER_OBJ + BOOLEAN_OBJ"
                ),
                new ErrorTest(
                "-true",
                "unknown operator: -BOOLEAN_OBJ"
                ),
                new ErrorTest(
                "true + false;",
                "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"
                ),
                new ErrorTest(
                "5; true + false; 5",
                "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"
                ),
                new ErrorTest(
                "if (10 > 1) { true + false; }",
                "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"
                ),
                new ErrorTest(
                "foobar",
                "identifier not found: foobar"
                ),
                new ErrorTest(
                    """{"name": "Monkey"}[fn(x) { x }];""",
                    "unusable as hash key: FUNCTION_OBJ"
                    )

            };


            foreach (ErrorTest test in tests)
            {
                IObject? evaluated = TestEval(test.input);
                try
                {
                    Error errObj = (Error)evaluated;
                    if (errObj.Message != test.expectedMessage)
                    {
                        Console.WriteLine("Error, wrong message. got: " + errObj.Message + " expected: " + test.expectedMessage);
                    }
                }
                catch
                {
                    Console.WriteLine("Error no error object returned. got: " + evaluated.Type());
                    Console.WriteLine(test.input);
                }
            }
        }


        private static void TestLetStatementsNew()
        {
            letStmtTest[] tests = new letStmtTest[]
            {
                new letStmtTest("let a = 5; a;", 5),
                new letStmtTest("let a = 5 * 5; a;", 25),
                new letStmtTest("let a = 5; let b = a; b;", 5),
                new letStmtTest("let a = 5; let b = a; let c = a + b + 5; c;", 15),
            };

            foreach (letStmtTest test in tests)
            {
                _ = testIntegerObject(TestEval(test.input), test.expected);
            }
        }

        private static void TestFunctionObject()
        {
            string input = "fn(x) {x +2;};";
            IObject? evaluated = TestEval(input);
            if (evaluated.GetType() != typeof(Function))
            {
                Console.WriteLine("Object not function. got: " + evaluated.GetType().ToString());
                return;
            }
            Function fn = (Function)evaluated;
            if (fn.Parameters.Length != 1)
            {
                Console.WriteLine("Error wrong number of parameters. got: " + fn.Parameters.Length);
                return;
            }

            if (fn.Parameters[0].String() != "x")
            {
                Console.WriteLine("Error parameter is not x. got: " + fn.Parameters[0].String());
                return;
            }

            string expectedBody = "(x + 2)";
            if (fn.Body.String() != expectedBody)
            {
                Console.WriteLine("Error body is not correct. got: " + fn.Body.String());
                return;
            }
        }

 
        private static void TestFunctionApplication()
        {
            FuncTestParam[] tests = new FuncTestParam[]
            {
                new FuncTestParam("let identity = fn(x) { x; }; identity(5);", 5),
                new FuncTestParam("let identity = fn(x) { return x; }; identity(5);", 5),
                new FuncTestParam("let double = fn(x) { x * 2; }; double(5);", 10),
                new FuncTestParam("let add = fn(x, y) { x + y; }; add(5, 5);", 10),
                new FuncTestParam("let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", 20),
                new FuncTestParam("fn(x) { x; }(5)", 5)
            };

            foreach (FuncTestParam test in tests)
            {
                _ = testIntegerObject(TestEval(test.input), test.expected);
            }
        }

        private static void TestParsingArray()
        {
            string input = "[1, 2 * 2, 3 + 3]";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);

            try
            {
                AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];
                try
                {
                    AST.ArrayLiteral array = (AST.ArrayLiteral)stmt.Expression;
                    if (array.Elements.Length != 3)
                    {
                        Console.WriteLine("Error: array.Element.Length is not 3 got: " + array.Elements.Length.ToString());
                    }
                    _ = TestintegerLiteral(array.Elements[0], 1);
                    _ = TestInfixExpression(array.Elements[1],(long) 2, "*", (long)2);
                    _ = TestInfixExpression(array.Elements[1], (long)3, "+", (long)3);
                }
                catch
                {
                    Console.WriteLine("exp not ArrayLiteral. got " + stmt.Expression.GetType());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void TestArrayLiteral()
        {
            string input = "[1, 2 * 2, 3 + 3]";

            IObject? evaluated = TestEval(input);
            if (evaluated.Type() == ObjectType.ARRAY_OBJ)
            {
                Array result = (Array)evaluated;
                if (result.Elements.Length == 3)
                {
                    _ = testIntegerObject(result.Elements[0], 1);
                    _ = testIntegerObject(result.Elements[1], 4);
                    _ = testIntegerObject(result.Elements[2], 6);
                }
                else
                {
                    Console.WriteLine("error array has wrong number of elements. got:  " + result.Elements.Length);
                }
            }
            else
            {
                Console.WriteLine("Error object is not an array. got: " + evaluated);
            }
        }

        private static void TestArrayindexExpressions()
        {
            ArrayIndexTest[] tests = new ArrayIndexTest[] {
            new ArrayIndexTest("[1, 2, 3][0]",(long)1),
                    new ArrayIndexTest("[1, 2, 3][1]",(long) 2),
                    new ArrayIndexTest("[1, 2, 3][2]",(long)3),
                    new ArrayIndexTest("let i = 0; [1][i];",(long)1),
                    new ArrayIndexTest("[1, 2, 3][1 + 1];",(long)3),
                    new ArrayIndexTest("let myArray = [1, 2, 3]; myArray[2];",(long)3),
                    new ArrayIndexTest("let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];",(long)6),
                    new ArrayIndexTest("let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]",(long)2),
                    new ArrayIndexTest("[1, 2, 3][3]",null),
                    new ArrayIndexTest("[1, 2, 3][-1]",null)
            };

            foreach (ArrayIndexTest test in tests)
            {
                IObject? evaluated = TestEval(test.input);
                try
                {
                    long integer = (long)test.expected;
                    _ = testIntegerObject(evaluated, integer);
                }
                catch (Exception)
                {
                    _ = TestNullObject(evaluated);
                }
            }
        }

        private static void TestParshingHashLitealsStringKeys()
        {
            string input = """{"one": 1, "two": 2, "three": 3}""";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);

            AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];

            AST.HashLiteral hash = (AST.HashLiteral)stmt.Expression;
            if (hash.Pairs.Count() != 3)
            {
                Console.WriteLine("erorr hash contains improper number of values");
            }

            Dictionary<string, long> expected = new()
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 },
            };
            foreach (KeyValuePair<AST.IExpression, AST.IExpression> pair in hash.Pairs)
            {
                AST.StringLiteral literal = (AST.StringLiteral)pair.Key;
                long exp;
                _ = expected.TryGetValue(literal.String(), out exp);
                _ = TestintegerLiteral(pair.Value, exp);
            }
            TestParsingEmptyHashLiteral();
            TestParsingHashLiteralsWithExpressions();
        }

        private static void TestParsingEmptyHashLiteral()
        {
            string input = "{}";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);

            AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];

            AST.HashLiteral hash = (AST.HashLiteral)stmt.Expression;
            if (hash.Pairs.Count() != 0)
            {
                Console.WriteLine("erorr hash contains improper number of values");
            }
        }

        private static void TestParsingHashLiteralsWithExpressions()
        {
            string input = """{"one": 0 + 1, "two": 10 - 8, "three": 15 / 5}""";
            Lexer l = new(input);
            Parser p = new(l);
            AST.Program? program = p.ParseProgram();
            CheckParseErrors(p);

            AST.ExpressionStatement stmt = (AST.ExpressionStatement)program.Value.Statements[0];

            AST.HashLiteral hash = (AST.HashLiteral)stmt.Expression;
            if (hash.Pairs.Count() != 3)
            {
                Console.WriteLine("erorr hash contains improper number of values");
            }
            _ = TestInfixExpression(hash.Pairs.ElementAt(0).Value, (long)0, "+", (long)1);
            _ = TestInfixExpression(hash.Pairs.ElementAt(1).Value, (long)10, "-", (long)8);
            _ = TestInfixExpression(hash.Pairs.ElementAt(2).Value, (long)15, "/", (long)5);

        }

        private static void TestStringHashKey()
        {
            String hello1 = new() { Value = "Hello World" };
            String hello2 = new() { Value = "Hello World" };
            String diff1 = new() { Value = "My name is johnny" };
            String diff2 = new() { Value = "My name is johnny" };

            if (hello1.GetHashKey().Value != hello2.GetHashKey().Value)
            {
                Console.WriteLine("string with same content have different hash keys");
            }
            if (diff1.GetHashKey().Value != diff2.GetHashKey().Value)
            {
                Console.WriteLine("string with same content have different hash keys");
            }
            if (hello1.GetHashKey().Value == diff1.GetHashKey().Value)
            {
                Console.WriteLine("string with differnet content have the same hash key");
            }
        }

        private static void TestHashLiterals()
        {
            string input = """let two = "two";{ "one": 10 - 9, two: 1 + 1,"thr" + "ee": 6 / 2, 4: 4,true: 5, false: 6 }""";

            IObject? evaluated = TestEval(input);
            try
            {
                Hash result = (Hash)evaluated;

                Dictionary<HashKey, long> expected = new()
                {
                    { new String() { Value = "one" }.GetHashKey(), 1 },
                    { new String() { Value = "two" }.GetHashKey(), 2 },
                    { new String() { Value = "three" }.GetHashKey(), 3 },
                    { new Integer() { Value = 4 }.GetHashKey(), 4 },
                    { Evaluator.TRUE.GetHashKey(), 5 },
                    { Evaluator.FALSE.GetHashKey(), 6 }
                };
                for (int i = 0; i < expected.Count; i++)
                {
                    KeyValuePair<HashKey, long> currentExpected = expected.ElementAt(i);
                    bool ok = result.Pairs.TryGetValue(currentExpected.Key, out HashPair pair);
                    if (!ok)
                    {
                        Console.WriteLine("No Pair for given key in Pairs");
                        return;
                    }
                    _ = testIntegerObject(pair.Value, currentExpected.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void TestHashIndexExpressions()
        {
            HashIndexTest[] tests = new HashIndexTest[] {
                    new HashIndexTest("""{"foo": 5}["foo"]""",(long)5),
                    new HashIndexTest("""{"foo": 5}["bar"]""",null),
                    new HashIndexTest("""let key = "foo"; {"foo": 5}[key]""",(long)5),
                    new HashIndexTest("""{}["foo"]""",null),
                    new HashIndexTest("""{5: 5}[5]""",(long)5),
                    new HashIndexTest("""{true: 5}[true]""",(long)5),
                    new HashIndexTest("""{false: 5}[false]""",(long)5),
            };

            foreach (HashIndexTest test in tests)
            {
                IObject? evaluated = TestEval(test.input);
                try
                {
                    long integer = (long)test.expected;
                    _ = testIntegerObject(evaluated, integer);
                }
                catch (Exception)
                {
                    _ = TestNullObject(evaluated);
                }
            }
        }
        
        /// <summary>
        /// Runs all test functions
        /// </summary>
        internal static void RunAllTests()
        {
            TestHashIndexExpressions();
            TestHashLiterals();
            TestStringHashKey();
            TestParsingHashLiteralsWithExpressions();
            TestParsingEmptyHashLiteral();
            TestParshingHashLitealsStringKeys();
            TestArrayindexExpressions();
            TestArrayLiteral();
            TestParsingArray();
            TestFunctionApplication();
            TestFunctionObject();
            TestLetStatementsNew();
            TestErrorHandling();
            TestReturnStatements();
            TestIfElseExpression();
            TestBangOperator();
            TestEvalBooleanExpression();
            TestEvalIntegerExpression();
            TestCallExpressionParsing();
            TestFunctionLiteralParsing();
            TestIfExpression();
            TestOperatorPrecendenceParsing();
            TestParsingInfixExpressions();
            TestParsingPrefixExpressions();
            TestIntegerLiteralExpression();
            TestIdentifierExpression();
            TestString();
            TestReturnStatement();
            TestLetStatements();
        }
    }
}
