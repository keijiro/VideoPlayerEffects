using UnityEngine;

namespace VideoEffects.Voxelizer
{
    [ExecuteInEditMode]
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

        void OnDestroy()
        {
            if (_material != null)
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            _material.SetColor("_FillColor", _fillColor);
            _material.SetColor("_LineColor", _lineColor);

            Graphics.Blit(source, destination, _material, 0);
        }

        #endregion
    }
}
