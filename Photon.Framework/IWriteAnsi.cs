using System;

namespace Photon.Framework
{
    public interface IWrite
    {
        void Write(string text, ConsoleColor color);
        void Write(object value, ConsoleColor color);
        void WriteLine(string text, ConsoleColor color);
        void WriteLine(object value, ConsoleColor color);
    }

    public interface IWrite<out T> : IWrite
        where T : IWrite<T>
    {
        new T Write(string text, ConsoleColor color);
        new T Write(object value, ConsoleColor color);
        new T WriteLine(string text, ConsoleColor color);
        new T WriteLine(object value, ConsoleColor color);
    }

    public interface IWriteBlocks : IWrite
    {
        IBlockWriter WriteBlock();
    }

    public interface IWriteBlocks<out T, out Z> : IWriteBlocks, IWrite<T>
        where T : IWrite<T>
        where Z : IBlockWriter<Z>
    {
        new Z WriteBlock();
    }

    public interface IBlockWriter : IWrite, IDisposable
    {
        void Post();
    }

    public interface IBlockWriter<out T> : IWrite<T>, IBlockWriter
        where T : IBlockWriter<T>
    {}
}
