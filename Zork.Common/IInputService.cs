using System;

namespace Zork.Common
{
    public interface IInputService
    {
        event EventHandler<string> InputReceived;
    }
}
