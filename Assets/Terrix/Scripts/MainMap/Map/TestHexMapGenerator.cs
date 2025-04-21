using CustomUtilities.Attributes;
using Terrix.Settings;
using UnityEngine;

namespace Terrix.Map
{
    public class TestHexMapGenerator: MonoBehaviour
    {
        [SerializeField] private HexMapGenerator mapGenerator;
        [SerializeField] private HexMapRenderer mapRenderer;
        [SerializeField] private HexMapGeneratorSettingsSO settings;

        [EditorButton]
        public void Draw()
        {
            mapGenerator.InitializeMocks();
            mapRenderer.RenderMap(mapGenerator.GenerateMap(settings.Get()));
        }
    }
}