
using UnityEngine;

namespace CustomUtilities.Extensions
{
    public static class RectTransformExtension
    {
        public static void ReScaleImageIntoRectTransform(this RectTransform imageRect, RectTransform containerTransform)
        {

            var widthDifference = imageRect.rect.width - containerTransform.rect.width;
            var heightDifference = imageRect.rect.height - containerTransform.rect.height;

            if (widthDifference > 0 || heightDifference > 0)
            {
                if (heightDifference > widthDifference)
                {
                    imageRect.sizeDelta =
                        imageRect.rect.size * (containerTransform.rect.size.y / imageRect.rect.size.y);
                }
                else
                {
                    imageRect.sizeDelta =
                        imageRect.rect.size * (containerTransform.rect.size.x / imageRect.rect.size.x);
                }
            }
        }
    }
}
