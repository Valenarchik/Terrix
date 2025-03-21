using System;
using FishNet.Serializing;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Networking
{
    public static class SerializationExtensions
    {
        public static void WriteHexMap(this Writer writer, HexMap value)
        {
            writer.Write(value.Hexes);
            writer.WriteVector2Int(value.Size);
        }

        public static HexMap ReadHexMap(this Reader reader)
        {
            var hexes = reader.Read<Hex[]>();
            var size = reader.ReadVector2Int();
            var matrix = hexes.ToMatrix(size.x, size.y);
            return new HexMap(matrix);
        }

        public static void WriteHex(this Writer writer, Hex value)
        {
            writer.Write(value.Data);
            writer.Write(value.Position);
            writer.Write(value.NeighboursPositions);
            writer.Write(value.Owner);
        }

        public static Hex ReadHex(this Reader reader)
        {
            return new Hex(reader.Read<HexData>(), reader.ReadVector2Int(), reader.Read<Vector2Int[]>(),
                reader.Read<Player>());
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
            writer.Write(value.Owner);
        }

        public static Country ReadCountry(this Reader reader)
        {
            return new Country(reader.Read<IGameDataProvider>(), reader.Read<Player>());
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
                writer.Write(value.PlayerType);
                writer.Write(value.Country);
            }
        }

        public static Player ReadPlayer(this Reader reader)
        {
            if (!reader.ReadBoolean())
            {
                return null;
            }

            return new Player(reader.Read<PlayerType>(), reader.Read<Country>());
        }
        public static void WriteActionBool(this Writer writer, Country value)
        {
            writer.Write(value.GameDataProvider);
            writer.Write(value.Owner);
        }

        public static Country ReadActionBool(this Reader reader)
        {
            return new Country(reader.Read<IGameDataProvider>(), reader.Read<Player>());
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
    }
}