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

    public class DomainBlockWriter : IDisposable
    {
        private readonly DomainOutput domainOutput;
        private readonly ScriptOutput output;


        public DomainBlockWriter(DomainOutput output)
        {
            this.domainOutput = output;

            this.output = new ScriptOutput();
        }

        public void Dispose()
        {
            output?.Dispose();
        }

        public DomainBlockWriter Write(string text, ConsoleColor color)
        {
            output.Write(text, color);
            return this;
        }

        public DomainBlockWriter Write(object value, ConsoleColor color)
        {
            output.Write(value, color);
            return this;
        }

        public DomainBlockWriter WriteLine(string text, ConsoleColor color)
        {
            output.WriteLine(text, color);
            return this;
        }

        public DomainBlockWriter WriteLine(object value, ConsoleColor color)
        {
            output.WriteLine(value, color);
            return this;
        }

        public void Post()
        {
            domainOutput.WriteRaw(output.GetString());
        }
    }
}
