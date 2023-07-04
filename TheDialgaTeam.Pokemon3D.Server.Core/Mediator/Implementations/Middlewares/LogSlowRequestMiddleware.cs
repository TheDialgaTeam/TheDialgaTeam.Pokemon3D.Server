// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations.Middlewares;

internal sealed partial class LogSlowRequestMiddleware<TRequest> : IRequestMiddleware<TRequest> where TRequest : IRequest
{
    private readonly ILogger<LogSlowRequestMiddleware<TRequest>> _logger;

    public LogSlowRequestMiddleware(ILogger<LogSlowRequestMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        await next().ConfigureAwait(false);
        var duration = Stopwatch.GetElapsedTime(startTime);

        if (duration > TimeSpan.FromMilliseconds(500))
        {
            PrintLongRunningRequest(typeof(TRequest).Name, duration.TotalMilliseconds, request);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Request] Long Running Request: {name} ({elapsedMilliseconds:F2} ms) {request}")]
    private partial void PrintLongRunningRequest(string name, double elapsedMilliseconds, TRequest request);
}

internal sealed partial class LogSlowRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LogSlowRequestMiddleware<TRequest, TResponse>> _logger;

    public LogSlowRequestMiddleware(ILogger<LogSlowRequestMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        var response = await next().ConfigureAwait(false);
        var duration = Stopwatch.GetElapsedTime(startTime);

        if (duration > TimeSpan.FromMilliseconds(500))
        {
            PrintLongRunningRequest(typeof(TRequest).Name, duration.TotalMilliseconds, request);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Request] Long Running Request: {name} ({elapsedMilliseconds:F2} ms) {request}")]
    private partial void PrintLongRunningRequest(string name, double elapsedMilliseconds, TRequest request);
}