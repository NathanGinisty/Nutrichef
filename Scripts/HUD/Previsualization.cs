using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Previsualization : MonoBehaviour
{
    [SerializeField] Vector3 posToGo;
    [Space(10)]

    /// <summary>
    /// Raycast mouse to screen.
    /// </summary>
    public bool useInternalRaycast = true;
    public bool follow = true;

    // lerp
    private bool useLerp = false;
    private float t_toGoMax = 0f;
    private float t_toGoActual = 0f;
    private Vector3 departPos;
    private Vector3 previousPosToGo;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Transform childTransform;

    private bool isInitialised = false;

    public void Initialise()
    {
        if (!isInitialised)
        {
            isInitialised = true;

            childTransform = transform.GetChild(0);
            meshFilter = childTransform.GetComponent<MeshFilter>();
            meshRenderer = childTransform.GetComponent<MeshRenderer>();
            posToGo = new Vector3(0, 0, 0);
        }
    }

    void Update()
    {
        if (useInternalRaycast)
        {
            posToGo = GetMouseToScreen();
        }

        UpdateFollowing(posToGo);
    }

    #region PUBLIC

    /// <summary>
    /// If you want to only use your position calcul method, don't forget to set followMouse to false.
    /// </summary>
    public void SetPos(Vector3 _posToGo)
    {
        posToGo = _posToGo;
    }

    public void SetPos(Vector3 _posToGo, Quaternion _rota)
    {
        posToGo = _posToGo;
        transform.rotation = _rota;
    }

    /// <summary>
    /// Set lerp and the time require to go to the next pos
    /// </summary>
    public void SetLerp(bool _useLerp, float _t_toGo = 0f)
    {
        useLerp = _useLerp;
        t_toGoMax = _t_toGo;

        departPos = transform.position;
        previousPosToGo = posToGo;
    }

	public void SetPrevisualization(Material _material)
	{
		if (_material == null)
			Debug.LogError("Trying to set a NULL Material to a Previsualization");

        meshRenderer.materials = new Material[1];
        meshRenderer.material = _material;
	}
    /// <summary>
    /// Set the mesh of the 3D model that you want to previsualize
    /// </summary>
    public void SetPrevisualization(Mesh _mesh, Material _material = null)
    {
        meshFilter.mesh = _mesh;
        if (_material != null)
        {
            meshRenderer.materials = new Material[1];
            meshRenderer.material = _material;
        }
    }

    public void SetPrevisualization(Mesh _mesh, Material[] _materials)
    {
        meshFilter.mesh = _mesh;
        if (_materials != null) meshRenderer.materials = _materials;
    }

    public void SetPrevisualization(Mesh _mesh, Vector3 _pos, Material _material = null)
    {
        meshFilter.mesh = _mesh;
        if (_material != null)
        {
            meshRenderer.materials = new Material[1];
            meshRenderer.material = _material;
        }

        transform.position = _pos;
    }

    public void SetPrevisualization(Mesh _mesh, Vector3 _pos, Material[] _materials)
    {
        meshFilter.mesh = _mesh;
        if (_materials != null) meshRenderer.materials = _materials;

        transform.position = _pos;
    }

    public void SetPrevisualization(Mesh _mesh, Vector3 _pos, Quaternion _rota, Material _material = null)
    {
        meshFilter.mesh = _mesh;
        if (_material != null)
        {
            meshRenderer.materials = new Material[1];
            meshRenderer.material = _material;
        }

        transform.position = _pos;
        transform.rotation = _rota;
    }

    public void SetPrevisualization(Mesh _mesh, Vector3 _pos, Quaternion _rota, Material[] _materials)
    {
        meshFilter.mesh = _mesh;
        if (_materials != null) meshRenderer.materials = _materials;

        transform.position = _pos;
        transform.rotation = _rota;
    }

    public void SetPrevisualization(Mesh _mesh, Vector3 _pos, Quaternion _rota, Vector3 _scale, Material _material = null)
    {
        meshFilter.mesh = _mesh;
        if (_material != null)
        {
            meshRenderer.materials = new Material[1];
            meshRenderer.material = _material;
        }

        transform.position = _pos;
        transform.rotation = _rota;
        transform.localScale = _scale;
    }

    public void SetPrevisualization(Mesh _mesh, Vector3 _pos, Quaternion _rota, Vector3 _scale, Material[] _materials)
    {
        meshFilter.mesh = _mesh;
        if (_materials != null) meshRenderer.materials = _materials;

        transform.position = _pos;
        transform.rotation = _rota;
        transform.localScale = _scale;
    }

    // --------

    public void SetPrevisualization(GameObject _go)
    {
        CreateChildren(_go);
    }

    public void SetPrevisualization(GameObject _go, Material _material = null)
    {
        if (_material != null) meshRenderer.material = _material;
        CreateChildren(_go);

    }

    public void SetPrevisualization(GameObject _go, Vector3 _pos, Quaternion _rota, Material _material = null)
    {
        if (_material != null) meshRenderer.material = _material;
        CreateChildren(_go);

        transform.position = _pos;
        transform.rotation = _rota;

    }

    public void SetPrevisualization(GameObject _go, Vector3 _pos, Quaternion _rota, Vector3 _scale, Material _material = null)
    {
        if (_material != null) meshRenderer.material = _material;
        CreateChildren(_go);

        transform.position = _pos;
        transform.rotation = _rota;
        transform.localScale = _scale;

    }


    #endregion


    #region PRIVATE

    private void UpdateFollowing(Vector3 _posToGo)
    {
        if (follow)
        {
            if (!useLerp)
            {
                transform.position = _posToGo;
            }
            else
            {
                if (previousPosToGo != _posToGo)
                {
                    departPos = transform.position;
                    t_toGoActual = 0f;
                }

                if (t_toGoActual < t_toGoMax)
                {
                    t_toGoActual += Time.deltaTime;
                    transform.position = Vector3.Lerp(departPos, _posToGo, t_toGoActual / t_toGoMax);

                }
                else
                {
                    t_toGoActual = 1;
                    transform.position = Vector3.Lerp(departPos, _posToGo, t_toGoActual / t_toGoMax);

                    departPos = transform.position;
                }

                previousPosToGo = _posToGo;
            }
        }
    }

    private Vector3 GetMouseToScreen()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            return hit.point;
        }

        return posToGo;
    }

    // 1 parent + X child
    private void CreateChildren(GameObject _go)
    {
        Transform[] transformsFromGO = _go.GetComponentsInChildren<Transform>(false);

        // First
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        childTransform.name = transformsFromGO[0].name;
        childTransform.SetPositionAndRotation(transformsFromGO[0].localPosition, transformsFromGO[0].localRotation);
        childTransform.localScale = transformsFromGO[0].localScale;

        if (transformsFromGO[0].GetComponent<MeshFilter>() != null)
        {
            meshFilter.mesh = transformsFromGO[0].GetComponent<MeshFilter>().sharedMesh;
        }

        if (transformsFromGO[0].GetComponent<MeshRenderer>() != null)
        {
            meshRenderer.sharedMaterials = transformsFromGO[0].GetComponent<MeshRenderer>().sharedMaterials;
        }

        // Others
        for (int i = 1; i < transformsFromGO.Length; i++)
        {
            GameObject newGO = new GameObject(transformsFromGO[i].name);

            newGO.transform.SetParent(childTransform);

            newGO.transform.SetPositionAndRotation(transformsFromGO[i].localPosition, transformsFromGO[i].localRotation);
            newGO.transform.localScale = transformsFromGO[i].localScale;

            if (transformsFromGO[i].GetComponent<MeshFilter>() != null)
            {
                MeshFilter mf = newGO.AddComponent<MeshFilter>();
                mf.mesh = transformsFromGO[i].GetComponent<MeshFilter>().sharedMesh;
            }

            if (transformsFromGO[i].GetComponent<MeshRenderer>() != null)
            {
                MeshRenderer mr = newGO.AddComponent<MeshRenderer>();
                mr.sharedMaterials = transformsFromGO[i].GetComponent<MeshRenderer>().sharedMaterials;
            }
        }
    }

    // X parent + X child
    [System.Obsolete("Ne fonctionne pas!!")]
    private void CreateChildren_v2(GameObject _go)
    {
        Transform[] transformsFromGO = _go.GetComponentsInChildren<Transform>(false);

        // First
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        childTransform.name = transformsFromGO[0].name;
        childTransform.SetPositionAndRotation(transformsFromGO[0].localPosition, transformsFromGO[0].localRotation);
        childTransform.localScale = transformsFromGO[0].localScale;

        if (transformsFromGO[0].GetComponent<MeshFilter>() != null)
        {
            meshFilter.mesh = transformsFromGO[0].GetComponent<MeshFilter>().sharedMesh;
        }

        if (transformsFromGO[0].GetComponent<MeshRenderer>() != null)
        {
            meshRenderer.sharedMaterials = transformsFromGO[0].GetComponent<MeshRenderer>().sharedMaterials;
        }

        // Others
        for (int i = 1; i < transformsFromGO.Length; i++)
        {
            Debug.Log(transformsFromGO[i].name);

            GameObject newGO = new GameObject(transformsFromGO[i].name);

            newGO.transform.SetParent(transform.Find(transformsFromGO[i].parent.name));

            newGO.transform.SetPositionAndRotation(transformsFromGO[i].localPosition, transformsFromGO[i].localRotation);
            newGO.transform.localScale = transformsFromGO[i].localScale;

            if (transformsFromGO[i].GetComponent<MeshFilter>() != null)
            {
                MeshFilter mf = newGO.AddComponent<MeshFilter>();
                mf.mesh = transformsFromGO[i].GetComponent<MeshFilter>().sharedMesh;
            }

            if (transformsFromGO[i].GetComponent<MeshRenderer>() != null)
            {
                MeshRenderer mr = newGO.AddComponent<MeshRenderer>();
                mr.sharedMaterials = transformsFromGO[i].GetComponent<MeshRenderer>().sharedMaterials;
            }
        }
    }
    #endregion
}
