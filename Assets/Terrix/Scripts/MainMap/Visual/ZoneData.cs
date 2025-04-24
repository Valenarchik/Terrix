using UnityEngine;

namespace Terrix.Visual
{
    public class ZoneData
    {
        public int PlayerId { get; }
        public Texture2D Texture { get; set; } = null;
        public Color? Color { get; set; } = null;
        public Vector2? TileSizeModifier { get; set; } = null;
        public string PlayerName { get; set; }

        public Color? BorderColor { get; set; } = null;

        public ZoneData(int playerId)
        {
            PlayerId = playerId;
        }

        public ZoneData(int id, Color color, string playerName)
        {
            PlayerId = id;
            var mainColor = color;
            mainColor.a = 0.3f;
            Color = mainColor;
            BorderColor = color;
            PlayerName = playerName;
        }
    }
}