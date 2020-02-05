using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackOfAliment : MonoBehaviour
{
    public float t_expiry;
    public AlimentObject alimentObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        t_expiry -= Time.deltaTime;

        //switch ()
        //{ }
    }

    public void Init(AlimentObject _alimentObject, float _t_expiry = 0f)
    {
        alimentObject = _alimentObject;

        //if (alimentObject.meshStack != null) GetComponent<MeshFilter>().mesh = alimentObject.meshStack;
        //if (_alimentObject.materialsStack != null && _alimentObject.materialsStack.Length > 0) GetComponent<MeshRenderer>().materials = _alimentObject.materialsStack;

        if (_t_expiry != 0f) t_expiry = _t_expiry;
    }
}
