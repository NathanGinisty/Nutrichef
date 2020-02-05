using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionFrame : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [SerializeField] Vector3 pos;
    [SerializeField] Vector3 size;

    void Start()
    {
        // mesh
        meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Plane);
        meshFilter.mesh = tmp.GetComponent<MeshFilter>().mesh;
        Destroy(tmp);

        meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    void Update()
    {
        
    }

    public void Set(Vector3 _pos, Vector3 _size, Material _material = null)
    {
        SetPosAndSize(_pos, _size);

        if (_material != null) meshRenderer.material = _material;
    }

    public void SetPos(Vector3 _pos)
    {
        pos = _pos;
        transform.position = pos;
    }

    public void SetSize(Vector3 _size)
    {
        size = _size;
        transform.localScale = new Vector3(_size.x, 1, -size.z);
    }

    public void SetPosAndSize(Vector3 _pos, Vector3 _size)
    {
        SetPos(_pos);
        SetSize(_size);
    }
}
