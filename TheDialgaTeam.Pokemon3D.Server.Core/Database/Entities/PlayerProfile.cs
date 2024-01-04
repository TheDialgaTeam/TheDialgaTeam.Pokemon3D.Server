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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Database.Entities;

[Index(nameof(DisplayName), IsUnique = true)]
[Index(nameof(GameJoltId))]
public sealed class PlayerProfile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PlayerProfileId { get; init; }
    
    [StringLength(512)]
    public required string DisplayName { get; set; }
    
    public ulong? GameJoltId { get; init; }
    
    [StringLength(512)]
    public string? Password { get; set; }
    
    public required PlayerType PlayerType { get; set; }
    
    public required LocalWorld LocalWorld { get; init; }

    public List<BlockedPlayerProfile> BlockProfiles { get; init; } = [];
}