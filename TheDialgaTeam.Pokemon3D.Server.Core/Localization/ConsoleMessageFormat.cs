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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Localization;

public sealed class ConsoleMessageFormat
{
    #region PokemonServerListener

    public string ServerIsStarting { get; set; } = "[Server] Starting Pokemon 3D Server.";

    public string ServerStartedListening { get; set; } = "[Server] Server started listening on {0}";

    public string ServerStoppedListening { get; set; } = "[Server] Stopped listening for new players.";

    public string ServerAllowOfflineProfile { get; set; } = "[Server] Players with offline profile can join the server.";

    public string ServerAllowAnyGameModes { get; set; } = "[Server] Players can join with any GameMode(s).";

    public string ServerAllowAnyGameModesExcept { get; set; } = "[Server] Players can join with any GameModes except the following GameMode(s): {0}";

    public string ServerAllowOnlyGameModes { get; set; } = "[Server] Players can join with the following GameMode(s): {0}";

    public string ServerRunningPortCheck { get; set; } = "[Server] Checking port {0} is open.";

    public string ServerPortCheckFailed { get; set; } = "[Server] Unable to check port {0} due to error getting public ip address.";

    public string ServerPortIsOpened { get; set; } = "[Server] Port {0} is opened. Players will be able to join via {1}.";

    public string ServerPortIsClosed { get; set; } = "[Server] Port {0} is closed. Players will not be able to join using public ip address.";

    public string ServerError { get; set; } = "[Server] Error: {0}";

    #endregion
    
    #region NatDeviceUtility
    
    public string NatSearchForUpnpDevices { get; set; } = "[NAT] Searching for UPnP devices. This will take {0:F0} seconds.";

    public string NatFoundUpnpDevices { get; set; } = "[NAT] Found {0} UPnP devices.";

    public string NatNatCreatedUpnpDeviceMapping { get; set; } = "[NAT] Created new UPnP port mapping for interface {0}.";

    #endregion

    #region PokemonServerClient

    public string ClientReceiveRawPacket { get; set; } = "[Client {0}] Receive raw packet data: {1}";
    
    public string ClientSendRawPacket { get; set; } = "[Client {0}] Send raw packet data: {1}";

    public string ClientReadSocketIssue { get; set; } = "[Client {0}] Unable to read data from this network.";
    
    public string ClientWriteSocketIssue { get; set; } = "[Client {0}] Unable to write data from this network.";
    
    public string ClientReceiveInvalidPacket { get; set; } = "[Client {0}] Invalid packet received.";

    public string ClientDisconnected { get; set; } = "[Client {0}] Disconnected.";

    #endregion

    #region NetworkContainer
    
    public string PlayerJoin { get; set; } = "[Server] {0} join the server.";
    public string PlayerLeft { get; set; } = "[Server] {0} left the server.";
    public string PlayerLeftWithReason { get; set; } = "[Server] {0} left the server with the following reason: {1}";
    
    public string PlayerUnableToJoin { get; set; } = "[Server] {0} is unable to join the server with the following reason: {1}";

    #endregion

    #region World

    public string GlobalWorldStatus { get; set; } = "[World] Current Season: {0} | Current Weather: {1} | Current Time: {2}";

    #endregion
}