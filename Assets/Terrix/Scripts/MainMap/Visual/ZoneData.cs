using UnityEngine;

namespace Terrix.Visual
{
    public class ZoneData
    {
        public int PlayerId { get; }
        public Texture2D Texture { get; set; } = null;
        public Color? Color { get; set; } = null;
        public Vector2? TileSizeModifier { get; set; } = null;
        
        public ZoneData(int playerId)
        {
            PlayerId = playerId;
        }
    }
}