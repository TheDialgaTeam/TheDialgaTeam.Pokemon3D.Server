using System;

namespace TheDialgaTeam.Pokemon3D.Server.Serilog
{
    internal class UiLogger
    {
        public event Action<string>? Log;

        public void WriteToLogOutput(string output)
        {
            Log?.Invoke(output);
        }
    }
}