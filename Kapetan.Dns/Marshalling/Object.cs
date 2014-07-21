using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Kapetan.Dns.Marshalling
{
    public class Object
    {
        public static Object New(object obj)
        {
            return new Object(obj);
        }

        public static string Dump(object obj)
        {
            return DumpObject(obj);
        }

        private static string DumpObject(object obj)
        {
            if (obj is string)
            {
                return (string)obj;
            }
            else if (obj is IDictionary)
            {
                return DumpDictionary((IDictionary)obj);
            }
            else if (obj is IEnumerable)
            {
                return DumpList((IEnumerable)obj);
            }
            else
            {
                return obj == null ? "null" : obj.ToString();
            }
        }

        private static string DumpList(IEnumerable enumerable)
        {
            return "[" + string.Join(", ", enumerable.Cast<object>().Select(o => DumpObject(o)).ToArray()) + "]";
        }

        private static string DumpDictionary(IDictionary dict)
        {
            StringBuilder result = new StringBuilder();

            result.Append("{");

            foreach (DictionaryEntry pair in dict)
            {
                result
                    .Append(pair.Key)
                    .Append("=")
                    .Append(DumpObject(pair.Value))
                    .Append(", ");
            }

            if (result.Length > 1)
            {
                result.Remove(result.Length - 2, 2);
            }

            return result.Append("}").ToString();
        }

        private object obj;
        private Dictionary<string, string> pairs;

        public Object(object obj)
        {
            this.obj = obj;
            this.pairs = new Dictionary<string, string>();
        }

        public Object Remove(params string[] names)
        {
            foreach (string name in names)
            {
                pairs.Remove(name);
            }

            return this;
        }

        public Object Add(params string[] names)
        {
            Type type = obj.GetType();

            foreach (string name in names)
            {
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                object value = property.GetValue(obj, new object[] { });

                pairs.Add(name, DumpObject(value));
            }

            return this;
        }

        public Object Add(string name, object value)
        {
            pairs.Add(name, DumpObject(value));
            return this;
        }

        public Object AddAll()
        {
            PropertyInfo[] properties = obj.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj, new object[] { });
                pairs.Add(property.Name, DumpObject(value));
            }

            return this;
        }

        public override string ToString()
        {
            return DumpDictionary(pairs);
        }
    }
}
