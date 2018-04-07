using AnsiConsole;
using System;
using System.IO;
using System.Text;

namespace Photon.Framework.Scripts
{
    public class ScriptOutput : MarshalByRefObject, IDisposable
    {
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

        public override string ToString()
        {
            lock (lockHandle) {
                return builder.ToString();
            }
        }
    }
}
