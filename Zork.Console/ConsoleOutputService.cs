using System;
using Zork.Common;

namespace Zork
{
    internal class ConsoleOutputService : IOutputService
    {
        public void Write(object value)
        {
            Console.Write(value);
        }

        public void WriteLine(object value)
        {
            Console.WriteLine(value);
        }
    }
}
