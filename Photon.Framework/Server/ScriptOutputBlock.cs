using System;

namespace Photon.Framework.Server
{
    [Serializable]
    public class ScriptOutputBlock : IBlockWriter<ScriptOutputBlock>
    {
        private readonly ScriptOutput scriptOutput;
        private readonly ScriptOutput blockOutput;
        private volatile bool isPosted;


        public ScriptOutputBlock(ScriptOutput output)
        {
            scriptOutput = output;
            blockOutput = new ScriptOutput();
        }

        public void Dispose()
        {
            Post();

            blockOutput?.Dispose();
        }

        public ScriptOutputBlock Write(string text, ConsoleColor color)
        {
            blockOutput.Write(text, color);
            return this;
        }

        public ScriptOutputBlock Write(object value, ConsoleColor color)
        {
            blockOutput.Write(value, color);
            return this;
        }

        public ScriptOutputBlock WriteLine(string text, ConsoleColor color)
        {
            blockOutput.WriteLine(text, color);
            return this;
        }

        public ScriptOutputBlock WriteLine(object value, ConsoleColor color)
        {
            blockOutput.WriteLine(value, color);
            return this;
        }

        public void Post()
        {
            if (isPosted) return;
            isPosted = true;

            scriptOutput.WriteRaw(blockOutput.GetString());
        }
    }
}
