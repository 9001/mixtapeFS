using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace mixtapeFS
{
    class Z
    {
        // http://csharptest.net/529/how-to-correctly-escape-command-line-arguments-in-c/index.html
        public static string EscapeArguments(params string[] args)
        {
            StringBuilder arguments = new StringBuilder();
            Regex invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
            Regex needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
            Regex escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string
            for (int carg = 0; args != null && carg < args.Length; carg++)
            {
                if (args[carg] == null) { throw new ArgumentNullException("args[" + carg + "]"); }
                if (invalidChar.IsMatch(args[carg])) { throw new ArgumentOutOfRangeException("args[" + carg + "]"); }
                if (args[carg] == String.Empty) { arguments.Append("\"\""); }
                else if (!needsQuotes.IsMatch(args[carg])) { arguments.Append(args[carg]); }
                else
                {
                    arguments.Append('"');
                    arguments.Append(escapeQuote.Replace(args[carg], m =>
                    m.Groups[1].Value + m.Groups[1].Value +
                    (m.Groups[2].Value == "\"" ? "\\\"" : "")
                    ));
                    arguments.Append('"');
                }
                if (carg + 1 < args.Length)
                    arguments.Append(' ');
            }
            return arguments.ToString();
        }
    }

    class Logger
    {
        public List<String> msgs;

        public Logger()
        {
            msgs = new List<string>();
        }

        public void log(string[] words)
        {
            //if (!words[0].StartsWith("cache"))
            //    return;

            lock (msgs)
            {
                msgs.Add(System.DateTime.UtcNow.ToString("HH:mm:ss.ffff [") + string.Join("] [", words) + "]");
            }
        }

        public string[] pop()
        {
            lock (msgs)
            {
                var ret = msgs.ToArray();
                msgs.Clear();
                return ret;
            }
        }
    }
}
