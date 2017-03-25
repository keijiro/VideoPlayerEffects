using UnityEngine;

namespace Voxelizer
{
    [RequireComponent(typeof(Camera))]
    public class PostEffect : MonoBehaviour
    {
        #region Editable properties

        [SerializeField, ColorUsage(false)] Color _fillColor = Color.black;
        [SerializeField, ColorUsage(false)] Color _lineColor = Color.white;

        public Color fillColor { set { _fillColor = value; } }
        public Color lineColor { set { _lineColor = value; } }

        #endregion

        #region Private fields

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnDestroy()
        {
            Destroy(_material);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetColor("_FillColor", _fillColor);
            _material.SetColor("_LineColor", _lineColor);
            Graphics.Blit(source, destination, _material, 0);
        }

        #endregion
    }
}
