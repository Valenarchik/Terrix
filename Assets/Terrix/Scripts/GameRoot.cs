using CustomUtilities.DataStructures;
using Terrix.Settings;
using UnityEngine;

namespace Terrix
{
    public class GameRoot: DontDestroyOnLoadMonoSingleton<GameRoot>
    {
        [SerializeField] private GameSettingsSO gameSettingsSo;
        public GameSettingsSO GameSettingsSo => gameSettingsSo;
    }
}