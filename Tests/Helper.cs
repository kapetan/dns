using System.Collections.Generic;
using System.IO;

namespace DNS.Tests {
    public static class Helper {
        
        public static byte[] ReadFixture(params string[] paths) {
            string path = Path.Combine(paths);
            path = Path.Combine("Fixtures", path);

            return File.ReadAllBytes(path);
        }

        public static T[] GetArray<T>(params T[] parameters) {
            return parameters;
        }

        public static IList<T> GetList<T>(params T[] parameters) {
            return new List<T>(parameters);
        }
    }
}
