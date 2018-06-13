using System;

namespace Photon.Framework
{
    public interface IWriteAnsi
    {
        IWriteAnsi Write(string text, ConsoleColor color);
        IWriteAnsi Write(object value, ConsoleColor color);
        IWriteAnsi WriteLine(string text, ConsoleColor color);
        IWriteAnsi WriteLine(object value, ConsoleColor color);
        IWriteAnsi WriteBlock(Action<IWriteAnsi> writerAction);
    }
}
