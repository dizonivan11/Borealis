using System.Collections.Generic;

public delegate void ScriptHandler(Borealis.Net.Network sender, Borealis.Net.ScriptEventArgs e);

namespace Borealis.Net
{
    public class Script
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
        
        public Network Sender { get; set; }
        public ScriptEventArgs Args { get; set; }

        public Script(Network sender, ScriptEventArgs e) {
            Sender = sender;
            Args = e;
        }

        public void Run() {
            Run(Sender, Args);
        }
    }
}
