using System;
using System.IO;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace TheDialgaTeam.Pokemon3D.Server.Serilog.Sink
{
    internal class ActionSink : ILogEventSink, IDisposable
    {
        private readonly Action<string> _outputAction;
        private readonly ITextFormatter _formatter;
        private readonly object _syncRoot;

        private readonly MemoryStream _memoryStream = new();
        private readonly StreamWriter _streamWriter;

        public ActionSink(Action<string> outputAction, ITextFormatter formatter, object syncRoot)
        {
            _outputAction = outputAction;
            _formatter = formatter;
            _syncRoot = syncRoot;
            _streamWriter = new StreamWriter(_memoryStream);
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_syncRoot)
            {
                _memoryStream.Seek(0, SeekOrigin.Begin);
                _memoryStream.SetLength(0);

                _formatter.Format(logEvent, _streamWriter);

                _streamWriter.Flush();
                _outputAction(Encoding.UTF8.GetString(_memoryStream.ToArray()));
            }
        }

        public void Dispose()
        {
            _memoryStream.Dispose();
            _streamWriter.Dispose();
        }
    }
}