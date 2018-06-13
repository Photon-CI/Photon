using System;

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

        public IWriteAnsi WriteRaw(string text)
        {
            OnWriteRaw?.Invoke(text);
            return this;
        }

        public DomainBlockWriter WriteBlock()
        {
            return new DomainBlockWriter(this);
        }
    }
}
