using Photon.Framework.Domain;
using System;
using System.IO;
using System.Text;

namespace Photon.Framework.Server
{
    public class ScriptOutput : MarshalByRefInstance, IWriteAnsi
    {
        public event EventHandler Changed;

        private readonly StringBuilder builder;
        private readonly StringWriter writer;
        private readonly AnsiWriter ansiWriter;
        protected Lazy<object> lockHandle;

        public int Length {
            get {
                lock (lockHandle.Value) {
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
            lockHandle = new Lazy<object>();

            writer.NewLine = "\n";
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            writer?.Dispose();
        }

        public IWriteAnsi Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                ansiWriter.Write(text, color);
            }

            OnChanged();
            return this;
        }

        public IWriteAnsi Write(object value, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                ansiWriter.Write(value, color);
            }

            OnChanged();
            return this;
        }

        public IWriteAnsi WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                ansiWriter.WriteLine(text, color);
            }

            OnChanged();
            return this;
        }

        public IWriteAnsi WriteLine(object value, ConsoleColor color = ConsoleColor.Gray)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                ansiWriter.WriteLine(value, color);
            }

            OnChanged();
            return this;
        }

        public IWriteAnsi WriteBlock(Action<IWriteAnsi> writerAction)
        {
            if (writerAction == null) throw new ArgumentNullException(nameof(writerAction));
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            string text;
            using (var _writer = new ScriptOutput()) {
                writerAction.Invoke(_writer);
                _writer.Flush();
                text = _writer.GetString();
            }

            lock (lockHandle.Value) {
                writer.Flush();
                builder.Append(text);
            }

            return this;
        }

        public ScriptOutput WriteRaw(string text)
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                writer.Flush();
                builder.Append(text);
            }

            return this;
        }

        public void Flush()
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                writer.Flush();
            }
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public string GetString()
        {
            if (lockHandle == null) throw new ApplicationException("LockHandle is undefined!");

            lock (lockHandle.Value) {
                return builder.ToString();
            }
        }
    }
}
