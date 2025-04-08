using System;
using Terrix.DTO;

namespace Terrix.Settings
{
    [Serializable]
    public class HexDataSerializable
    {
        public HexType HexType;
        public float Income;
        public float Resist;
        public bool CanCapture;
        public bool IsSeeTile;


        public GameHexData Get()
        {
            return new GameHexData(HexType, Income, Resist, CanCapture, IsSeeTile);
        }
    }
}