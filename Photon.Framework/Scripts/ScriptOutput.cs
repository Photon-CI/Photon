using System;
using System.Text;

namespace Photon.Framework.Scripts
{
    public class LineAppendedEventArgs
    {
        public string Line {get;}

        public LineAppendedEventArgs(string line)
        {
            this.Line = line;
        }
    }

    [Serializable]
    public class ScriptOutput
    {
        public event EventHandler<LineAppendedEventArgs> LineAppended;

        private readonly StringBuilder builder;


        public ScriptOutput()
        {
            builder = new StringBuilder();
        }

        public void AppendLine(string line)
        {
            builder.AppendLine(line);
            OnLineAppended(line);
        }

        protected virtual void OnLineAppended(string line)
        {
            LineAppended?.Invoke(this, new LineAppendedEventArgs(line));
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
