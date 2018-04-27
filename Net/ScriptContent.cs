using System.Collections.Generic;

namespace Borealis.Net
{
    public class ScriptContent
    {
        public List<string[]> Items;

        public ScriptContent(string rawContent) {
            Items = new List<string[]>();
            if (rawContent == string.Empty) return;

            string[] list = rawContent.Delist();
            for (int i = 0; i < list.Length; i++) {
                list[i] = list[i].Replace(Network.ENCODED_ENUMERATOR_END, Network.ENUMERATOR_END);
                string[] enumerated = list[i].Denumerate();
                for (int j = 0; j < enumerated.Length; j++) {
                    enumerated[j] = enumerated[j].Replace(Network.ENCODED_ENUMERATOR, Network.ENUMERATOR);
                }
                Items.Add(enumerated);
            }
        }
    }
}
