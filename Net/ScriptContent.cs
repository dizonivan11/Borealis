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
                string[] enumerated = list[i].Denumerate();
                Items.Add(enumerated);
            }
        }
    }
}
