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

using TheDialgaTeam.Pokemon3D.Server.Core.Localization;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed class LocalizationOptions
{
    public Dictionary<string, LocalizedString> CultureInfo { get; set; } = new() { { "en", new LocalizedString() } };
}

public sealed class LocalizedString
{
    public PlayerNameDisplayFormat PlayerNameDisplayFormat { get; set; } = new();
    
    public ConsoleMessageFormat ConsoleMessageFormat { get; set; } = new();
    
    public GameMessageFormat GameMessageFormat { get; set; } = new();
}

public sealed class PlayerNameDisplayFormat
{
    public string GameJoltNameDisplayFormat { get; set; } = "{0} ({1})";

    public string OfflineNameDisplayFormat { get; set; } = "{0}";
}

public sealed class ConsoleMessageFormat
{
    public string ServerIsStarting { get; set; } = "Starting Pokemon 3D Server.";
    public string ServerStartedListening { get; set; } = "Server started listening on {0}";
    public string ServerAllowOfflineProfile { get; set; } = "Players with offline profile can join the server.";
    public string ServerAllowAnyGameModes { get; set; } = "Players can join with any GameMode(s).";
    public string ServerAllowAnyGameModesExcept { get; set; } = "Players can join with any GameModes except the following GameMode(s): {0}";
    public string ServerAllowOnlyGameModes { get; set; } = "Players can join with the following GameMode(s): {0}";
    public string ServerBindingError { get; set; } = "Unable to bind {0} due to an exception.";
    public string ServerStoppedListening { get; set; } = "Stopped listening for new players.";
    
    public string ServerRunningPortCheck { get; set; } = "Checking port {0} is open.";
    public string ServerPortCheckFailed { get; set; } = "Unable to check port {0} due to error getting public ip address.";
    public string ServerPortIsOpened { get; set; } = "Port {0} is opened. Players will be able to join via {1}";
    public string ServerPortIsClosed { get; set; } = "Port {0} is closed. Players will not be able to join using public ip address.";
    
    public string NatSearchForUpnpDevice { get; set; } = "Searching for UPnP device. This will take up to {0:F0} seconds.";
    public string NatFoundUpnpDevice { get; set; } = "Found UPnP device at {0}";
    public string NatCreatedUpnpDeviceMapping { get; set; } = "Created new UPnP port mapping for interface {0}";

    public string ClientReceivedRawPacket { get; set; } = "Received raw packet data: {0}";
    public string ClientSentRawPacket { get; set; } = "Sent raw packet data: {0}";
    public string ClientReadSocketIssue { get; set; } = "Unable to read data from this client.";
    public string ClientWriteSocketIssue { get; set; } = "Unable to write data to this client.";
    public string ClientReceivedInvalidPacket { get; set; } = "Invalid packet received.";
    public string ClientReceivedNoPing { get; set; } = "Client did not send a valid ping for too long.";
    public string ClientDisconnected { get; set; } = "Client is disconnected.";

    public string ServerUncaughtExceptionThrown { get; set; } = "Server has thrown an error when handling packet data.";

    public string GlobalWorldStatus { get; set; } = "Current Season: {0} | Current Weather: {1} | Current Time: {2}";

    #region NetworkContainer

    public string PlayerJoin { get; set; } = "[Server] {0} join the server.";
    public string PlayerLeft { get; set; } = "[Server] {0} left the server.";
    public string PlayerLeftWithReason { get; set; } = "[Server] {0} left the server with the following reason: {1}";

    public string PlayerUnableToJoin { get; set; } = "[Server] {0} is unable to join the server with the following reason: {1}";

    public string GameStateMessage { get; set; } = "[Server] The player {0} {1}";

    #endregion
}

public sealed class GameMessageFormat
{
    public string ServerIsFull { get; set; } = "This server is currently full of players.";
    public string ServerOnlyAllowGameJoltProfile { get; set; } = "This server do not allow offline profile.";
    public string ServerWhitelistedGameModes { get; set; } = "This server require you to play the following GameMode(s): {0}";
    public string ServerBlacklistedGameModes { get; set; } = "This server do not allow this GameMode to join.";

    public string ServerError { get; set; } = "This server has thrown an error when handling packet data.";

    public string PlayerJoin { get; set; } = "{0} join the server.";
    public string PlayerLeft { get; set; } = "{0} left the server.";

    public string GameStateMessage { get; set; } = "The player {0} {1}";
}