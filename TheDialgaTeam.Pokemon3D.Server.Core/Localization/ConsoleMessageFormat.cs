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

using System.Diagnostics.CodeAnalysis;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Localization;

public sealed class ConsoleMessageFormat
{
    #region PokemonServerListener

    public string ServerIsStarting { get; init; } = "[Server] Starting Pokemon 3D Server.";

    public string ServerStartedListening { get; init; } = "[Server] Server started listening on {0}";

    public string ServerStoppedListening { get; init; } = "[Server] Stopped listening for new players.";

    public string ServerAllowOfflineProfile { get; init; } = "[Server] Players with offline profile can join the server.";

    public string ServerAllowAnyGameModes { get; init; } = "[Server] Players can join with any GameMode(s).";

    public string ServerAllowAnyGameModesExcept { get; init; } = "[Server] Players can join with any GameModes except the following GameMode(s): {0}";

    public string ServerAllowOnlyGameModes { get; init; } = "[Server] Players can join with the following GameMode(s): {0}";

    public string ServerRunningPortCheck { get; init; } = "[Server] Checking port {0} is open.";

    public string ServerPortCheckFailed { get; init; } = "[Server] Unable to check port {0} due to error getting public ip address.";

    public string ServerPortIsOpened { get; init; } = "[Server] Port {0} is opened. Players will be able to join via {1}.";

    public string ServerPortIsClosed { get; init; } = "[Server] Port {0} is closed. Players will not be able to join using public ip address.";

    public string ServerError { get; init; } = "[Server] Error: {0}";

    #endregion
    
    #region NatDeviceUtility
    
    public string NatSearchForUpnpDevices { get; init; } = "[NAT] Searching for UPnP devices. This will take {0:F0} seconds.";

    public string NatFoundUpnpDevices { get; init; } = "[NAT] Found {0} UPnP devices.";

    public string NatNatCreatedUpnpDeviceMapping { get; init; } = "[NAT] Created new UPnP port mapping for interface {0}.";

    #endregion

    #region NetworkContainer

    public string PlayerUnableToJoin { get; init; } = "[Server] {0} is unable to join the server with the following reason: {1}";

    #endregion

    #region World

    public string GlobalWorldStatus { get; init; } = "[World] Current Season: {0} | Current Weather: {1} | Current Time: {2}";

    #endregion
}