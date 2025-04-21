using UnityEngine;

namespace Terrix.Visual
{
    public class ZoneMaterialFactory: MonoBehaviour
    {
        private static readonly int baseTextureId = Shader.PropertyToID("_BaseTexture");
        private static readonly int baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int tileSizeModifierId = Shader.PropertyToID("_TileSizeModifier");
        private static readonly int borderColorId = Shader.PropertyToID("_BorderColor");
        
        [SerializeField] private Material defaultZoneMaterial;

        public Material Create(ZoneData data)
        {
            var material = new Material(defaultZoneMaterial);

            if (data.Texture != null)
            {
                material.SetTexture(baseTextureId, data.Texture);
            }

            if (data.Color != null)
            {
                material.SetColor(baseColorId, data.Color.Value);
            }

            if (data.TileSizeModifier != null)
            {
                material.SetVector(tileSizeModifierId, data.TileSizeModifier.Value);
            }

            if (data.BorderColor != null)
            {
                material.SetColor(borderColorId, data.BorderColor.Value);
            }

            return material;
        }
    }
}