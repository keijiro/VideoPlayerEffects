using UnityEngine;
using UnityEngine.Video;

namespace VideoOverlayEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Stripe : MonoBehaviour
    {
        #region Editable properties

        [SerializeField] VideoPlayer _sourceVideo;
        [SerializeField] Texture _sourceTexture;
        [Space]
        [SerializeField] Color _strokeColor = Color.white;
        [SerializeField] Color _backgroundColor = Color.black;
        [Space]
        [SerializeField, Range(-180, 180)] float _angle = 0;
        [SerializeField] float _repeat = 25;
        [Space]
        [SerializeField, Range(0, 1)] float _width = 0.3f;
        [SerializeField, Range(0, 4)] float _thinLine = 1;
        [Space]
        [SerializeField, Range(0, 4)] float _noiseFrequency = 1;
        [SerializeField, Range(0, 4)] float _noiseAnimation = 0.3f;
        [SerializeField, Range(0, 8)] float _noiseAmplitude = 1;

        #endregion

        #region Private members

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;
        float _noiseOffset;

        #endregion

        #region MonoBehaviour functions

        void Update()
        {
            if (Application.isPlaying)
                _noiseOffset += _noiseAnimation * Time.deltaTime;
        }

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

            if (_sourceVideo != null)
                _material.SetTexture("_SourceTex", _sourceVideo.texture);
            else if (_sourceTexture != null)
                _material.SetTexture("_SourceTex", _sourceTexture);
            else
                _material.SetTexture("_SourceTex", source);

            // To give gamma color, use SetVector instead of SetColor.
            _material.SetVector("_Color", _strokeColor);
            _material.SetVector("_BGColor", _backgroundColor);

            var cam = GetComponent<Camera>();
            var sin = Mathf.Sin(Mathf.Deg2Rad * _angle);
            var cos = Mathf.Cos(Mathf.Deg2Rad * _angle);

            var trans = new Vector4( // Actually this is a 2x2 matrix.
                cam.aspect * cos, -sin, cam.aspect * sin, cos
            );

            var inv_trans = new Vector4( // Actually this is a 2x2 matrix.
                cos / cam.aspect, sin / cam.aspect, -sin, cos
            );

            _material.SetVector("_Trans", trans);
            _material.SetVector("_InvTrans", inv_trans);

            _material.SetFloat("_Repeat", _repeat);
            _material.SetFloat("_Thin", _thinLine);
            _material.SetFloat("_Width", _width);

            _material.SetVector("_Noise", new Vector3(
                _noiseFrequency, _noiseAmplitude, _noiseOffset
            ));

            Graphics.Blit(source, destination, _material, 0);
        }

        #endregion
    }
}
