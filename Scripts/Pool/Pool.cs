using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Pool : MonoBehaviourPun
{
    List<Poolable> models = new List<Poolable>();
    public GameObject modelGameObject { get; private set; }
    public Poolable modelPoolable { get; private set; }

    public void Init(GameObject _gameObject, int size)
    {
        List<int> poolablesViewID = new List<int>();
        modelPoolable = _gameObject.GetComponent<Poolable>();

        modelGameObject = _gameObject;
        modelGameObject.gameObject.SetActive(false);
        int viewID = -1;
        for (int i = 0; i < size; i++)
        {
            viewID = i;
            GameObject go = Instantiate(modelGameObject, transform);
            Poolable poolable = go.GetComponent<Poolable>();

            poolable.pool = this;
            models.Add(poolable);
        }
       
    }

    public Poolable Get(bool _instantiateViewID = false)
    {
        Poolable poolable;

        if (models.Count == 0)
        {
            poolable = Instantiate(modelGameObject, transform).GetComponent<Poolable>();
        }
        else
        {
            poolable = models.First();
            models.Remove(models.First());
        }

        poolable.gameObject.SetActive(true);
        poolable.transform.SetParent(null);

        if(_instantiateViewID)
        {
            PhotonNetwork.AllocateViewID(poolable.GetComponent<PhotonView>());
        }


        return poolable;
    }


    public Poolable Get(int viewID)
    {
        Poolable _object = Get();
        _object.photonView.ViewID = viewID;

        _object.gameObject.SetActive(true);
        _object.transform.SetParent(null);

        return _object;
    }

    public void Insert(Poolable _object)
    {
        _object.transform.SetParent(transform);
        _object.gameObject.SetActive(false);
        PhotonView view = _object.GetComponent<PhotonView>();
        view.ViewID = 0;
    }
    
}
