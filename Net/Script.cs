using System.Collections.Generic;

public delegate void ScriptHandler(Borealis.Net.Network sender, Borealis.Net.ScriptEventArgs e);

namespace Borealis.Net
{
    public static class Script
    {
        public static Dictionary<string, ScriptHandler> Events;

        static Script() {
            Events = new Dictionary<string, ScriptHandler>();
        }

        public static void Run(Network sender, ScriptEventArgs e) {
            if (Events.ContainsKey(e.Header)) {
                Events[e.Header].DynamicInvoke(sender, e);
            }
        }
    }
}
