using System;
using Photon.Framework.Server;

namespace Photon.Framework.Domain
{
    public delegate void WriteFunc(object text, ConsoleColor color);
    public delegate void WriteLineFunc(object text, ConsoleColor color);
    public delegate void WriteRawFunc(string text);

    public class DomainOutput : MarshalByRefInstance, IWriteAnsi
    {
        public event WriteFunc OnWrite;
        public event WriteLineFunc OnWriteLine;
        public event WriteRawFunc OnWriteRaw;


        public IWriteAnsi Write(string text, ConsoleColor color)
        {
            OnWrite?.Invoke(text, color);
            return this;
        }

        public IWriteAnsi Write(object value, ConsoleColor color)
        {
            OnWrite?.Invoke(value, color);
            return this;
        }

        public IWriteAnsi WriteLine(string text, ConsoleColor color)
        {
            OnWriteLine?.Invoke(text, color);
            return this;
        }

        public IWriteAnsi WriteLine(object value, ConsoleColor color)
        {
            OnWriteLine?.Invoke(value, color);
            return this;
        }

        public IWriteAnsi WriteBlock(Action<IWriteAnsi> writerAction)
        {
            if (writerAction == null) throw new ArgumentNullException(nameof(writerAction));

            using (var writer = new ScriptOutput()) {
                writerAction.Invoke(writer);
                writer.Flush();

                OnWriteRaw?.Invoke(writer.GetString());
            }

            return this;
        }
    }
}
