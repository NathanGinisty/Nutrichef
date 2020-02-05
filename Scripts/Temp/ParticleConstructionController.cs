using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleConstructionController : MonoBehaviour
{
    ParticleSystem particleSystem;
    [SerializeField] bool setManually;
    [SerializeField] int emitCount = 35;
    // Start is called before the first frame update
    void Start()
    {

        particleSystem = GetComponent<ParticleSystem>();
        BSName bsName = GetComponentInParent<BSName>();
        if (!setManually && bsName != null)
        {
            var sh = particleSystem.shape;
            sh.position = new Vector3(bsName.size.x / 2f, 0, bsName.size.y / 2f);
            sh.scale = new Vector3(1, bsName.size.y, 1);
            emitCount =  (int)((bsName.size.x * bsName.size.y) * 35f * 0.7f);
        }

        particleSystem.Emit(emitCount);
    }
    
}
