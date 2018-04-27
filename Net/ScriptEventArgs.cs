using System;

namespace Borealis.Net
{
    public class ScriptEventArgs
    {
        public string Header { get; private set; }
        public ScriptContent Content { get; private set; }

        public ScriptEventArgs(string request) {
            if (request.IndexOf(Network.SPLITTER) > -1) {
                string[] tokens = request.Split(new string[] { Network.SPLITTER }, 2, StringSplitOptions.None);
                Header = tokens[0];
                Content = new ScriptContent(tokens[1]);
            } else {
                Header = string.Empty;
                Content = null;
            }
        }
    }

    public static class StringExtension
    {
        public static string[] Enumerate(this string value) {
            return value.Split(new string[] { Network.ENUMERATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] List(this string value) {
            return value.Split(new string[] { Network.ENUMERATOR_END }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}