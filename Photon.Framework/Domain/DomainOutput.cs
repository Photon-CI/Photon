using System;

namespace Photon.Framework.Domain
{
    public delegate void WriteFunc(object text, ConsoleColor color);
    public delegate void WriteLineFunc(object text, ConsoleColor color);
    public delegate void WriteRawFunc(string text);

    public class DomainOutput : MarshalByRefInstance, IWriteBlocks<DomainOutput, DomainBlockWriter>
    {
        public event WriteFunc OnWrite;
        public event WriteLineFunc OnWriteLine;
        public event WriteRawFunc OnWriteRaw;


        public DomainOutput Write(string text, ConsoleColor color)
        {
            OnWrite?.Invoke(text, color);
            return this;
        }

        public DomainOutput Write(object value, ConsoleColor color)
        {
            OnWrite?.Invoke(value, color);
            return this;
        }

        public DomainOutput WriteLine(string text, ConsoleColor color)
        {
            OnWriteLine?.Invoke(text, color);
            return this;
        }

        public DomainOutput WriteLine(object value, ConsoleColor color)
        {
            OnWriteLine?.Invoke(value, color);
            return this;
        }

        public DomainOutput WriteRaw(string text)
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
