using System;

namespace Photon.Framework
{
    public interface IWrite<out T>
        where T : IWrite<T>
    {
        T Write(string text, ConsoleColor color);
        T Write(object value, ConsoleColor color);
        T WriteLine(string text, ConsoleColor color);
        T WriteLine(object value, ConsoleColor color);
    }

    public interface IWriteBlocks<out T, out Z> : IWrite<T>
        where T : IWrite<T>
        where Z : IBlockWriter<Z>
    {
        Z WriteBlock();
    }

    public interface IBlockWriter<out T> : IWrite<T>, IDisposable
        where T : IBlockWriter<T>
    {
        void Post();
    }
}
