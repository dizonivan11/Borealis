using System;
using System.Collections.Generic;
using System.Text;

namespace Borealis.Net
{
    public static class ContentController
    {
        public static string Enumerate(this string[] values) {
            if (values.Length < 1) return string.Empty;
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < values.Length; i++) {
                if (i > 0) ret.Append(Network.ENUMERATOR);
                ret.Append(values[i]);
            }
            return ret.ToString();
        }

        public static string List(this string[] enumeratedValues) {
            if (enumeratedValues.Length < 1) return string.Empty;
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < enumeratedValues.Length; i++) {
                string value = enumeratedValues[i] + Network.ENUMERATOR_END;
                if (i > 0) ret.Append(Network.ENUMERATOR_END);
                ret.Append(value);
            }
            return ret.ToString();
        }

        public static string Enumerate(this List<string> values) {
            if (values.Count < 1) return string.Empty;
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < values.Count; i++) {
                if (i > 0) ret.Append(Network.ENUMERATOR);
                ret.Append(values[i]);
            }
            return ret.ToString();
        }
        
        public static string List(this List<string> enumeratedValues) {
            if (enumeratedValues.Count < 1) return string.Empty;
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < enumeratedValues.Count; i++) {
                string value = enumeratedValues[i] + Network.ENUMERATOR_END;
                if (i > 0) ret.Append(Network.ENUMERATOR_END);
                ret.Append(value);
            }
            return ret.ToString();
        }

        public static string[] Denumerate(this string value) {
            return value.Split(Network.ENUMERATOR);
        }

        public static string[] Delist(this string value) {
            return value.Split(Network.ENUMERATOR_END);
        }
    }
}
