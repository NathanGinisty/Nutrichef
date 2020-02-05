using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    public Table[] tables = new Table[4];

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        tables = GetComponentsInChildren<Table>();
    }

    public Table GetFreeTable()
    {
        foreach (Table t in tables)
        {
            if (t.IsFree)
            {
                return t;
            }
        }
        return null;
    }
}
