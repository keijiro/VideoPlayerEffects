using UnityEngine;
using System.Collections.Generic;

public class Cubes : MonoBehaviour
{
    [SerializeField] int _columns = 32;
    [SerializeField] int _rows = 18;
    [SerializeField] Vector2 _extent = new Vector2(3.2f, 1.8f);
    [SerializeField] RenderTexture _source;
    [SerializeField] Mesh _shape;
    [SerializeField] Material _material;
    [SerializeField] float _scale = 0.1f;

    [SerializeField, HideInInspector] Shader _feedbackShader;

    Mesh _bulkMesh;
    MaterialPropertyBlock _props;

    Material _feedbackMaterial;
    RenderTexture _feedbackBuffer;

    void OnValidate()
    {
        _columns = Mathf.Clamp(_columns, 1, 128);
        _rows = Mathf.Clamp(_rows, 1, 128);
        _scale = Mathf.Max(_scale, 0.0f);
    }

    void OnDestroy()
    {
        Destroy(_bulkMesh);

        if (_feedbackBuffer != null)
            RenderTexture.ReleaseTemporary(_feedbackBuffer);

        Destroy(_feedbackMaterial);
    }

    void Start()
    {
        _bulkMesh = BuildBulkMesh();
        _props = new MaterialPropertyBlock();
        _feedbackMaterial = new Material(_feedbackShader);
    }

    void Update()
    {
        var rt = RenderTexture.GetTemporary(
            _source.width, _source.height, 0, RenderTextureFormat.ARGBHalf
        );

        _feedbackMaterial.SetTexture("_PrevTex", _feedbackBuffer);
        Graphics.Blit(_source, rt, _feedbackMaterial, 0);

        if (_feedbackBuffer != null)
            RenderTexture.ReleaseTemporary(_feedbackBuffer);
        _feedbackBuffer = rt;

        _props.SetTexture("_ModTex", _feedbackBuffer);
        _props.SetFloat("_Scale", _scale);
        _props.SetVector("_Extent", _extent);

        Graphics.DrawMesh(
            _bulkMesh, transform.localToWorldMatrix, _material,
            gameObject.layer, null, 0, _props
        );
    }

    Mesh BuildBulkMesh()
    {
        var instanceCount = _columns * _rows;

        var iVertices = _shape.vertices;
        var iNormals = _shape.normals;
        var iUVs = _shape.uv;

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

                for (var i = 0; i < _shape.vertexCount; i++)
                    oUV2.Add(uv);
            }
        }

        var iIndices = _shape.triangles;
        var oIndices = new List<int>(iIndices.Length * instanceCount);

        for (var i = 0; i < instanceCount; i++)
        {
            for (var j = 0; j < iIndices.Length; j++)
            {
                oIndices.Add(iIndices[j]);
                iIndices[j] += _shape.vertexCount;
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(oVertices);
        mesh.SetNormals(oNormals);
        mesh.SetUVs(0, oUVs);
        mesh.SetUVs(1, oUV2);
        mesh.SetTriangles(oIndices, 0);
        mesh.UploadMeshData(true);
        return mesh;
    }
}
