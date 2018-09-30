using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borealis.Net {
    public class EnumerableContent {
        public string[] Contents { get; set; }

        public EnumerableContent(params string[] contents) {
            Contents = contents;
        }

        public override string ToString() {
            if (Contents.Length < 1) return string.Empty;
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < Contents.Length; i++) {
                Contents[i] = Contents[i].Encode(Network.ENUMERATOR_END).Encode(Network.ENUMERATOR);
                if (i > 0) ret.Append(Network.ENUMERATOR);
                ret.Append(Contents[i]);
            }
            return ret.ToString();
        }
    }
}
