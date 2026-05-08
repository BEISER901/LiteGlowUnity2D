using UnityEngine;
using System.Collections.Generic;

namespace com.BEISER901.liteglow2d {
    [ExecuteAlways]
    public class LiteGlow2D : MonoBehaviour
    {
        public static List<LiteGlow2D> Instances = new();

        public enum TypeRender
        {
            Sprite = 0,
            Current = 1,
            ChildGroup = 2
        }

        public TypeRender typeRender = TypeRender.Current;

        public float intensity = 1f;
        public Vector2 size = Vector2.one;
        public Vector2 offset = Vector2.zero;
        public float angle = 0f;
        public Color color = Color.white;

        public enum ModeType
        {
            PlainAlpha = 0,
            SmoothCenter = 1,
            Mask = 2
        }

        [SerializeField]
        private ModeType mode = ModeType.PlainAlpha;

        [SerializeField]
        private bool useTexture = false;

        [SerializeField, Range(0.1f, 2.0f)]
        private float glowRadius = 0.8f;

        [SerializeField, Range(1.0f, 10.0f)]
        private float glowSharpness = 4.0f;

        [SerializeField, Range(0.1f, 5f)]
        private float smoothStrength = 1.5f;

        [SerializeField, Range(0.0f, 1.0f)]
        private float smoothThreshold = 0.5f;

        [SerializeField, Range(0.0f, 3.0f)]
        private float smoothPower = 1.2f;

        [SerializeField]
        private bool invertSmooth = false;

        [SerializeField, Range(0.0f, 1.0f)]
        private float alphaCutoff = 0.5f;

        [SerializeField]
        private Sprite brushTextureSprite;

        public Texture2D BrushTexture => brushTextureSprite != null ? brushTextureSprite.texture : null;
        public float AlphaCutoff => alphaCutoff;

        public int Mode => (int)mode;

        public float GlowRadius
        {
            get => glowRadius;
            set => glowRadius = Mathf.Max(0.1f, value);
        }

        public float GlowSharpness
        {
            get => glowSharpness;
            set => glowSharpness = Mathf.Max(1.0f, value);
        }

        public bool UseTexture
        {
            get => useTexture;
            set => useTexture = value;
        }

        public float SmoothStrength => smoothStrength;
        public float SmoothThreshold => smoothThreshold;
        public float SmoothPower => smoothPower;
        public bool InvertSmooth => invertSmooth;

        public Sprite _spriteForRender;

        public SpriteRenderer[] spriteRenderers => typeRender switch
        {
            TypeRender.Current => GetComponent<SpriteRenderer>() != null ? new SpriteRenderer[] { GetComponent<SpriteRenderer>() } : System.Array.Empty<SpriteRenderer>(),
            TypeRender.ChildGroup => GetComponentsInChildren<SpriteRenderer>(),
            _ => GetComponent<SpriteRenderer>() != null ? new SpriteRenderer[] { GetComponent<SpriteRenderer>() } : System.Array.Empty<SpriteRenderer>()
        };

        private void OnEnable()
        {
            if (!Instances.Contains(this))
                Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        public Bounds GetBounds()
        {
            return new Bounds(transform.position + (Vector3)offset, size);
        }

        private void OnValidate()
        {
            glowRadius = Mathf.Max(0.1f, glowRadius);
            glowSharpness = Mathf.Max(1.0f, glowSharpness);
            smoothStrength = Mathf.Max(0.1f, smoothStrength);

            if (!System.Enum.IsDefined(typeof(ModeType), mode))
                mode = ModeType.PlainAlpha;
        }
    }
}