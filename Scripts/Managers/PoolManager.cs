using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance = null;

    [SerializeField] int DefaultSizePool = 50;
    Dictionary<KeyValuePair<string, AlimentState>, Pool> pools = new Dictionary<KeyValuePair<string, AlimentState>, Pool>();

    public delegate void OnEndPoolLoad();
    public OnEndPoolLoad onEndPoolLoad;
    
    int countAliment;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
    }

    public void InitPool()
    {
        Debug.Log("GameManager INIT POOL");
        countAliment = FoodDatabase.mapAlimentObject.Count;
        int countAlimentLoad = 0;
        foreach (KeyValuePair<string, AlimentObject> pair in FoodDatabase.mapAlimentObject)
        {
            for (int stateIndex = 0; stateIndex < (int)AlimentState.COUNT; stateIndex++)
            {
                CreatePool(pair.Value, (AlimentState)stateIndex);
            }
            countAlimentLoad++;
        }
    }

    public void InitPool(ref float progress)
    {
        countAliment = FoodDatabase.mapAlimentObject.Count;
        int countAlimentLoad = 0;
        foreach (KeyValuePair<string, AlimentObject> pair in FoodDatabase.mapAlimentObject)
        {
            for (int stateIndex = 0; stateIndex < (int)AlimentState.COUNT; stateIndex++)
            {
                // Try to see if pair.Value contain the alimentState
                foreach (AlimentObject.InfoState infoState in pair.Value.listState)
                {
                    if (infoState.state == (AlimentState)stateIndex)
                    {
                        // create
                        CreatePool(pair.Value, (AlimentState)stateIndex);
                    }
                }
            }
            countAlimentLoad++;
            progress = countAlimentLoad / countAliment;
        }
    }

    private void CreatePool(AlimentObject aliment, AlimentState _state)
    {
        //create pool
        GameObject go = new GameObject("Pool :" + aliment.name + " " + _state.ToString());
        go.transform.parent = transform;
        Pool newPool = go.AddComponent<Pool>();

        KeyValuePair<string, AlimentState> key = new KeyValuePair<string, AlimentState>(aliment.name, _state); // Create a new key with the NAME and the STATE of the current aliment

        GameObject tmpAliment = FoodDatabase.InstantiateAliment(aliment, _state);

        newPool.Init(tmpAliment, DefaultSizePool);

        pools[key] = newPool;

    }

    public Poolable Get(KeyValuePair<string, AlimentState> key,bool instantiateViewID = false)
    {
        return pools[key].Get(instantiateViewID);
    }

    public Poolable Get(KeyValuePair<string, AlimentState> key, int viewID)
    {
        return pools[key].Get(viewID);
    }

    public Poolable Get(KeyValuePair<string, AlimentState> key, Transform transformParent, bool instantiateViewID = false)
    {
        Poolable foodObject = pools[key].Get(instantiateViewID);
        foodObject.transform.parent = transformParent;
        return foodObject;
    }

    public Poolable Get(KeyValuePair<string, AlimentState> key, Vector3 position, Quaternion rotation, Transform transformParent, bool instantiateViewID = false)
    {
        Poolable foodObject = pools[key].Get(instantiateViewID);
        foodObject.transform.parent = transformParent;
        foodObject.transform.position = position;
        foodObject.transform.rotation = rotation;
        return foodObject;
    }

    public void Insert(KeyValuePair<string, AlimentState> key, Poolable _object)
    {
        if (pools.ContainsKey(key))
        {
            pools[key].Insert(_object);
        }
        else
        {
            GameObject go = new GameObject("Pool :" + _object.name);
            go.AddComponent<Pool>().Init(_object.gameObject, DefaultSizePool);
            Debug.LogError(_object.name + " try to be insert in a pool add it in the Manager!");
        }
    }
}
