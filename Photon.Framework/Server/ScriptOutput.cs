using System;
using System.IO;
using System.Text;
using AnsiConsole;

namespace Photon.Framework.Server
{
    public class ScriptOutput : MarshalByRefObject, IDisposable
    {
        public event EventHandler Changed;

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

            writer.NewLine = "\n";
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

            OnChanged();
            return this;
        }

        public ScriptOutput Append(object value, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (lockHandle) {
                ansiWriter.Write(value, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput AppendLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (lockHandle) {
                ansiWriter.WriteLine(text, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput AppendLine(object value, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (lockHandle) {
                ansiWriter.WriteLine(value, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput AppendRaw(string text)
        {
            lock (lockHandle) {
                writer.Flush();
                builder.Append(text);
            }

            return this;
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            lock (lockHandle) {
                return builder.ToString();
            }
        }
    }
}
