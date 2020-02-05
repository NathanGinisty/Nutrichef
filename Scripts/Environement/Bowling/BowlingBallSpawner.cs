using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingBallSpawner : MonoBehaviour
{
    [SerializeField] Object bowlingBall;
    [Space]
    [SerializeField] float spawnRateMin;
    [SerializeField] float spawnRateMax;

    float timePass;
    float currentSpawnRate;

    // Start is called before the first frame update
    void Start()
    {
        timePass = 0;
        currentSpawnRate = Random.Range(spawnRateMin, spawnRateMax);
    }

    // Update is called once per frame
    void Update()
    {
        if (timePass >= currentSpawnRate)
        {
            timePass = 0;

            GameObject go = Instantiate(bowlingBall) as GameObject;
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
            Rigidbody rb = go.GetComponent<Rigidbody>();
            rb.velocity = transform.forward * 20;

            currentSpawnRate = Random.Range(spawnRateMin, spawnRateMax);
        }
        else
        {
            timePass += Time.deltaTime;
        }
    }
}
