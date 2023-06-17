using System.Security.Cryptography;
using System.Text;

namespace MonkeyLangInterpreter
{
    public enum ObjectType
    {
        INTEGER_OBJ,
        BOOLEAN_OBJ,
        NULL_OBJ,
        RETURN_VALUE_OBJ,
        ERROR_OBJ,
        FUNCTION_OBJ,
        STRING_OBJ,
        BUILTIN_OBJ,
        ARRAY_OBJ,
        HASH_OBJ
    }

    internal interface IObject
    {
        public ObjectType Type();
        public string Inspect();
    }

    internal interface IHashable
    {
        public HashKey GetHashKey();
    }

    internal struct Integer : IObject, IHashable
    {
        public long Value;
        public string Inspect()
        {
            return Value.ToString();
        }

        public ObjectType Type()
        {
            return ObjectType.INTEGER_OBJ;
        }

        public HashKey GetHashKey()
        {
            HashKey h = new()
            {
                Type = Type(),
                Value = Value
            };
            return h;
        }
    }

    internal struct Boolean : IObject, IHashable
    {
        public bool Value;
        public string Inspect()
        {
            if (Value)
                return "true";
            return "false";
        }

        public ObjectType Type()
        {
            return ObjectType.BOOLEAN_OBJ;
        }
        public HashKey GetHashKey()
        {
            HashKey key = new();
            if (Value)
            {
                key.Value = 1;
            }
            else
            {
                key.Value = 0;
            }
            key.Type = Type();
            return key;
        }
    }

    internal struct Null : IObject
    {
        public string Inspect()
        {
            return "null";
        }

        public ObjectType Type()
        {
            return ObjectType.NULL_OBJ;
        }
    }

    internal struct ReturnValue : IObject
    {
        public IObject Value;

        public string Inspect()
        {
            return Value.Inspect();
        }

        public ObjectType Type()
        {
            return ObjectType.RETURN_VALUE_OBJ;
        }
    }

    internal struct Error : IObject
    {
        public string Message;
        public string Inspect()
        {
            return "ERRROR: " + Message;
        }

        public ObjectType Type()
        {
            return ObjectType.ERROR_OBJ;
        }
    }

    internal class Environment
    {
        public Dictionary<string, IObject> store;
        public Environment outer;

        public (IObject, bool) Get(string name)
        {
            if (store.ContainsKey(name))
            {

                return (store[name], true);
            }
            else
            {
                if (outer != null)
                {
                    return outer.Get(name);
                }
            }

            return (null, false);
        }

        public IObject Set(string name, IObject val)
        {
            store[name] = val;
            return val;
        }

        public static Environment NewEnvironment()
        {
            return new Environment() { store = new Dictionary<string, IObject>(), outer = null };
        }

        public static Environment NewEnclosedEnvironment(Environment outer)
        {
            Environment env = new() { store = new Dictionary<string, IObject>(), outer = null };
            env.outer = outer;
            return env;
        }

    }

    internal struct Function : IObject
    {
        public AST.Identifier[] Parameters;
        public AST.BlockStatement Body;
        public Environment Env;

        public string Inspect()
        {
            List<string> _params = new();
            foreach (AST.Identifier param in Parameters)
            {
                _params.Add(param.String());
            }
            string s = "fn(" + string.Join(",", _params.Select(x => x.ToString()).ToArray()) + "){\n" + Body.String() + "\n}";
            return s;
        }

        public ObjectType Type()
        {
            return ObjectType.FUNCTION_OBJ;
        }
    }

    internal struct String : IObject, IHashable
    {
        public string Value;
        public string Inspect()
        {
            return Value;
        }

        public ObjectType Type()
        {
            return ObjectType.STRING_OBJ;
        }
        public HashKey GetHashKey()
        {
            HashKey key = new();

            using (SHA256? sha256 = SHA256.Create())
            {
                key.Value = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(Value))).GetHashCode();
                key.Type = Type();
            }
            return key;
        }
    }

    internal struct Builtin : IObject
    {
        public Func<object[], object> BuiltinFunction;

        public string Inspect()
        {
            return "builtin function";
        }

        public ObjectType Type()
        {
            return ObjectType.BUILTIN_OBJ;
        }
    }

    internal struct Array : IObject
    {
        public IObject[] Elements;

        public string Inspect()
        {
            List<string> _elements = new();
            foreach (IObject? param in Elements)
            {
                _elements.Add(param.Inspect());
            }
            string s = "[" + string.Join(", ", _elements.Select(x => x.ToString()).ToArray()) + "]";
            return s;
        }

        public ObjectType Type()
        {
            return ObjectType.ARRAY_OBJ;
        }
    }

    internal struct HashKey
    {
        public ObjectType Type;
        public long Value;
    }

    internal struct HashPair
    {
        public IObject Key;
        public IObject Value;
    }

    internal struct Hash : IObject
    {
        public Dictionary<HashKey, HashPair> Pairs = new();
        public Hash()
        {

        }

        public string Inspect()
        {
            List<string> lines = new();
            foreach (KeyValuePair<HashKey, HashPair> pair in Pairs)
            {
                lines.Add($"{pair.Value.Key.Inspect()}: {pair.Value.Value.Inspect()}");
            }
            string? s = "{" + string.Join(", ", lines.Select(x => x.ToString()).ToArray()) + "}";
            return s;
        }

        public ObjectType Type()
        {
            return ObjectType.HASH_OBJ;
        }
    }

}
