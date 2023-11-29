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

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Logging;

public static partial class Logger
{
    [LoggerMessage(LogLevel.Information, "[Server] {message}")]
    public static partial void PrintServerMessage(ILogger logger, string message);
    
    [LoggerMessage(LogLevel.Information, "[Server] Starting Pokemon 3D Server.")]
    public static partial void PrintServerStarting(ILogger logger);

    [LoggerMessage(LogLevel.Information, "[Server] Server started listening on {ipEndpoint}.")]
    public static partial void PrintServerStarted(ILogger logger, IPEndPoint ipEndPoint);

    [LoggerMessage(LogLevel.Information, "[Server] Players with offline profile can join the server.")]
    public static partial void PrintServerOfflineMode(ILogger logger);

    [LoggerMessage(LogLevel.Information, "[Server] Players can join with any GameMode(s).")]
    public static partial void PrintServerPlayerCanJoinWithAny(ILogger logger);

    [LoggerMessage(LogLevel.Information, "[Server] Players can join with any GameModes except the following GameMode(s): {gameModes}")]
    public static partial void PrintServerPlayerCanJoinWithAnyExcept(ILogger logger, string gameModes);

    [LoggerMessage(LogLevel.Information, "[Server] Players can join with the following GameMode(s): {gameModes}")]
    public static partial void PrintServerPlayerCanJoinWith(ILogger logger, string gameModes);

    [LoggerMessage(LogLevel.Information, "[Server] Checking port {port} is open...")]
    public static partial void PrintRunningPortCheck(ILogger logger, int port);

    [LoggerMessage(LogLevel.Warning, "[Server] Unable to get public IP Address. Ensure that you have open the port {port} for players to join.")]
    public static partial void PrintUnableToGetPublicIpAddress(ILogger logger, int port);

    [LoggerMessage(LogLevel.Information, "[Server] Port {port} is opened. Players will be able to join via {ipEndpoint} (Public).")]
    public static partial void PrintPublicPortIsAvailable(ILogger logger, int port, IPEndPoint ipEndPoint);

    [LoggerMessage(LogLevel.Information, "[Server] Port {port} is not opened. Players will not be able to join.")]
    public static partial void PrintPublicPortIsNotAvailable(ILogger logger, int port);

    [LoggerMessage(LogLevel.Error, "[Server] Error ({socketError}): {message}")]
    public static partial void PrintServerError(ILogger logger, Exception exception, SocketError socketError, string message);

    [LoggerMessage(LogLevel.Information, "[Server] Stopped listening for new players.")]
    public static partial void PrintServerStopped(ILogger logger);
}