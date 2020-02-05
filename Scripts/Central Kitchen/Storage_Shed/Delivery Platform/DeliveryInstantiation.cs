using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Photon.Pun;
using Photon.Realtime;

public class DeliveryInstantiation : MonoBehaviourPun
{
    [SerializeField] Transform PosInstantiationParent;
    [SerializeField] DeliveryMan deliveryMan;
    [SerializeField] DeliveryMan_UI ui;

    List<KeyValuePair<string, AlimentState>> boxToInstantiate = new List<KeyValuePair<string, AlimentState>>();
    Transform[] PosInstantiation = new Transform[4];
    List<int> PosFree = new List<int>();
    Dictionary<BoxDatasController, int> boxInPos = new Dictionary<BoxDatasController, int>();

    int nbTotalOfBox = 0;

    // Start is called before the first frame update

    private void Awake()
    {

        GameManager.Instance.initScripts += Init;
    }

    void Init()
    {
        for (int i = 0; i < PosInstantiationParent.childCount; i++)
        {
            PosInstantiation[i] = PosInstantiationParent.GetChild(i);
            PosFree.Add(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
    }

    private void OnGrabBox(GrabableObject _grabableObject)
    {
        BoxDatasController box = _grabableObject.GetComponent<BoxDatasController>();
        PosFree.Add(boxInPos[box]);
        boxInPos.Remove(box);

        UpdateBoxPosition();

        StartCoroutine(InstantiateBox());

        _grabableObject.onGrab -= OnGrabBox;

        if (boxInPos.Count == 0)
        {
            deliveryMan.Leave();
        }

        // sound
        GameManager.Instance.Audio.PlaySound("FastConveyorBelt", AudioManager.Canal.SoundEffect);
    }

    private void UpdateBoxPosition()
    {
        int finalPos = -1;

        for (int i = 0; i < PosInstantiation.Count(); i++)
        {
            if (PosFree.Contains(i))
            {
                finalPos = i;
                for (int j = i; j < PosInstantiation.Count(); j++)
                {
                    if (!PosFree.Contains(j))
                    {
                        BoxDatasController ga = null;

                        foreach (BoxDatasController _g in boxInPos.Keys)
                        {
                            if (boxInPos[_g] == j)
                            {
                                ga = _g;
                                break;
                            }
                        }

                        if (ga != null)
                        {
                            boxInPos[ga] = finalPos;
                            PosFree.Remove(finalPos);

                            StartCoroutine(TranslateBoxToPosition(ga, j));

                            break;
                        }
                    }
                }
            }
        }
    }

    private int GetFinalBoxPosition(int currentPosIndex)
    {
        int finalPos = currentPosIndex;

        for (int i = currentPosIndex; i >= 0; i--)
        {
            if (PosFree.Contains(i))
            {
                finalPos = i;
            }
        }

        return finalPos;
    }

    IEnumerator TranslateBoxToPosition(BoxDatasController _box, int previousPos)
    {
        int finalPos = boxInPos[_box];
        Vector3 startPos = _box.transform.position;
        float timePass = 0f;

        float timeNeeded = 1f * (previousPos - finalPos);

        bool posReleased = false;

        while (timePass < timeNeeded && previousPos != finalPos)
        {
            timePass += Time.deltaTime;

            if (!posReleased)
            {
                PosFree.Add(previousPos);
                posReleased = true;
            }

            _box.transform.position = Vector3.Lerp(startPos, PosInstantiation[finalPos].position, timePass / timeNeeded);

            yield return 0;
        }

        if (finalPos == 0)
        {
            _box.SetActive(true);
        }

        yield return 0;
    }

    IEnumerator InstantiateBox()
    {
        while (boxToInstantiate.Count > 0 && PosFree.Count > 0)
        {
            yield return new WaitForSeconds(1.2f);

            if (PosFree.Contains(PosInstantiation.Count() - 2))
            {
                int posIndex = PosInstantiation.Count() - 1;
                int finalPosIndex;

                Poolable foodToInstantiate = PoolManager.Instance.Get(boxToInstantiate[0], true);
                BoxDatasController box = foodToInstantiate.GetComponent<BoxDatasController>();
                PhotonView view = foodToInstantiate.GetComponent<PhotonView>();

                foodToInstantiate.transform.position = PosInstantiation[posIndex].position;
                foodToInstantiate.transform.rotation = PosInstantiation[posIndex].rotation;

                box.Init();
                box.SetActive(false);
                box.Grabable.onGrab += OnGrabBox;

                boxInPos.Add(box, posIndex);

                PosFree.Remove(posIndex);

                finalPosIndex = GetFinalBoxPosition(posIndex);

                boxInPos[box] = finalPosIndex;
                PosFree.Remove(finalPosIndex);

                StartCoroutine(TranslateBoxToPosition(box, posIndex));

                photonView.RPC("InstantiateBox", RpcTarget.Others, boxToInstantiate[0].Key, boxToInstantiate[0].Value, posIndex, finalPosIndex, view.ViewID);

                boxToInstantiate.RemoveAt(0);

            }

            yield return 0;
        }

        yield return 0;
    }

    [PunRPC]
    void InstantiateBox(string _alimentName, AlimentState _alimentState, int posFree, int finalPos, int viewID)
    {
        //Debug.Log("Receive RPC InstantiateBox :" + _alimentName + " AlimentState:" + _alimentState + " viewID:" + viewID);
        KeyValuePair<string, AlimentState> key = new KeyValuePair<string, AlimentState>(_alimentName, _alimentState);

        Poolable foodToInstantiate = PoolManager.Instance.Get(key, viewID);
        BoxDatasController box = foodToInstantiate.GetComponent<BoxDatasController>();

        foodToInstantiate.transform.position = PosInstantiation[posFree].position;
        foodToInstantiate.transform.rotation = PosInstantiation[posFree].rotation;

        box.Init();
        box.SetActive(false);
        box.Grabable.onGrab += OnGrabBox;

        boxInPos.Add(box, posFree);

        PosFree.Remove(posFree);

        boxInPos[box] = finalPos;
        PosFree.Remove(finalPos);

        StartCoroutine(TranslateBoxToPosition(box, posFree));
    }

    // fill list of Aliment boxes to instantiate

    public void FillList()
    {
        Dictionary<string, int> order = null;


        order = deliveryMan.DeliveryManOrder;

        nbTotalOfBox = order.Count;

        // Instantiation of Player order UI
        for (int i = 0; i < order.Keys.Count; i++)
        {
            string alimentName = order.Keys.ToList()[i];
            int AlimentAmount = order[alimentName];

            for (int j = 0; j < AlimentAmount; j++)
            {
                boxToInstantiate.Add(new KeyValuePair<string, AlimentState>(alimentName, AlimentState.Box));
            }
        }

        StartCoroutine(InstantiateBox());

        ui.DisplayUI(false);
    }
}
