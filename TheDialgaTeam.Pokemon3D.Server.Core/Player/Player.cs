using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

public sealed class Player
{
    public int Id { get; }

    /// <summary>
    ///     Get Player DataItem[0]
    /// </summary>
    public string GameMode { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[1]
    /// </summary>
    public bool IsGameJoltPlayer { get; private set; }

    /// <summary>
    ///     Get Player DataItem[2]
    /// </summary>
    public string GameJoltId { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[3]
    /// </summary>
    public string DecimalSeparator { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[4]
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[5]
    /// </summary>
    public string LevelFile { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[6]
    /// </summary>
    public Position Position { get; private set; }

    /// <summary>
    ///     Get Player DataItem[7]
    /// </summary>
    public int Facing { get; private set; }

    /// <summary>
    ///     Get Player DataItem[8]
    /// </summary>
    public bool Moving { get; private set; }

    /// <summary>
    ///     Get Player DataItem[9]
    /// </summary>
    public string Skin { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[10]
    /// </summary>
    public BusyType BusyType { get; private set; }

    /// <summary>
    ///     Get Player DataItem[11]
    /// </summary>
    public bool PokemonVisible { get; private set; }

    /// <summary>
    ///     Get Player DataItem[12]
    /// </summary>
    public Position PokemonPosition { get; private set; }

    /// <summary>
    ///     Get/Set Player DataItem[13]
    /// </summary>
    public string PokemonSkin { get; private set; } = string.Empty;

    /// <summary>
    ///     Get/Set Player DataItem[14]
    /// </summary>
    public int PokemonFacing { get; private set; }

    public string DisplayStatus => IsGameJoltPlayer ? $"{Id}: {Name} ({GameJoltId}) - {BusyType}" : $"{Id}: {Name} - {BusyType}";

    public Player(int id)
    {
        Id = id;
    }

    public string[] Update(Package package)
    {
        if (package.IsFullPackageData())
        {
            GameMode = package.DataItems[0];
            IsGameJoltPlayer = int.Parse(package.DataItems[1]) > 0;
            GameJoltId = package.DataItems[2];
            DecimalSeparator = package.DataItems[3];
            Name = package.DataItems[4];
            LevelFile = package.DataItems[5];
            Position = new Position(package.DataItems[6], DecimalSeparator);
            Facing = int.Parse(package.DataItems[7]);
            Moving = int.Parse(package.DataItems[8]) > 0;
            Skin = package.DataItems[9];
            BusyType = Enum.Parse<BusyType>(package.DataItems[10]);
            PokemonVisible = int.Parse(package.DataItems[11]) > 0;
            PokemonPosition = new Position(package.DataItems[12], DecimalSeparator);
            PokemonSkin = package.DataItems[13];
            PokemonFacing = int.Parse(package.DataItems[14]);

            return package.DataItems;
        }

        var difference = new List<string>(new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }) { Capacity = 15 };

        if (LevelFile != package.DataItems[5])
        {
            LevelFile = package.DataItems[5];
            difference.Add(package.DataItems[5]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        if (Position.ToString() != package.DataItems[6])
        {
            Position = new Position(package.DataItems[6], DecimalSeparator);
            difference.Add(package.DataItems[6]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        var facing = int.Parse(package.DataItems[7]);

        if (Facing != facing)
        {
            Facing = facing;
            difference.Add(package.DataItems[7]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        var moving = int.Parse(package.DataItems[8]) > 0;

        if (Moving != moving)
        {
            Moving = moving;
            difference.Add(package.DataItems[8]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        if (Skin != package.DataItems[9])
        {
            Skin = package.DataItems[9];
            difference.Add(package.DataItems[9]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        var busyType = Enum.Parse<BusyType>(package.DataItems[10]);

        if (BusyType != busyType)
        {
            BusyType = busyType;
            difference.Add(package.DataItems[10]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        var pokemonVisible = int.Parse(package.DataItems[11]) > 0;

        if (PokemonVisible != pokemonVisible)
        {
            PokemonVisible = pokemonVisible;
            difference.Add(package.DataItems[11]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        var pokemonPosition = new Position(package.DataItems[12], DecimalSeparator);

        if (PokemonPosition.ToString() != package.DataItems[12])
        {
            PokemonPosition = pokemonPosition;
            difference.Add(package.DataItems[12]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        if (PokemonSkin != package.DataItems[13])
        {
            PokemonSkin = package.DataItems[13];
            difference.Add(package.DataItems[13]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        var pokemonFacing = int.Parse(package.DataItems[14]);

        if (PokemonFacing != pokemonFacing)
        {
            PokemonFacing = pokemonFacing;
            difference.Add(package.DataItems[14]);
        }
        else
        {
            difference.Add(string.Empty);
        }

        return difference.ToArray();
    }

    public string[] GenerateGameData()
    {
        return new[]
        {
            GameMode,
            IsGameJoltPlayer ? "1" : "0",
            GameJoltId,
            DecimalSeparator,
            Name,
            LevelFile,
            Position.ToString(),
            Facing.ToString(),
            Moving ? "1" : "0",
            Skin,
            ((int) BusyType).ToString(),
            PokemonVisible ? "1" : "0",
            PokemonPosition.ToString(),
            PokemonSkin,
            PokemonFacing.ToString()
        };
    }
}