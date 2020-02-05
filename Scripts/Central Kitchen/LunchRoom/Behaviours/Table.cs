using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Table : MonoBehaviour
{
    bool isFree = true;

    [SerializeField] Transform chairPos;
    Customer customer = null;

    public Transform ChairPos { get => chairPos; private set => chairPos = value; }
    public bool IsFree { get => isFree; set => isFree = value; }

}
