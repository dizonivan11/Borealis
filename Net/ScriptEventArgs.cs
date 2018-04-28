using System;

namespace Borealis.Net
{
    public class ScriptEventArgs
    {
        public string Header { get; private set; }
        public ScriptContent Content { get; private set; }

        public ScriptEventArgs(string request) {
            if (request.IndexOf(Network.SPLITTER.Decoded) > -1) {
                string[] tokens = request.Split(new string[] { Network.SPLITTER.Decoded }, 2, StringSplitOptions.None);
                Header = tokens[0];
                Content = new ScriptContent(tokens[1]);
            } else {
                Header = string.Empty;
                Content = null;
            }
        }
    }
}