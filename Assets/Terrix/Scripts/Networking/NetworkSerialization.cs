using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Serializing;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Map;
using Terrix.Network.DTO;
using Terrix.Settings;
using Terrix.Visual;
using UnityEngine;

namespace Terrix.Networking
{
    public static class NetworkSerialization
    {
        public static void WriteHexMap(this Writer writer, HexMap value)
        {
            writer.Write(value.Hexes.ToArray());
            writer.WriteVector3Int(value.Size);
        }

        public static HexMap ReadHexMap(this Reader reader)
        {
            var hexes = reader.Read<Hex[]>();
            var size = reader.ReadVector3Int();
            var matrix = hexes.ToMatrix(size.x, size.y, size.z);
            return new HexMap(matrix);
        }

        public static void WriteHex(this Writer writer, Hex value)
        {
            writer.Write(value.HexType);
            writer.WriteVector3Int(value.Position);
            writer.WriteVector3(value.WorldPosition);
            writer.Write(value.PlayerId);
        }

        public static Hex ReadHex(this Reader reader)
        {
            return new Hex(reader.Read<HexType>(), reader.ReadVector3Int(), reader.ReadVector3(),
                reader.Read<int?>());
        }

        public static void WriteCountry(this Writer writer, Country value)
        {
            writer.WriteList(value.Cells.ToList());
            writer.Write(value.Population);
            writer.WriteInt32(value.TotalCellsCount);
            writer.Write(value.DensePopulation);
        }

        public static Country ReadCountry(this Reader reader)
        {
            return new Country(
                reader.ReadListAllocated<Hex>(), reader.Read<float>(), reader.ReadInt32(), reader.Read<float>());
        }

        public static void WriteIGameDataProvider(this Writer writer, IGameDataProvider value)
        {
            if (value is GameDataProvider gameDataProvider)
            {
                writer.WriteUInt8Unpacked(1);
                writer.Write(gameDataProvider);
            }
        }

        public static IGameDataProvider ReadIGameDataProvider(this Reader reader)
        {
            var classType = reader.ReadUInt8Unpacked();
            if (classType == 1)
            {
                return reader.Read<GameDataProvider>();
            }

            return default;
        }

        public static void WritePlayer(this Writer writer, Player value)
        {
            var isInitialized = value is not null;
            writer.WriteBoolean(isInitialized);
            if (isInitialized)
            {
                if (value is Bot bot)
                {
                    writer.WriteUInt8Unpacked(1);
                    writer.Write(bot);
                }
                else
                {
                    writer.WriteUInt8Unpacked(2);
                    writer.WriteInt32(value.ID);
                    writer.Write(value.PlayerType);
                    writer.Write(value.Country);
                    writer.WriteString(value.PlayerName);
                    writer.WriteColor(value.PlayerColor);
                }
            }
        }

        public static Player ReadPlayer(this Reader reader)
        {
            if (!reader.ReadBoolean())
            {
                return null;
            }

            var classType = reader.ReadUInt8Unpacked();
            if (classType == 1)
            {
                return reader.Read<Bot>();
            }

            var player = new Player(reader.ReadInt32(), reader.Read<PlayerType>(), reader.Read<Country>(),
                reader.ReadString(), reader.ReadColor());
            player.Country.Owner = player;
            return player;
        }

        public static void WriteBot(this Writer writer, Bot value)
        {
            var isInitialized = value is not null;
            writer.WriteBoolean(isInitialized);
            if (isInitialized)
            {
                writer.WriteInt32(value.ID);
                writer.Write(value.PlayerType);
                writer.Write(value.Country);
                writer.WriteString(value.PlayerName);
                writer.WriteColor(value.PlayerColor);
            }
        }

        public static Bot ReadBot(this Reader reader)
        {
            if (!reader.ReadBoolean())
            {
                return null;
            }

            var player = new Bot(reader.ReadInt32(), reader.Read<PlayerType>(), reader.Read<Country>(),
                reader.ReadString(), reader.ReadColor());
            player.Country.Owner = player;
            return player;
        }


        public static void WriteUpdateCellsData(this Writer writer, Country.UpdateCellsData value)
        {
            writer.WriteInt32(value.PlayerId);
            writer.Write(value.ChangeData);
        }

        public static Country.UpdateCellsData ReadUpdateCellsData(this Reader reader)
        {
            return new Country.UpdateCellsData(reader.ReadInt32(), reader.Read<Country.CellChangeData[]>());
        }

        public static void WriteAllCountriesDrawerSettings(this Writer writer, AllCountriesDrawer.Settings value)
        {
            writer.Write(value.Zones);
            writer.Write(value.DragZone);
        }

        public static AllCountriesDrawer.Settings ReadAllCountriesDrawerSettings(this Reader reader)
        {
            return new AllCountriesDrawer.Settings(reader.Read<ZoneData[]>(), reader.Read<ZoneData>());
        }

