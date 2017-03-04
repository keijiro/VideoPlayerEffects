using UnityEngine;
using UnityEngine.Video;

namespace VideoPlayerEffects
{
    public class VideoKeying : MonoBehaviour
    {
        #region Editable properties

        [SerializeField] VideoPlayer _sourceVideo;
        [SerializeField] RenderTexture _sourceTexture;

        [Header("Matte Generation")]
        [SerializeField, ColorUsage(false)] Color _keyColor = Color.green;
        [SerializeField, Range(0, 1)] float _threshold = 0.5f;
        [SerializeField, Range(0, 1)] float _tolerance = 0.2f;

        [Header("Spill Removal")]
        [SerializeField, Range(0, 1)] float _spillRemoval = 0.5f;
        [SerializeField, Range(0, 1)] float _desaturate = 0.2f;

        [Space]
        [SerializeField] bool _debug;

        #endregion

        #region Private assets and objects

        [SerializeField, HideInInspector] Shader _shader;
        [SerializeField, HideInInspector] Mesh _quadMesh;

        Material _material;
        RenderTexture _buffer;

        #endregion

        #region Private functions

        Vector3 RGB2YCgCo(Color rgb)
        {
            return new Vector3(
                 0.25f * rgb.r + 0.5f * rgb.g + 0.25f * rgb.b,
                -0.25f * rgb.r + 0.5f * rgb.g - 0.25f * rgb.b,
                 0.50f * rgb.r                - 0.50f * rgb.b
            );
        }

        Vector3 YCgCo2RGB(Vector3 ycgco)
        {
            return new Vector3(
                ycgco.x - ycgco.y + ycgco.z,
                ycgco.x + ycgco.y,
                ycgco.x - ycgco.y - ycgco.z
            );
        }

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnDestroy()
        {
            if (_buffer != null) RenderTexture.ReleaseTemporary(_buffer);
        }

        void Update()
        {
            if (_buffer != null) RenderTexture.ReleaseTemporary(_buffer);

            var source = _sourceVideo != null ? _sourceVideo.texture : _sourceTexture;
            if (source == null) return;

            var rt1 = RenderTexture.GetTemporary(source.width, source.height);
            var rt2 = RenderTexture.GetTemporary(source.width, source.height);

            // Keying
            var ycgco = RGB2YCgCo(_keyColor);
            _material.SetVector("_CgCo", new Vector2(ycgco.y, ycgco.z));
            _material.SetVector("_Matte", new Vector2(
                _threshold * 0.1f, (_threshold + _tolerance) * 0.1f
            ));
            _material.SetVector("_Spill", new Vector2(
                _spillRemoval, 1 - _desaturate
            ));
            Graphics.Blit(source, rt1, _material, 0);

            // Alpha dilate
            Graphics.Blit(rt1, rt2, _material, 1);

            // Alpha blur (horizontal)
            _material.SetVector("_BlurDir", Vector3.right);
            Graphics.Blit(rt2, rt1, _material, 2);

            // Alpha blur (vertical)
            _material.SetVector("_BlurDir", Vector3.up);
            Graphics.Blit(rt1, rt2, _material, 2);

            _buffer = rt2;
            RenderTexture.ReleaseTemporary(rt1);
        }

        void OnRenderObject()
        {
            if (_debug)
            {
                _material.SetPass(3);
                _material.SetTexture("_MainTex", _buffer);
                Graphics.DrawMeshNow(_quadMesh, Matrix4x4.identity);
            }
        }

        #endregion
    }
}
