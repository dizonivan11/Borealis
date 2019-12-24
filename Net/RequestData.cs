using System.Collections.Generic;
using System.Text;

namespace Borealis.Net {
    public class RequestData : Dictionary<string, string> {

        // Transmission Control characters
        const char STX = '\u0002'; // Start Of Text character
        const char GS = '\u001d'; // Group Separator character
        const char ETX = '\u0003'; // End Of Text character
        public const char EOT = '\u0004'; // End Of Transmission character
        public bool HasHeader { get { return ContainsKey("header"); } }

        public static string Stringify(RequestData rd) {
            StringBuilder sb = new StringBuilder();
            foreach (var key in rd.Keys) {
                string value = rd[key];
                sb.Append(STX);
                sb.Append(key);
                sb.Append(GS);
                sb.Append(value);
                sb.Append(ETX);
            }
            sb.Append(EOT);
            return sb.ToString();
        }

        public static RequestData Destringify(string s) {
            RequestData rd = new RequestData();
            StringBuilder sb = new StringBuilder();
            bool storing = false;
            string cKey = string.Empty;
            bool ended = false;
            for (int i = 0; i < s.Length; i++) {
                switch (s[i]) {
                    // start of key
                    case STX:
                        storing = true;
                        cKey = string.Empty;
                        sb.Clear();
                        break;
                    // end of key, start of value
                    case GS:
                        cKey = sb.ToString();
                        sb.Clear();
                        break;
                    // end of value, add to record
                    case ETX:
                        storing = false;
                        rd.Add(cKey, sb.ToString());
                        cKey = string.Empty;
                        sb.Clear();
                        break;
                    // end of request
                    case EOT:
                        ended = true;
                        break;
                    default:
                        if (storing) sb.Append(s[i]);
                        break;
                }
                if (ended) break;
            }
            return rd;
        }
    }
}
