using System;
using System.Collections.Generic;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class Player
    {
        private int _isGameJoltPlayer;
        private string _position = null!;
        private int _moving;
        private int _busyType;
        private int _pokemonVisible;
        private string _pokemonPosition = null!;

        private IReadOnlyList<string> _lastValidPlayerData = null!;

        public int Id { get; }

        public PlayerNetwork PlayerNetwork { get; }

        /// <summary>
        /// Get Player DataItem[0]
        /// </summary>
        public string GameMode { get; private set; } = null!;

        /// <summary>
        /// Get Player DataItem[1]
        /// </summary>
        public bool IsGameJoltPlayer
        {
            get => _isGameJoltPlayer > 0;
            private set => _isGameJoltPlayer = value ? 1 : 0;
        }

        /// <summary>
        /// Get Player DataItem[2]
        /// </summary>
        public int GameJoltId { get; private set; }

        /// <summary>
        /// Get Player DataItem[3]
        /// </summary>
        public string DecimalSeparator { get; private set; } = null!;

        /// <summary>
        /// Get Player DataItem[4]
        /// </summary>
        public string Name { get; private set; } = null!;

        /// <summary>
        /// Get Player DataItem[5]
        /// </summary>
        public string LevelFile { get; private set; } = null!;

        /// <summary>
        /// Get Player DataItem[6]
        /// </summary>
        public Position Position
        {
            get => new(_position, DecimalSeparator);
            private set => _position = value.ToString();
        }

        /// <summary>
        /// Get Player DataItem[7]
        /// </summary>
        public int Facing { get; private set; }

        /// <summary>
        /// Get Player DataItem[8]
        /// </summary>
        public bool Moving
        {
            get => _moving > 0;
            private set => _moving = value ? 1 : 0;
        }

        /// <summary>
        /// Get Player DataItem[9]
        /// </summary>
        public string Skin { get; private set; } = null!;

        /// <summary>
        /// Get Player DataItem[10]
        /// </summary>
        public BusyType BusyType
        {
            get => Enum.Parse<BusyType>(_busyType.ToString());
            private set => _busyType = (int) value;
        }

        /// <summary>
        /// Get Player DataItem[11]
        /// </summary>
        public bool PokemonVisible
        {
            get => _pokemonVisible > 0;
            private set => _pokemonVisible = value ? 1 : 0;
        }

        /// <summary>
        /// Get Player DataItem[12]
        /// </summary>
        public Position PokemonPosition
        {
            get => new(_pokemonPosition, DecimalSeparator);
            private set => _pokemonPosition = value.ToString();
        }

        /// <summary>
        /// Get/Set Player DataItem[13]
        /// </summary>
        public string PokemonSkin { get; private set; } = null!;

        /// <summary>
        /// Get/Set Player DataItem[14]
        /// </summary>
        public int PokemonFacing { get; private set; }

        public Player(int id, PlayerNetwork playerNetwork)
        {
            Id = id;
            PlayerNetwork = playerNetwork;
        }
    }
}