        public static void WriteZoneData(this Writer writer, ZoneData value)
        {
            writer.WriteInt32(value.PlayerId);
            writer.WriteString(value.PlayerName);
            var hasColor = value.Color is not null;
            writer.WriteBoolean(hasColor);
            if (hasColor)
            {
                writer.Write((Color)value.Color);
            }
        }

        public static ZoneData ReadZoneData(this Reader reader)
        {
            var id = reader.ReadInt32();
            var playerName = reader.ReadString();
            var hasColor = reader.ReadBoolean();

            if (hasColor)
            {
                return new ZoneData(id, reader.Read<Color>(), playerName);
            }

            return new ZoneData(id);
        }

        public static T[,] ToMatrix<T>(this T[] array, int width, int height)
        {
            if (array.Length != width * height)
            {
                throw new Exception("Data is invalid");
            }

            var matrix = new T[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[x, y] = array[y * width + x];
                }
            }

            return matrix;
        }

        public static T[,,] ToMatrix<T>(this T[] array, int width, int height, int length)
        {
            if (array.Length != width * height * length)
            {
                throw new Exception("Data is invalid");
            }

            var matrix = new T[width, height, length];
            for (int z = 0; z < length; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        matrix[x, y, z] = array[z * height + y * width + x];
                    }
                }
            }

            return matrix;
        }

        public static T[] ToArray<T>(this T[,,] matrix)
        {
            var result = new List<T>();
            for (var z = 0; z < matrix.GetLength(2); z++)
            {
                for (var y = 0; y < matrix.GetLength(1); y++)
                {
                    for (var x = 0; x < matrix.GetLength(0); x++)
                    {
                        result.Add(matrix[x, y, z]);
                    }
                }
            }

            return result.ToArray();
        }

        public class PlayersCountryMapData
        {
            public IPlayersProvider IPlayersProvider;
            public HexMap HexMap;

            public PlayersCountryMapData(IPlayersProvider playersProvider, HexMap hexMap)
            {
                IPlayersProvider = playersProvider;
                HexMap = hexMap;
            }
        }

        public static void WritePlayersCountryMapData(this Writer writer,
            PlayersCountryMapData value)
        {
            var map = value.HexMap;
            writer.WriteInt32(map.Hexes.Length);
            writer.WriteVector3Int(map.Size);
            foreach (var hex in map.Hexes.ToArray())
            {
                WriteHex(hex);
            }

            var players = value.IPlayersProvider.GetAll().ToArray();
            writer.WriteInt32(players.Length);
            foreach (var player in players)
            {
                writer.WriteBoolean(player is null);
                if (player is not null)
                {
                    if (player is Bot bot)
                    {
                        writer.WriteUInt8Unpacked(1);
                        writer.WriteInt32(bot.ID);
                        writer.Write(bot.PlayerType);
                        var country = bot.Country;
                        var hexes = country.Cells;
                        writer.WriteInt32(hexes.Count());

                        foreach (var hex in hexes)
                        {
                            // writer.Write(hex.HexType);
                            writer.WriteVector3Int(hex.Position);
                            // writer.WriteVector3(hex.WorldPosition);
                            // writer.Write(hex.PlayerId);
                            // writer.Write(hex.Players); //TODO брать 
                        }

                        // writer.WriteList(country.Cells.ToList());
                        writer.Write(country.Population);
                        writer.WriteInt32(country.TotalCellsCount);
                        writer.Write(country.DensePopulation);
                        writer.WriteString(bot.PlayerName);
                        writer.WriteColor(bot.PlayerColor);
                    }
                    else
                    {
                        writer.WriteUInt8Unpacked(2);
                        writer.WriteInt32(player.ID);
                        writer.Write(player.PlayerType);
                        var country = player.Country;

                        var hexes = country.Cells;
                        writer.WriteInt32(hexes.Count());
                        foreach (var hex in hexes)
                        {
                            writer.WriteVector3Int(hex.Position);

                            // WriteHex(hex);
                            // writer.Write(hex.Players); //TODO брать 
                        }

                        // writer.WriteList(country.Cells.ToList());
                        writer.Write(country.Population);
                        writer.WriteInt32(country.TotalCellsCount);
                        writer.Write(country.DensePopulation);
                        writer.WriteString(player.PlayerName);
                        writer.WriteColor(player.PlayerColor);
                    }
                }
            }

            void WriteHex(Hex hex)
            {
                writer.Write(hex.HexType);
                writer.WriteVector3Int(hex.Position);
                writer.WriteVector3(hex.WorldPosition);
                writer.Write(hex.PlayerId);
            }
            // writer.Write((PlayersProvider)value.IPlayersProvider);
        }

        public static PlayersCountryMapData ReadPlayersCountryMapData(this Reader reader)
        {
            var playersProvider = new PlayersProvider(new List<Player>());
            var mapLength = reader.ReadInt32();
            var mapSize = reader.ReadVector3Int();
            var hexes = new Hex[mapLength];
            for (int i = 0; i < mapLength; i++)
            {
                var hex = GetHex();
                hexes[i] = hex;
            }

            var hexMap = new HexMap(hexes.ToMatrix(mapSize.x, mapSize.y, mapSize.z));

            var playersCount = reader.ReadInt32();
            for (int i = 0; i < playersCount; i++)
            {
                if (reader.ReadBoolean())
                {
                    continue;
                }

                var classType = reader.ReadUInt8Unpacked();
                if (classType == 1)
                {
                    var bot = new Bot(reader.ReadInt32(), reader.Read<PlayerType>(), GetCountry(),
                        reader.ReadString(), reader.ReadColor());
                    bot.Country.Owner = bot;
                    playersProvider.AddPlayer(bot);
                }
                else
                {
                    var player = new Player(reader.ReadInt32(), reader.Read<PlayerType>(), GetCountry(),
                        reader.ReadString(), reader.ReadColor());
                    player.Country.Owner = player;
                    playersProvider.AddPlayer(player);
                }
            }

            return new PlayersCountryMapData(playersProvider, hexMap);

            Country GetCountry()
            {
                var hexes = new Hex[reader.ReadInt32()];
                for (int i = 0; i < hexes.Count(); i++)
                {
                    hexes[i] = GetHexFormMap();
                }

                var population = reader.Read<float>();
                var totalCellsCount = reader.ReadInt32();
                var densePopulation = reader.Read<float>();
                return new Country(hexes, population, totalCellsCount, densePopulation);
            }

            Hex GetHex()
            {
                var hexType = reader.Read<HexType>();
                var position = reader.ReadVector3Int();
                var worldPosition = reader.ReadVector3();
                var id = reader.Read<int?>();
                return new Hex(hexType, position, worldPosition, id, playersProvider);
            }

            Hex GetHexFormMap()
            {
                var position = reader.ReadVector3Int();
                return hexMap.FindHex(position);
            }
        }

        public static void WriteAttackMessage(this Writer writer, AttackMessage value)
        {
            writer.Write(value.ID);
            writer.WriteInt32(value.Owner);
            writer.Write(value.Target);
            writer.Write(value.Points);
            writer.Write(value.Territory);
            writer.WriteBoolean(value.IsGlobalAttack);
        }

        public static AttackMessage ReadAttackMessage(this Reader reader)
        {
            return new AttackMessage(reader.Read<Guid>(), reader.ReadInt32(), reader.Read<int?>(), reader.Read<float>(),
                reader.Read<Vector3Int[]>(), reader.ReadBoolean());
        }

        public static void WriteSimplifiedCountry(this Writer writer, SimplifiedCountry value)
        {
            writer.WriteInt32(value.PlayerId);
            writer.Write(value.Population);
            writer.WriteInt32(value.CellsCount);
        }

        public static SimplifiedCountry ReadSimplifiedCountry(this Reader reader)
        {
            return new SimplifiedCountry(reader.ReadInt32(), reader.Read<float>(), reader.ReadInt32());
        }

        public static void WriteSimplifiedHex(this Writer writer, SimplifiedHex value)
        {
            writer.WriteVector3Int(value.Position);
            writer.Write(value.PlayerId);
        }

        public static SimplifiedHex ReadSimplifiedHex(this Reader reader)
        {
            return new SimplifiedHex(reader.ReadVector3Int(), reader.Read<int?>());
        }

        public static void WriteUpdateSimplifiedCellsData(this Writer writer, UpdateSimplifiedCellsData value)
        {
            writer.WriteInt32(value.PlayerId);
            writer.WriteList(value.ChangeData);
        }

        public static UpdateSimplifiedCellsData ReadUpdateSimplifiedCellsData(this Reader reader)
        {
            return new UpdateSimplifiedCellsData(reader.ReadInt32(),
                reader.ReadListAllocated<SimplifiedCellChangeData>());
        }

        public static void WriteSimplifiedCellChangeData(this Writer writer, SimplifiedCellChangeData value)
        {
            writer.WriteVector3Int(value.Position);
            writer.Write(value.Mode);
        }

        public static SimplifiedCellChangeData ReadSimplifiedCellChangeData(this Reader reader)
        {
            return new SimplifiedCellChangeData(reader.ReadVector3Int(), reader.Read<Country.UpdateCellMode>());
        }

        public static void WriteLobbySettings(this Writer writer, LobbySettings value)
        {
            writer.WriteInt32(value.PlayersCount);
            writer.WriteInt32(value.BotsCount);
        }

        public static LobbySettings ReadLobbySettings(this Reader reader)
        {
            return new LobbySettings(reader.ReadInt32(), reader.ReadInt32());
        }
    }
}