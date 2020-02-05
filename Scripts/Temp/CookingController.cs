using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CookingController : MonoBehaviour
{
    [SerializeField] List<Color> donenessColors = new List<Color>() { Color.white, Color.grey };
    [Range(0, 1)] [SerializeField] float debugDoneness = 0;
    [Range(0, 1)] [SerializeField] float DonenessStartParticle = 0.5f;

    List<Material> materials = new List<Material>();
    ParticleSystem particleSystem;
    float doneness = 0;

    void Start()
    {
        if (donenessColors.Count < 2)
        {
            Debug.LogError("Aliment have only one color");
            Destroy(this);
        }
        materials = GetComponentInParent<MeshRenderer>().materials.ToList();
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (debugDoneness != doneness)
        {
            SetDoneness(debugDoneness);
        }
    }

    public void SetDoneness(float _doneness)
    {
        doneness = Mathf.Clamp01(_doneness);

        float colorActual = _doneness * (donenessColors.Count - 1);

        int colorIndexA = Mathf.FloorToInt(colorActual);
        int colorIndexB = colorIndexA + 1;
        Color result;

        if (colorIndexB < donenessColors.Count)
        {
            float inter = colorActual - colorIndexA;
            result = Color.Lerp(donenessColors[colorIndexA], donenessColors[colorIndexB], inter);
        }
        else
        {
            result = donenessColors[colorIndexA];
        }

        materials.ForEach(x => x.color = result);

        var emission = particleSystem.emission;
        if (_doneness > DonenessStartParticle)
        {
            emission.enabled = true;
        }
        else
        {
            emission.enabled = false;
        }
    }

}
