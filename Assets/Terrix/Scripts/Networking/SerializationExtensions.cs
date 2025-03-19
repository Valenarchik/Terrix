using System;
using System.Linq;
using FishNet.Serializing;
using Terrix.DTO;
using Terrix.Entities;
using Terrix.Map;
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