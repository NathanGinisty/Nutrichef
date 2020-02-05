using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Poolable : MonoBehaviourPun
{
    public Pool pool { get; set; }

    public void DelObject()
    {
        pool.Insert(this);
    }

    [PunRPC]
    public void DelObjectOnline()
    {
        pool.Insert(this);
    }
}
