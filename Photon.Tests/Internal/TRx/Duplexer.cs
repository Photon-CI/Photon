using Photon.Framework;
using System;
using System.IO;

namespace Photon.Tests.Internal.TRx
{
    internal class Duplexer : IDisposable
    {
        private readonly DuplexerInputStream inputStreamA;
        private readonly DuplexerInputStream inputStreamB;
        private readonly DuplexerOutputStream outputStreamA;
        private readonly DuplexerOutputStream outputStreamB;

        public Stream StreamA {get;}
        public Stream StreamB {get;}


        public Duplexer()
        {
            var pipeA = new DuplexerPipe();
            var pipeB = new DuplexerPipe();

            inputStreamA = new DuplexerInputStream(pipeA);
            inputStreamB = new DuplexerInputStream(pipeB);
            outputStreamA = new DuplexerOutputStream(pipeA);
            outputStreamB = new DuplexerOutputStream(pipeB);

            StreamA = new CombinedStream(inputStreamA, outputStreamB);
            StreamB = new CombinedStream(inputStreamB, outputStreamA);
        }

        public void Dispose()
        {
            inputStreamA?.Dispose();
            inputStreamB?.Dispose();
            outputStreamA?.Dispose();
            outputStreamB?.Dispose();

            StreamA?.Dispose();
            StreamB?.Dispose();
        }
    }
}
