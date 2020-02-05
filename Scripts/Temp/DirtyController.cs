using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtyController : MonoBehaviour
{
    [SerializeField] bool setManually = false;
    [SerializeField] List<Material> objMaterials = new List<Material>();
    [SerializeField] float Dirtyness = 0;
    ParticleSystem particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        if (!setManually)
        {
            MeshRenderer meshRenderer = GetComponentInParent<MeshRenderer>();
            if (meshRenderer != null)
            {
                foreach (var mat in meshRenderer.materials)
                {
                    if (mat.HasProperty("_Dirtyness"))
                    {
                        objMaterials.Add(mat);
                    }
                }

            }

        }
        SetDirtyness(Dirtyness);
    }

    public void SetDirtyness(float val)
    {
        Dirtyness = val;
        val = Mathf.Clamp01(val);
        objMaterials.ForEach(x => x.SetFloat("_Dirtyness", val));

        if (val > 0.8f)
        {
            var emission =  particleSystem.emission;
            emission.enabled = true;
        }
        else
        {
            var emission = particleSystem.emission;
            emission.enabled = false;
        }
    }
}
