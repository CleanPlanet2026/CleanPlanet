using UnityEngine;

namespace CleanPlanet.Map
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ExplorationPortalVisual : MonoBehaviour
    {
        private const int TextureSize = 64;
        private const float BaseScale = 0.9f;

        private static readonly Color PortalColor = new(0.15f, 0.95f, 0.9f, 1f);
        private static Sprite _portalSprite;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = GetOrCreateSprite();
            _spriteRenderer.color = PortalColor;
            _spriteRenderer.sortingOrder = 20;
        }

        private void Update()
        {
            float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
            float scale = Mathf.Lerp(BaseScale, 1.1f, pulse);
            transform.localScale = new Vector3(scale, scale, 1f);

            Color color = PortalColor;
            color.a = Mathf.Lerp(0.55f, 0.95f, pulse);
            _spriteRenderer.color = color;
        }

        private static Sprite GetOrCreateSprite()
        {
            if (_portalSprite != null)
            {
                return _portalSprite;
            }

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = "Runtime Portal Marker",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color32[TextureSize * TextureSize];
            Vector2 center = new((TextureSize - 1) * 0.5f, (TextureSize - 1) * 0.5f);
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float ring = 1f - Mathf.Clamp01(Mathf.Abs(distance - 22f) / 4f);
                    float glow = (1f - Mathf.Clamp01(Mathf.Abs(distance - 22f) / 10f)) * 0.35f;
                    byte alpha = (byte)(Mathf.Clamp01(Mathf.Max(ring, glow)) * 255f);
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _portalSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            _portalSprite.name = "Runtime Portal Marker";
            _portalSprite.hideFlags = HideFlags.HideAndDontSave;
            return _portalSprite;
        }
    }
}
