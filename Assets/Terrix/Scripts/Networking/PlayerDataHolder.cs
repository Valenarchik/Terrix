using UnityEngine;

namespace Terrix.Networking
{
    public static class PlayerDataHolder
    {
        // public static PlayerDataHolder Instance { get; private set; }
        //
        // private void Awake() => Instance = this;

        public static Color Color { get; set; }
        public static string PlayerName { get; set; }

        public static void SetData(Color color, string playerName)
        {
            Color = color;
            PlayerName = playerName;
        }
    }
}