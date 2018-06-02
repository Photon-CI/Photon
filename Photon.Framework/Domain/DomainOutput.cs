using System;

namespace Photon.Framework.Domain
{
    public delegate void WriteFunc(object text, ConsoleColor color);
    public delegate void WriteLineFunc(object text, ConsoleColor color);

    public class DomainOutput : MarshalByRefInstance, IWriteAnsi
    {
        public event WriteFunc OnWrite;
        public event WriteLineFunc OnWriteLine;


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
    }
}
