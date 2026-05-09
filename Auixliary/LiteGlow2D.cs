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
        public ModeType mode = ModeType.PlainAlpha;

        [SerializeField]
        public bool useTexture = false;

        [SerializeField, Range(0.0f, 3.0f)]
        public float smoothPower = 1.2f;

        [SerializeField]
        public bool invertSmooth = false;

        [SerializeField]
        public Sprite brushTextureSprite;

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
            if (!System.Enum.IsDefined(typeof(ModeType), mode))
                mode = ModeType.PlainAlpha;
        }
    }
}