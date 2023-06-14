namespace MonkeyLangInterpreter
{
    internal class Builtins
    {
        private static Func<object[], object> len = (parameters) =>
        {
            if (parameters.Length != 1)
            {
                return Evaluator.NewError("wrong number of arguments. got: {0}, expected 1", parameters.Length);
            }
            object? arg = parameters[0];
            Type? t = arg.GetType();
            if (t == typeof(String))
            {
                return new Integer() { Value = ((String)arg).Value.Length };
            }
            if (t == typeof(Array))
            {
                return new Integer() { Value = ((Array)arg).Elements.LongLength };
            }


            return Evaluator.NewError("argument to 'len' not supported, got: " + ((IObject)parameters[0]).Type());
        };


        private static Func<object[], object> first = (args) =>
        {
            if (args.Length != 1)
            {
                return Evaluator.NewError("wrong number of arguments. got: {0}, expected: 1", args.Length);
            }
            if (((IObject)args[0]).Type() != ObjectType.ARRAY_OBJ)
            {
                return Evaluator.NewError("argument to 'first' must be an Array, got: " + ((IObject)args[0]).Type());
            }
            Array arr = (Array)args[0];
            if (arr.Elements.Length > 0)
            {
                return arr.Elements[0];
            }
            return Evaluator.NULL;
        };

        private static Func<object[], object> last = (args) =>
        {
            if (args.Length != 1)
            {
                return Evaluator.NewError("wrong number of arguments. got: {0}, expected: 1", args.Length);
            }
            if (((IObject)args[0]).Type() != ObjectType.ARRAY_OBJ)
            {
                return Evaluator.NewError("argument to 'first' must be an Array, got: " + ((IObject)args[0]).Type());
            }
            Array arr = (Array)args[0];
            int length = arr.Elements.Length;
            if (length > 0)
            {
                return arr.Elements[length - 1];
            }
            return Evaluator.NULL;
        };

        private static Func<object[], object> rest = (args) =>
        {
            if (args.Length != 1)
            {
                return Evaluator.NewError("wrong number of arguments. got: {0}, expected: 1", args.Length);
            }
            if (((IObject)args[0]).Type() != ObjectType.ARRAY_OBJ)
            {
                return Evaluator.NewError("argument to 'first' must be an Array, got: " + ((IObject)args[0]).Type());
            }
            Array arr = (Array)args[0];
            int length = arr.Elements.Length;
            if (length > 0)
            {
                List<IObject> newElements = new();
                for (int i = 1; i < length; i++)
                {
                    newElements.Add(arr.Elements[i]);
                }
                return new Array() { Elements = newElements.ToArray() };
            }
            return Evaluator.NULL;
        };


        private static Func<object[], object> push = (args) =>
        {
            if (args.Length != 2)
            {
                return Evaluator.NewError("wrong number of arguments. got: {0}, expected: 2", args.Length);
            }
            if (((IObject)args[0]).Type() != ObjectType.ARRAY_OBJ)
            {
                return Evaluator.NewError("argument to 'first' must be an Array, got: " + ((IObject)args[0]).Type());
            }
            Array arr = (Array)args[0];
            int length = arr.Elements.Length;

            List<IObject> newElements = new();
            for (int i = 0; i < length; i++)
            {
                newElements.Add(arr.Elements[i]);
            }
            newElements.Add((IObject)args[1]);
            return new Array() { Elements = newElements.ToArray() };

        };

        private static Func<object[], object> puts = (args) =>
        {
            foreach (object o in args)
            {
                IObject arg = (IObject)o;
                Console.WriteLine(arg.Inspect());
            }
            return Evaluator.NULL;
        };

        private static Dictionary<string, Builtin> BultinFuncs = new()
        {
            {"len", new Builtin(){BuiltinFunction = len} } ,
            {"first", new Builtin(){BuiltinFunction = first} } ,
            {"last", new Builtin(){BuiltinFunction = last} } ,
            {"rest", new Builtin(){BuiltinFunction = rest} } ,
            {"push", new Builtin(){BuiltinFunction = push} } ,
            {"puts", new Builtin(){BuiltinFunction = puts } },

        };


        public static (IObject, bool) GetBuiltin(string key)
        {
            if (BultinFuncs.ContainsKey(key))
            {
                return (BultinFuncs[key], true);
            }

            return (null, false);
        }
    }
}
