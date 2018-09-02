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


        void IWrite.Write(string text, ConsoleColor color)
        {
            OnWrite?.Invoke(text, color);
        }

        public DomainOutput Write(string text, ConsoleColor color)
        {
            OnWrite?.Invoke(text, color);
            return this;
        }

        void IWrite.Write(object value, ConsoleColor color)
        {
            OnWrite?.Invoke(value, color);
        }

        public DomainOutput Write(object value, ConsoleColor color)
        {
            OnWrite?.Invoke(value, color);
            return this;
        }

        void IWrite.WriteLine(string text, ConsoleColor color)
        {
            OnWriteLine?.Invoke(text, color);
        }

        public DomainOutput WriteLine(string text, ConsoleColor color)
        {
            OnWriteLine?.Invoke(text, color);
            return this;
        }

        void IWrite.WriteLine(object value, ConsoleColor color)
        {
            OnWriteLine?.Invoke(value, color);
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

        IBlockWriter IWriteBlocks.WriteBlock()
        {
            return new DomainBlockWriter(this);
        }

        public DomainBlockWriter WriteBlock()
        {
            return new DomainBlockWriter(this);
        }
    }
}
