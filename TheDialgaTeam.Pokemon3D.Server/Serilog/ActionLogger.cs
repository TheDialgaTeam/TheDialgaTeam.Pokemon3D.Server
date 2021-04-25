using System;

namespace TheDialgaTeam.Pokemon3D.Server.Serilog
{
    internal class ActionLogger
    {
        public event Action<string>? Log;

        public void WriteToLogEvent(string output)
        {
            Log?.Invoke(output);
        }
    }
}