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


        public HexData Get()
        {
            return new HexData(HexType, Income, Resist, CanCapture, IsSeeTile);
        }
    }
}