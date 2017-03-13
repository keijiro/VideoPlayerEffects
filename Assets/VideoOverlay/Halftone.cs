using UnityEngine;
using UnityEngine.Video;

namespace VideoOverlayEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Halftone : MonoBehaviour
    {
        #region Editable properties

        [SerializeField] VideoPlayer _sourceVideo;
        [SerializeField] Texture _sourceTexture;
        [SerializeField] Color _backgroundColor = Color.white;
        [SerializeField] Color _fillColor = Color.black;
        [SerializeField] float _resolution = 50;
        [SerializeField] float _angle = 30;

        #endregion

        #region Private members

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;

        #endregion

        #region MonoBehaviour functions

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            if (_sourceVideo != null)
                _material.SetTexture("_SourceTex", _sourceVideo.texture);
            else if (_sourceTexture != null)
                _material.SetTexture("_SourceTex", _sourceTexture);
            else
                _material.SetTexture("_SourceTex", source);

            _material.SetColor("_BGColor", _backgroundColor);
            _material.SetColor("_Color", _fillColor);

            var cam = GetComponent<Camera>();
            var sin = Mathf.Sin(_angle);
            var cos = Mathf.Cos(_angle);

            var uv2grid = new Vector4( // Actually this is a 2x2 matrix.
                cam.aspect * cos, -sin, cam.aspect * sin, cos
            ) * _resolution;

            var grid2uv = new Vector4( // Actually this is a 2x2 matrix.
                cos / cam.aspect, sin / cam.aspect, -sin, cos
            ) / _resolution;

            _material.SetVector("_UV2Grid", uv2grid);
            _material.SetVector("_Grid2UV", grid2uv);
            _material.SetFloat("_Radius", cam.pixelHeight / _resolution);

            Graphics.Blit(source, destination, _material, 0);
        }

        #endregion
    }
}
