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

    //[Serializable]
    public class ScriptOutput : MarshalByRefObject
    {
        public event EventHandler<LineAppendedEventArgs> LineAppended;

        private readonly StringBuilder builder;
        private readonly object lockHandle;

        public int Length {
            get {
                lock (lockHandle) {
                    return builder.Length;
                }
            }
        }


        public ScriptOutput()
        {
            builder = new StringBuilder();
            lockHandle = new object();
        }

        public void AppendLine(string line)
        {
            lock (lockHandle) {
                builder.AppendLine(line);
            }

            OnLineAppended(line);
        }

        protected virtual void OnLineAppended(string line)
        {
            LineAppended?.Invoke(this, new LineAppendedEventArgs(line));
        }

        public override string ToString()
        {
            lock (lockHandle) {
                return builder.ToString();
            }
        }
    }
}
