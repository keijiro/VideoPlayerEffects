using UnityEngine;
using System.Collections.Generic;

namespace Voxelizer
{
    public class VoxelRenderer : MonoBehaviour
    {
        #region Editable properties

        [Space]
        [SerializeField] int _columns = 32;
        [SerializeField] int _rows = 18;
        [SerializeField] Vector2 _extent = new Vector2(3.2f, 1.8f);
        [Space]
        [SerializeField] RenderTexture _source;
        [SerializeField, Range(0, 1)] float _threshold = 0.05f;
        [SerializeField, Range(1, 10)] float _decaySpeed = 5;
        [Space]
        [SerializeField] float _voxelScale = 0.25f;
        [SerializeField] Material _voxelMaterial;
        [Space]
        [SerializeField] float _zMove = -0.1f;
        [SerializeField] float _noiseFrequency = 10;
        [SerializeField] float _noiseSpeed = 0.5f;
        [SerializeField] float _noiseToPosition = 0.015f;
        [SerializeField] float _noiseToRotation = 60;
        [SerializeField] float _noiseToScale = 0.5f;
        [Space]
        [SerializeField] bool _debug;

        #endregion

        #region Private members

        [SerializeField, HideInInspector] Mesh _baseMesh;
        Mesh _bulkMesh;

        [SerializeField, HideInInspector] Shader _feedbackShader;
        Material _feedbackMaterial;

        [SerializeField, HideInInspector] Mesh _quadMesh;

        RenderTexture _feedbackBuffer;
        MaterialPropertyBlock _props;

        #endregion

        #region MonoBehaviour functions

        void OnValidate()
        {
            _columns = Mathf.Clamp(_columns, 1, 128);
            _rows = Mathf.Clamp(_rows, 1, 128);
            _voxelScale = Mathf.Max(_voxelScale, 0.0f);
        }

        void OnDestroy()
        {
            if (_feedbackBuffer != null)
                RenderTexture.ReleaseTemporary(_feedbackBuffer);

            Destroy(_bulkMesh);
            Destroy(_feedbackMaterial);
        }

        void Start()
        {
            _bulkMesh = BuildBulkMesh();
            _feedbackMaterial = new Material(_feedbackShader);
            _props = new MaterialPropertyBlock();
        }

        void Update()
        {
            var rt = RenderTexture.GetTemporary(
                _source.width / 4, _source.height / 4, 0, RenderTextureFormat.RHalf
            );

            _feedbackMaterial.SetTexture("_PrevTex", _feedbackBuffer);
            _feedbackMaterial.SetFloat("_Convergence", -_decaySpeed);

            Graphics.Blit(_source, rt, _feedbackMaterial, 0);

            if (_feedbackBuffer != null)
                RenderTexture.ReleaseTemporary(_feedbackBuffer);
            _feedbackBuffer = rt;

            _props.SetTexture("_ModTex", _feedbackBuffer);
            _props.SetFloat("_Threshold", _threshold);
            _props.SetVector("_Extent", _extent);
            _props.SetFloat("_ZMove", _zMove);
            _props.SetFloat("_Scale", _voxelScale);
            _props.SetVector("_NoiseParams", new Vector2(
                _noiseFrequency, _noiseSpeed
            ));
            _props.SetVector("_NoiseAmp", new Vector3(
                _noiseToPosition, Mathf.Deg2Rad * _noiseToRotation, _noiseToScale
            ));

            Graphics.DrawMesh(
                _bulkMesh, transform.localToWorldMatrix, _voxelMaterial,
                gameObject.layer, null, 0, _props
            );
        }

        void OnRenderObject()
        {
            if (_debug)
            {
                _feedbackMaterial.SetPass(1);
                Graphics.DrawMeshNow(_quadMesh, Matrix4x4.identity);
            }
        }

        #endregion

        #region Bulk mesh construction

        Mesh BuildBulkMesh()
        {
            var instanceCount = _columns * _rows;

            var iVertices = _baseMesh.vertices;
            var iNormals = _baseMesh.normals;
            var iUVs = _baseMesh.uv;

            var oVertices = new List<Vector3>(iVertices.Length * instanceCount);
            var oNormals = new List<Vector3>(iNormals.Length * instanceCount);
            var oUVs = new List<Vector2>(iUVs.Length * instanceCount);

            for (var i = 0; i < instanceCount; i++)
            {
                oVertices.AddRange(iVertices);
                oNormals.AddRange(iNormals);
                oUVs.AddRange(iUVs);
            }

            var oUV2 = new List<Vector2>(oUVs.Count);

            for (var row = 0; row < _rows; row++)
            {
                for (var col = 0; col < _columns; col++)
                {
                    var uv = new Vector2(
                        (col + 0.5f) / _columns,
                        (row + 0.5f) / _rows
                    );

                    for (var i = 0; i < _baseMesh.vertexCount; i++)
                        oUV2.Add(uv);
                }
            }

            var iIndices = _baseMesh.triangles;
            var oIndices = new List<int>(iIndices.Length * instanceCount);

            for (var i = 0; i < instanceCount; i++)
            {
                for (var j = 0; j < iIndices.Length; j++)
                {
                    oIndices.Add(iIndices[j]);
                    iIndices[j] += _baseMesh.vertexCount;
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(oVertices);
            mesh.SetNormals(oNormals);
            mesh.SetUVs(0, oUVs);
            mesh.SetUVs(1, oUV2);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(oIndices, 0);
            mesh.UploadMeshData(true);
            return mesh;
        }

        #endregion
    }
}
