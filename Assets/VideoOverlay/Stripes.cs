using UnityEngine;
using UnityEngine.Video;

namespace VideoOverlay
{
    public class Stripes : MonoBehaviour
    {
        [Space]
        [SerializeField] VideoPlayer _videoInput;
        [SerializeField] Color _colorTint = Color.white;
        [SerializeField, Range(0, 1)] float _sourceOpacity = 1;
        [Space]
        [SerializeField] float _repeat = 25;
        [SerializeField] float _angle = 0;
        [Space]
        [SerializeField] float _minWidth = 1;
        [SerializeField] float _maxWidth = 8;
        [Space]
        [SerializeField] float _waveSpeed = 0.5f;
        [SerializeField] float _waveModulation = 0.8f;
        [SerializeField] float _waveFrequency = 0.2f;
        [SerializeField] float _waveHeight = 2.0f;

        [SerializeField, HideInInspector] Shader _shader;

        Material _material;

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetTexture("_VideoTex", _videoInput.texture);

            _material.SetColor("_Color", _colorTint);
            _material.SetFloat("_SourceOpacity", _sourceOpacity);

            _material.SetFloat("_Interval", 1.0f / _repeat);
            _material.SetFloat("_Angle", Mathf.Deg2Rad * _angle);
            _material.SetVector("_Width", new Vector2(_minWidth, _maxWidth));
            _material.SetVector("_Wave", new Vector4(
                _waveSpeed, _waveModulation, _waveFrequency, _waveHeight
            ));

            Graphics.Blit(source, destination, _material, 0);
        }
    }
}
