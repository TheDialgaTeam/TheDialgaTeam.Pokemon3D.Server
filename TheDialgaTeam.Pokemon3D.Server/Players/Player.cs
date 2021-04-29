using System;
using System.Collections.Generic;
using TheDialgaTeam.Pokemon3D.Server.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class Player
    {
        private string _gameMode = string.Empty;
        private bool _isGameJoltPlayer;
        private int _gameJoltId;
        private string _decimalSeparator = string.Empty;
        private string _name = string.Empty;
        private string _levelFile = string.Empty;
        private Position _position;
        private int _facing;
        private bool _moving;
        private string _skin = string.Empty;
        private BusyType _busyType;
        private bool _pokemonVisible;
        private Position _pokemonPosition;
        private string _pokemonSkin = string.Empty;
        private int _pokemonFacing;

        private IReadOnlyList<string>? _lastValidPlayerData;

        public int Id { get; }

        public PlayerNetwork PlayerNetwork { get; }

        /// <summary>
        /// Get Player DataItem[0]
        /// </summary>
        public string GameMode
        {
            get => _gameMode;
            private set => _gameMode = value;
        }

        /// <summary>
        /// Get Player DataItem[1]
        /// </summary>
        public bool IsGameJoltPlayer
        {
            get => _isGameJoltPlayer;
            private set => _isGameJoltPlayer = value;
        }

        /// <summary>
        /// Get Player DataItem[2]
        /// </summary>
        public int GameJoltId
        {
            get => _gameJoltId;
            private set => _gameJoltId = value;
        }

        /// <summary>
        /// Get Player DataItem[3]
        /// </summary>
        public string DecimalSeparator
        {
            get => _decimalSeparator;
            private set => _decimalSeparator = value;
        }

        /// <summary>
        /// Get Player DataItem[4]
        /// </summary>
        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        /// <summary>
        /// Get Player DataItem[5]
        /// </summary>
        public string LevelFile
        {
            get => _levelFile;
            private set => _levelFile = value;
        }

        /// <summary>
        /// Get Player DataItem[6]
        /// </summary>
        public Position Position
        {
            get => _position;
            private set => _position = value;
        }

        /// <summary>
        /// Get Player DataItem[7]
        /// </summary>
        public int Facing
        {
            get => _facing;
            private set => _facing = value;
        }

        /// <summary>
        /// Get Player DataItem[8]
        /// </summary>
        public bool Moving
        {
            get => _moving;
            private set => _moving = value;
        }

        /// <summary>
        /// Get Player DataItem[9]
        /// </summary>
        public string Skin
        {
            get => _skin;
            private set => _skin = value;
        }

        /// <summary>
        /// Get Player DataItem[10]
        /// </summary>
        public BusyType BusyType
        {
            get => _busyType;
            private set => _busyType = value;
        }

        /// <summary>
        /// Get Player DataItem[11]
        /// </summary>
        public bool PokemonVisible
        {
            get => _pokemonVisible;
            private set => _pokemonVisible = value;
        }

        /// <summary>
        /// Get Player DataItem[12]
        /// </summary>
        public Position PokemonPosition
        {
            get => _pokemonPosition;
            private set => _pokemonPosition = value;
        }

        /// <summary>
        /// Get/Set Player DataItem[13]
        /// </summary>
        public string PokemonSkin
        {
            get => _pokemonSkin;
            private set => _pokemonSkin = value;
        }

        /// <summary>
        /// Get/Set Player DataItem[14]
        /// </summary>
        public int PokemonFacing
        {
            get => _pokemonFacing;
            private set => _pokemonFacing = value;
        }

        public Player(int id, PlayerNetwork playerNetwork)
        {
            Id = id;
            PlayerNetwork = playerNetwork;
        }

        private static string Update<T>(ref T variable, T value)
        {
            if (variable?.Equals(value) ?? false) return string.Empty;
            variable = value;

            return value switch
            {
                bool boolValue => boolValue ? "1" : "0",
                Position positionValue => positionValue.ToString(),
                BusyType busyTypeValue => ((int) busyTypeValue).ToString(),
                var _ => value?.ToString() ?? string.Empty
            };
        }

        public void Update(Package package)
        {
            if (package.IsFullPackageData())
            {
                GameMode = package.DataItems[0];
                IsGameJoltPlayer = int.Parse(package.DataItems[1]) > 0;
                GameJoltId = int.Parse(package.DataItems[2]);
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

                _lastValidPlayerData = package.DataItems;
            }
            else
            {
                var difference = new List<string>
                {
                    Update(ref _gameMode, package.DataItems[0]),
                    Update(ref _isGameJoltPlayer, int.Parse(package.DataItems[1]) > 0),
                    Update(ref _gameJoltId, int.Parse(package.DataItems[2])),
                    Update(ref _decimalSeparator, package.DataItems[3]),
                    Update(ref _name, package.DataItems[4]),
                    Update(ref _levelFile, package.DataItems[5]),
                    Update(ref _position, new Position(package.DataItems[6], DecimalSeparator)),
                    Update(ref _facing, int.Parse(package.DataItems[7])),
                    Update(ref _moving, int.Parse(package.DataItems[8]) > 0),
                    Update(ref _skin, package.DataItems[9]),
                    Update(ref _busyType, Enum.Parse<BusyType>(package.DataItems[10])),
                    Update(ref _pokemonVisible, int.Parse(package.DataItems[11]) > 0),
                    Update(ref _pokemonPosition, new Position(package.DataItems[12], DecimalSeparator)),
                    Update(ref _pokemonSkin, package.DataItems[13]),
                    Update(ref _pokemonFacing, int.Parse(package.DataItems[14]))
                };

                // TODO: Send all data to everyone.

                _lastValidPlayerData = package.DataItems;
            }
        }
    }
}