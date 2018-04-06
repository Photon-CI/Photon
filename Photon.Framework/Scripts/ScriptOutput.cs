using AnsiConsole;
using System;
using System.IO;
using System.Text;

namespace Photon.Framework.Scripts
{
    //public class LineAppendedEventArgs
    //{
    //    public string Line {get;}

    //    public LineAppendedEventArgs(string line)
    //    {
    //        this.Line = line;
    //    }
    //}

    public class ScriptOutput : MarshalByRefObject, IDisposable
    {
        //public event EventHandler<LineAppendedEventArgs> LineAppended;

        private readonly StringBuilder builder;
        private readonly StringWriter writer;
        private readonly AnsiWriter ansiWriter;
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
            writer = new StringWriter(builder);
            ansiWriter = new AnsiWriter(writer);
            lockHandle = new object();
        }

        public void Dispose()
        {
            writer?.Dispose();
        }

        public ScriptOutput Append(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (lockHandle) {
                ansiWriter.Write(text, color);
            }

            return this;
        }

        public ScriptOutput AppendLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (lockHandle) {
                ansiWriter.WriteLine(text, color);
            }

            //OnLineAppended(line);
            return this;
        }

        //protected virtual void OnLineAppended(string line)
        //{
        //    LineAppended?.Invoke(this, new LineAppendedEventArgs(line));
        //}

        public override string ToString()
        {
            lock (lockHandle) {
                return builder.ToString();
            }
        }
    }
}
