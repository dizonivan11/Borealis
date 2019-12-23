public delegate void ScriptHandler(Borealis.Net.Network sender, Borealis.Net.RequestData r);

namespace Borealis.Net
{
    public class Script
    {
        public readonly static ScriptEventCollection Events = new ScriptEventCollection();

        public static void Run(Network sender, RequestData r) {
            if (r.HasHeader) {
                Events[r["header"].ToString()].DynamicInvoke(sender, r);
            }
        }
        
        public Network Sender { get; set; }
        public RequestData Request { get; set; }

        public Script(Network sender, RequestData r) {
            Sender = sender;
            Request = r;
        }

        public void Run() {
            Run(Sender, Request);
        }
    }
}
