using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPool : MonoBehaviour
{
    Poolable poolable;
    void Start()
    {
        poolable = GetComponent<Poolable>();

        poolable.DelObject();
    }
    
}
