using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkPrefab : MonoBehaviour
{
    [SerializeField] Object prefab;
    [SerializeField] int nbSpawn = 3000;
    void Start()
    {

        int sineNb = (int)Mathf.Pow(nbSpawn, 1f / 3f);
        float halfsineNb = sineNb / 2f;
        int nbSpawnCube = (int)Mathf.Pow(sineNb, 3f);
        for (int i = 0; i < sineNb; i++)
        {
            for (int j = 0; j < sineNb; j++)
            {
                for (int k = 0; k < sineNb; k++)
                {
                    GameObject newGo = Instantiate(prefab, transform) as GameObject;
                    Vector3 offset = new Vector3(i - halfsineNb, j, k - halfsineNb);
                    newGo.transform.position = transform.position + offset;
                }
            }
        }
        for (int i = nbSpawnCube; i < nbSpawn; i++)
        {
            GameObject newPommeGo = Instantiate(prefab, transform) as GameObject;
            newPommeGo.transform.position = transform.position + Vector3.up * (halfsineNb + (i - nbSpawnCube));

        }
    }
}
