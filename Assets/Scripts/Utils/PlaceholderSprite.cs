using UnityEngine;

namespace CleanPlanet.Utils
{
    public static class PlaceholderSprite
    {
        private static Sprite _sprite;

        public static void AssignIfMissing(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer.sprite != null)
            {
                return;
            }

            if (_sprite == null)
            {
                _sprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f);
                _sprite.name = "Runtime Placeholder Square";
                _sprite.hideFlags = HideFlags.HideAndDontSave;
            }

            spriteRenderer.sprite = _sprite;
        }
    }
}
