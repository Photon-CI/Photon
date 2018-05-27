using System;
using System.IO;
using System.Text;

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
                if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

                lock (lockHandle) {
                    return builder.Length;
                }
            }
        }


        public ScriptOutput()
        {
            builder = new StringBuilder();
            writer = new StringWriter(builder);
            var x = TextWriter.Synchronized(writer);
            ansiWriter = new AnsiWriter(x);
            lockHandle = new object();

            writer.NewLine = "\n";
        }

        public void Dispose()
        {
            writer?.Dispose();
        }

        public ScriptOutput Append(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle) {
                ansiWriter.Write(text, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput Append(object value, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle) {
                ansiWriter.Write(value, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput AppendLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle) {
                ansiWriter.WriteLine(text, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput AppendLine(object value, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle) {
                ansiWriter.WriteLine(value, color);
            }

            OnChanged();
            return this;
        }

        public ScriptOutput AppendRaw(string text)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle) {
                writer.Flush();
                builder.Append(text);
            }

            return this;
        }

        public void Flush()
        {
            writer.Flush();
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public string GetString()
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle) {
                return builder.ToString();
            }
        }
    }
}
