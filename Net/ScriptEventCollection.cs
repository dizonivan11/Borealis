using System.Collections.Generic;

namespace Borealis.Net {
    public class ScriptEventCollection : Dictionary<string, ScriptHandler> {
        public new void Add(string key, ScriptHandler handler) {
            if (ContainsKey(key)) Remove(key);
            base.Add(key, handler);
        }
    }
}
