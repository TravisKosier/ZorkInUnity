using System;

namespace Zork
{
    internal class ConsoleInputService : IInputService
    {
        public event EventHandler<string> InputReceived;
        public void ProcessInput()
        {
            string inputString = Console.ReadLine().Trim().ToUpper();
            InputReceived?.Invoke(this, inputString);
        }
    }
}
