using UnityEngine;

namespace Terrix.Visual
{
    public class ZoneData
    {
        public int ID { get; }
        public Texture2D Texture { get; set; } = null;
        public Color? Color { get; set; } = null;
        public Vector2? TileSizeModifier { get; set; } = null;
        
        public ZoneData(int id)
        {
            ID = id;
        }
    }
}