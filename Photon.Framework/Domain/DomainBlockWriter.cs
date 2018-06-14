using Photon.Framework.Server;
using System;

namespace Photon.Framework.Domain
{
    [Serializable]
    public class DomainBlockWriter : IDisposable
    {
        private readonly DomainOutput domainOutput;
        private readonly ScriptOutput output;
        private volatile bool isPosted;


        public DomainBlockWriter(DomainOutput output)
        {
            this.domainOutput = output;

            this.output = new ScriptOutput();
        }

        public void Dispose()
        {
            Post();

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
            if (isPosted) return;
            isPosted = true;

            domainOutput.WriteRaw(output.GetString());
        }
    }
}
