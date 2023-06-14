namespace MonkeyLangInterpreter
{
    internal class Evaluator
    {
        public static Boolean TRUE = new() { Value = true };
        public static Boolean FALSE = new() { Value = false };
        public static Null NULL = new();

        public static IObject? Eval(AST.INode node, Environment env)
        {
            Type t = node.GetType();
            if (t == typeof(AST.Program))
            {
                AST.Program s = (AST.Program)node;
                return EvalStatements(s.Statements, env);
            }
            else if (t == typeof(AST.ExpressionStatement))
            {
                AST.ExpressionStatement s = (AST.ExpressionStatement)node;
                return Eval(s.Expression, env);
            }
            else if (t == typeof(AST.IntegerLiteral))
            {
                AST.IntegerLiteral s = (AST.IntegerLiteral)node;
                return new Integer() { Value = s.Value };
            }
            else if (t == typeof(AST.Boolean))
            {
                AST.Boolean s = (AST.Boolean)node;
                return NativeBoolToBooleanObj(s.Value);
            }
            else if (t == typeof(AST.PrefixExpression))
            {
                AST.PrefixExpression s = (AST.PrefixExpression)node;
                IObject? right = Eval(s.Right, env);
                if (IsError(right))
                {
                    return right;
                }
                return EvalPrefixExpression(s.Operator, right);
            }
            else if (t == typeof(AST.InfixExpression))
            {
                AST.InfixExpression s = (AST.InfixExpression)node;
                IObject? left = Eval(s.Left, env);
                if (IsError(left))
                {
                    return left;
                }
                IObject? right = Eval(s.Right, env);
                if (IsError(right))
                {
                    return right;
                }
                return EvalInfixExpression(s.Operator, left, right);
            }
            else if (t == typeof(AST.BlockStatement))
            {
                AST.BlockStatement s = (AST.BlockStatement)node;
                return EvalBlockStatement(s, env);
            }
            else if (t == typeof(AST.IfExpression))
            {
                AST.IfExpression s = (AST.IfExpression)node;
                return EvalIfExpression(s, env);
            }
            else if (t == typeof(AST.ReturnStatement))
            {
                AST.ReturnStatement s = (AST.ReturnStatement)node;
                IObject? val = Eval(s.ReturnValue, env);
                if (IsError(val))
                {
                    return val;
                }
                return new ReturnValue() { Value = val };
            }
            else if (t == typeof(AST.Program))
            {
                AST.Program s = (AST.Program)node;
                return EvalProgram(s, env);
            }
            else if (t == typeof(AST.LetStatement))
            {
                AST.LetStatement s = (AST.LetStatement)node;
                IObject? val = Eval(s.Value, env);
                if (IsError(val))
                {
                    return val;
                }
                _ = env.Set(s.Name.Value, val);
                return val;
            }
            else if (t == typeof(AST.Identifier))
            {
                AST.Identifier s = (AST.Identifier)node;
                return EvalIdentifier(s, env);
            }
            else if (t == typeof(AST.FunctionLiteral))
            {
                AST.FunctionLiteral s = (AST.FunctionLiteral)node;
                AST.Identifier[]? _params = s.Parameters;
                AST.BlockStatement body = s.Body;
                return new Function() { Parameters = _params, Body = body, Env = env };
            }
            else if (t == typeof(AST.CallExpression))
            {
                AST.CallExpression s = (AST.CallExpression)node;
                IObject? function = Eval(s.Function, env);
                if (IsError(function))
                {
                    return function;
                }
                IObject[]? args = EvalExpressions(s.Arguments, env);
                if (args.Length == 1 && IsError(args[0]))
                {
                    return args[0];
                }
                return ApplyFunction(function, args);
            }
            else if (t == typeof(AST.StringLiteral))
            {
                AST.StringLiteral s = (AST.StringLiteral)node;
                return new String() { Value = s.Value };
            }
            else if (t == typeof(AST.ArrayLiteral))
            {
                AST.ArrayLiteral s = (AST.ArrayLiteral)node;
                IObject[] elements = EvalExpressions(s.Elements, env);
                if (elements.Length == 1 && IsError(elements[0]))
                {
                    return elements[0];
                }
                return new Array() { Elements = elements };
            }
            else if (t == typeof(AST.IndexExpression))
            {
                AST.IndexExpression s = (AST.IndexExpression)node;
                IObject? left = Eval(s.Left, env);
                if (IsError(left))
                {
                    return left;
                }
                IObject? index = Eval(s.Index, env);
                if (IsError(index))
                {
                    return index;
                }
                return EvalIndexExpression(left, index);
            }
            else if (t == typeof(AST.HashLiteral))
            {
                AST.HashLiteral s = (AST.HashLiteral)node;
                return EvalHashLiteral(s, env);
            }
            return null;
        }

        private static IObject EvalHashLiteral(AST.HashLiteral node, Environment env)
        {
            Dictionary<HashKey, HashPair> pairs = new();
            for (int i = 0; i < node.Pairs.Count; i++)
            {
                KeyValuePair<AST.IExpression, AST.IExpression> currentPair = node.Pairs.ElementAt(i);
                IObject? key = Eval(currentPair.Key, env);
                if (IsError(key))
                {
                    return key;
                }

                IHashable hashKey;
                try
                {
                    hashKey = (IHashable)key;
                }
                catch
                {
                    return NewError("unusable as hash key: " + key.Type());
                }


                IObject? value = Eval(currentPair.Value, env);
                if (IsError(value))
                {
                    return value;
                }

                HashKey hashsed = hashKey.GetHashKey();
                pairs.Add(hashsed, new HashPair() { Key = key, Value = value });
            }
            return new Hash() { Pairs = pairs };
        }


        private static IObject EvalIndexExpression(IObject left, IObject index)
        {
            if (left.Type() == ObjectType.ARRAY_OBJ && index.Type() == ObjectType.INTEGER_OBJ)
            {
                return EvalArrayIndexExpression(left, index);
            }
            if (left.Type() == ObjectType.HASH_OBJ)
            {
                return EvalHashIndexExpression(left, index);
            }
            return NewError("index operator not supported: " + left.Type());
        }

        private static IObject EvalHashIndexExpression(IObject hash, IObject index)
        {
            Hash hashObj = (Hash)hash;
            try
            {
                IHashable key = (IHashable)index;
                bool ok = hashObj.Pairs.TryGetValue(key.GetHashKey(), out HashPair pair);
                if (ok)
                {
                    return pair.Value;
                }
                return NULL;
            }
            catch
            {
                return NewError("unusable as hash key: " + index.Type());
            }
        }

        private static IObject EvalArrayIndexExpression(IObject array, IObject index)
        {
            Array arrayObject = (Array)array;
            long idx = ((Integer)index).Value;
            long max = arrayObject.Elements.Length - 1;

            if (idx < 0 || idx > max)
            {
                return NULL;
            }
            return arrayObject.Elements[(int)idx];
        }

        private static IObject ApplyFunction(IObject fn, IObject[] args)
        {
            if (fn.GetType() == typeof(Function))
            {
                Function func = (Function)fn;
                Environment? extendedEnv = ExtendFunctionEnv(func, args);
                IObject? evaluated = Eval(func.Body, extendedEnv);
                return UnwrapReturnValue(evaluated);
            }
            if (fn.GetType() == typeof(Builtin))
            {
                Builtin func = (Builtin)fn;
                return (IObject)func.BuiltinFunction.Invoke(args);
            }



            return NewError("not a function: " + fn.Type());

        }

        private static Environment ExtendFunctionEnv(Function fn, IObject[] args)
        {
            Environment? env = Environment.NewEnclosedEnvironment(fn.Env);

            for (int paramIdx = 0; paramIdx < fn.Parameters.Length; paramIdx++)
            {
                _ = env.Set(fn.Parameters[paramIdx].Value, args[paramIdx]);
            }
            return env;
        }


        private static IObject UnwrapReturnValue(IObject obj)
        {
            if (obj.Type() == ObjectType.RETURN_VALUE_OBJ)
            {
                return ((ReturnValue)obj).Value;
            }
            return obj;
        }

        private static IObject[] EvalExpressions(AST.IExpression[] exps, Environment env)
        {
            List<IObject> result = new();
            foreach (AST.IExpression? e in exps)
            {
                IObject? evaluated = Eval(e, env);
                if (IsError(evaluated))
                {
                    return new IObject[] { evaluated };
                }
                result.Add(evaluated);
            }
            return result.ToArray();
        }

        private static IObject EvalIdentifier(AST.Identifier node, Environment env)
        {
            IObject val;
            bool ok;
            (val, ok) = env.Get(node.Value);
            if (ok)
            {
                return val;
            }

            (val, ok) = Builtins.GetBuiltin(node.Value);
            if (ok)
            {
                return val;
            }

            return NewError("identifier not found: " + node.Value);
        }

        private static IObject EvalStatements(AST.IStatement[] stmts, Environment env)
        {
            if (stmts.Length == 0)
            {
                return NULL;
            }
            IObject result = Eval(stmts[0], env);
            List<AST.IStatement>? s = stmts.ToList();
            s.RemoveAt(0);
            stmts = s.ToArray();
            if (result.Type() == ObjectType.ERROR_OBJ)
            {
                return result;
            }
            try
            {
                ReturnValue r = (ReturnValue)result;
                return r.Value;
            }
            catch
            {

            }
            foreach (AST.IStatement? statement in stmts)
            {
                result = Eval(statement, env);
                switch (result.Type())
                {
                    case ObjectType.ERROR_OBJ: return result;
                    case ObjectType.RETURN_VALUE_OBJ: return ((ReturnValue)result).Value;
                }
            }
            return result;
        }

        private static IObject EvalBlockStatement(AST.BlockStatement block, Environment env)
        {
            IObject result = Eval(block.Statements[0], env);
            List<AST.IStatement>? s = block.Statements.ToList();
            s.RemoveAt(0);
            block.Statements = s.ToArray();
            if (result != null)
            {
                ObjectType rt = result.Type();
                if (rt == ObjectType.ERROR_OBJ || rt == ObjectType.RETURN_VALUE_OBJ)
                    return result;
            }

            foreach (AST.IStatement? statement in block.Statements)
            {
                result = Eval(statement, env);

                if (result != null)
                {
                    ObjectType rt = result.Type();
                    if (rt == ObjectType.ERROR_OBJ || rt == ObjectType.RETURN_VALUE_OBJ)
                        return result;
                }

            }
            return result;
        }

        private static IObject EvalProgram(AST.Program program, Environment env)
        {
            IObject result = Eval(program.Statements[0], env);
            List<AST.IStatement>? s = program.Statements.ToList();
            s.RemoveAt(0);
            program.Statements = s.ToArray();

            Type? te = result.GetType();
            if (te == typeof(ReturnValue))
            {
                ReturnValue r = (ReturnValue)result;
                return r.Value;
            }
            else if (te == typeof(Error))
            {
                return result;
            }

            foreach (AST.IStatement? statement in program.Statements)
            {
                result = Eval(statement, env);
                Type? t = result.GetType();
                if (t == typeof(ReturnValue))
                {
                    ReturnValue r = (ReturnValue)result;
                    return r.Value;
                }
                else if (t == typeof(Error))
                {
                    return result;
                }

            }
            return result;
        }

        private static IObject EvalPrefixExpression(string op, IObject right)
        {
            switch (op)
            {
                case "!":
                    return EvalBangOperatorExpression(right);
                case "-":
                    return EvalMinusPrefixOperatorExpression(right);
                default:
                    return NewError("unknown operator: {0}{1} ", op, right.Type());
            }
        }

        private static IObject EvalInfixExpression(string op, IObject left, IObject right)
        {
            if (left.Type() == ObjectType.INTEGER_OBJ && right.Type() == ObjectType.INTEGER_OBJ)
            {
                return EvalIntegerInfixExpression(op, left, right);
            }

            if (left.Type() == ObjectType.STRING_OBJ && right.Type() == ObjectType.STRING_OBJ)
            {
                return EvalStringInfixExpression(op, left, right);
            }

            //small hack to get avoid a reference type or use of a pointer
            //sometimes I hate C#

            if (left.Type() != right.Type())
            {
                return NewError("type mismatch: {0} {1} {2}", left.Type(), op, right.Type());
            }
            bool leftVal = ((Boolean)left).Value;
            bool rightVal = ((Boolean)right).Value;
            if (op == "==")
            {
                return NativeBoolToBooleanObj(leftVal == rightVal);
            }
            if (op == "!=")
            {
                return NativeBoolToBooleanObj(leftVal != rightVal);
            }

            return NewError("unknown operator: {0} {1} {2}", left.Type(), op, right.Type()); ;
        }

        private static IObject EvalIntegerInfixExpression(string op, IObject left, IObject right)
        {
            long leftVal = ((Integer)left).Value;
            long rightVal = ((Integer)right).Value;

            switch (op)
            {
                case "+":
                    return new Integer() { Value = leftVal + rightVal };
                case "-":
                    return new Integer() { Value = leftVal - rightVal };
                case "*":
                    return new Integer() { Value = leftVal * rightVal };
                case "/":
                    return new Integer() { Value = leftVal / rightVal };
                case "<":
                    return NativeBoolToBooleanObj(leftVal < rightVal);
                case ">":
                    return NativeBoolToBooleanObj(leftVal > rightVal);
                case "==":
                    return NativeBoolToBooleanObj(leftVal == rightVal);
                case "!=":
                    return NativeBoolToBooleanObj(leftVal != rightVal);
                default:
                    return NewError("unknown operator: {0} {1} {2}", left.Type(), op, right.Type());
            }
        }

        private static IObject EvalStringInfixExpression(string op, IObject left, IObject right)
        {
            if (op != "+")
            {
                return NewError($"unknown operator: {left.Type()} {op} {right.Type()}");
            }

            string? leftVal = ((String)left).Value;
            string? rightVal = ((String)right).Value;
            return new String() { Value = leftVal + rightVal };
        }

        private static IObject EvalBangOperatorExpression(IObject right)
        {
            Type t = right.GetType();
            if (t == typeof(Boolean))
            {
                return NativeBoolToBooleanObj(!((Boolean)right).Value);
            }
            if (t == typeof(Null))
            {
                return TRUE;
            }
            return FALSE;
        }

        private static IObject EvalMinusPrefixOperatorExpression(IObject right)
        {
            if (right.Type() != ObjectType.INTEGER_OBJ)
            {
                return NewError("unknown operator: -{0}", right.Type());
            }
            long value = ((Integer)right).Value;
            return new Integer() { Value = -value };
        }

        private static Boolean NativeBoolToBooleanObj(bool input)
        {
            return input ? TRUE : FALSE;
        }

        private static IObject EvalIfExpression(AST.IfExpression ie, Environment env)
        {
            IObject condition = Eval(ie.Condition, env);
            if (IsError(condition))
            {
                return condition;
            }
            if (IsTruthy(condition))
            {
                return Eval(ie.Consequence, env);
            }
            else if (ie.Alternative != null)
            {
                return Eval(ie.Alternative, env);
            }
            return NULL;
        }

        private static bool IsTruthy(IObject obj)
        {
            if (obj.Type() == ObjectType.NULL_OBJ)
            {
                return false;
            }
            if (obj.Type() == ObjectType.BOOLEAN_OBJ)
            {
                Boolean bo = (Boolean)obj;
                return bo.Value;
            }
            return true;
        }

        public static Error NewError(string format, params object[] args)
        {
            return new Error() { Message = string.Format(format, args) };
        }

        private static bool IsError(IObject obj)
        {
            if (obj != null)
            {
                return obj.Type() == ObjectType.ERROR_OBJ;
            }
            return false;
        }
    }
}
