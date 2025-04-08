using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Serializing;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Game.GameRules;
using Terrix.Map;
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
            writer.Write(value.CanCaptureHexes.ToArray());
            writer.WriteVector3Int(value.Size);
        }

        public static HexMap ReadHexMap(this Reader reader)
        {
            var hexes = reader.Read<Hex[]>();
            var canCaptureHexes = reader.Read<Hex[]>();
            var size = reader.ReadVector3Int();
            var matrix = hexes.ToMatrix(size.x, size.y, size.z);
            return new HexMap(matrix, canCaptureHexes);
        }

        public static void WriteHex(this Writer writer, Hex value)
        {
            writer.Write(value.HexType);
            writer.WriteVector3Int(value.Position);
            writer.Write(value.NeighboursPositions);
            writer.Write(value.PlayerId);
        }

        public static Hex ReadHex(this Reader reader)
        {
            return new Hex(reader.Read<HexType>(), reader.ReadVector3Int(), reader.Read<Vector3Int[]>(),
                reader.Read<int?>());
        }

        public static void WriteHexData(this Writer writer, HexData value)
        {
            writer.Write(value.HexType);
            writer.Write(value.Income);
            writer.Write(value.Resist);
            writer.Write(value.CanCapture);
            writer.Write(value.IsSeeTile);
        }

        public static HexData ReadHexData(this Reader reader)
        {
            return new HexData(reader.Read<HexType>(), reader.Read<float>(), reader.Read<float>(), reader.ReadBoolean(),
                reader.ReadBoolean());
        }

        public static void WriteCountry(this Writer writer, Country value)
        {
            writer.Write(value.GameDataProvider);
            writer.WriteList(value.CellsSet.ToList());
            writer.Write(value.Population);
            writer.WriteInt32(value.TotalCellsCount);
            writer.Write(value.DensePopulation);
            // writer.Write(value.Map);
            // writer.Write(value.Owner);
        }

        public static Country ReadCountry(this Reader reader)
        {
            // return new Country(reader.Read<IGameDataProvider>(), reader.Read<Player>());
            return new Country(reader.Read<IGameDataProvider>(),
                reader.ReadListAllocated<Hex>(), reader.Read<float>(), reader.ReadInt32(), reader.Read<float>());
            // reader.Read<HexMap>())
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

        public static void WriteIPlayersProvider(this Writer writer, IPlayersProvider value)
        {
            if (value is PlayersProvider playersProvider)
            {
                writer.WriteUInt8Unpacked(1);
                writer.Write(playersProvider);
            }
        }

        public static IPlayersProvider ReadIPlayersProvider(this Reader reader)
        {
            var classType = reader.ReadUInt8Unpacked();
            if (classType == 1)
            {
                return reader.Read<PlayersProvider>();
            }

            return default;
        }

        public static void WritePlayersProvider(this Writer writer, PlayersProvider value)
        {
            writer.WriteList(value.Players);
        }

        public static PlayersProvider ReadPlayersProvider(this Reader reader)
        {
            return new PlayersProvider(reader.ReadListAllocated<Player>());
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
    }
}