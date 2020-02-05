using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LoadManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject loadCanvas;

    public static LoadManager Instance;
    public static GameManager gameManager;
    public static BSResultData resultData;
    public static BSMapCreator mapCreator;

    public enum LoadState
    {
        LoadPool,
        LoadID,
        LoadMap,
        WaitingBuildID,
        LoadMapBuild,
        WaitingForPLayer,
        COUNT,
        End,
    }

    PoolManager poolManager;

    public LoadState state = LoadState.LoadPool;

    public float[] loadStatesPos = new float[(int)LoadState.COUNT];

    float loadingPos = 0;
    float loadingPosVisual = 0;
    bool isDataPoolReceived = false;
    bool isDataBuildReceived = false;
    Scene loadScene;
    Scene inGameScene;

    List<int> PLayersEndLoad = new List<int>();
    public Dictionary<KeyValuePair<string, AlimentState>, List<int>> poolReceived = new Dictionary<KeyValuePair<string, AlimentState>, List<int>>();

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(Instance.gameObject);
        }

        Instance = this;
        PoolManager.Instance.onEndPoolLoad = EndLoadPool;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.OfflineMode = true;
        }
        poolManager = PoolManager.Instance;
        gameManager = GameManager.Instance;
        resultData = BSResultData.Instance;

        gameManager.autoLaunch = false;
        StartCoroutine(LoadProcess());
        StartCoroutine(UpdateLoadScroll());

    }

    public void SendPoolViewID(KeyValuePair<string, AlimentState> _keyPool, List<int> _poolableID)
    {
        //Debug.Log("Send : " + _keyPool + " count :" + _poolableID.Count);
        photonView.RPC("OnReceivePoolID", RpcTarget.Others, _keyPool.Key, _keyPool.Value, _poolableID.Count, (object)_poolableID.ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_cells"></param>
    public void SendCells(CellData[] _cells)
    {
        //Debug.Log("SendWalls"); 
        List<Vector2> position = new List<Vector2>();
        List<int> index = new List<int>();
        foreach (var obj in _cells)
        {
            position.Add(obj.position);
            index.Add(obj.index);
        }

        photonView.RPC("OnReceiveCells", RpcTarget.Others, (object)position.ToArray(), (object)index.ToArray());
    }

    public void SendWalls(ObjectData[] _walls)
    {
        //Debug.Log("SendWalls"); 
        List<Vector2> position = new List<Vector2>();
        List<float> YRotation = new List<float>();

        foreach (var obj in _walls)
        {
            position.Add(obj.position);
            YRotation.Add(obj.YRotation);
        }

        photonView.RPC("OnReceiveWalls", RpcTarget.Others, (object)position.ToArray(), (object)YRotation.ToArray());
    }

    public void SendCorners(ObjectData[] _corners)
    {
        //Debug.Log("SendCorners");
        List<Vector2> position = new List<Vector2>();
        List<float> YRotation = new List<float>();

        foreach (var obj in _corners)
        {
            position.Add(obj.position);
            YRotation.Add(obj.YRotation);
        }

        photonView.RPC("OnReceiveCorners", RpcTarget.Others, (object)position.ToArray(), (object)YRotation.ToArray());
    }

    public void SendEntrances(EntranceData[] _entrances)
    {
        //Debug.Log("SendEntrances");
        List<Vector2> position = new List<Vector2>();
        List<float> YRotation = new List<float>();
        List<int> blockID = new List<int>();
        List<int> viewID = new List<int>();
        List<int> fromRoom = new List<int>();
        List<int> toRoom = new List<int>();
        List<int> type = new List<int>();

        foreach (var obj in _entrances)
        {
            position.Add(obj.objData.position);
            YRotation.Add(obj.objData.YRotation);
            blockID.Add(obj.objData.blockID);
            viewID.Add(obj.objData.viewID);
            fromRoom.Add((int)obj.fromRoom);
            toRoom.Add((int)obj.toRoom);
            type.Add((int)obj.type);
        }

        photonView.RPC("OnReceiveEntrances", RpcTarget.Others, (object)fromRoom.ToArray(), (object)toRoom.ToArray(), (object)type.ToArray(), (object)position.ToArray(), (object)YRotation.ToArray(), (object)blockID.ToArray(), (object)viewID.ToArray());
    }

    public void SendFurniture(int furnitureIndex, ObjectData[] _furniture)
    {
        //Debug.Log("SendFurniture");
        List<Vector2> position = new List<Vector2>();
        List<float> YRotation = new List<float>();
        List<int> blockID = new List<int>();
        List<int> roomID = new List<int>();
        List<int> viewID = new List<int>();

        foreach (var obj in _furniture)
        {
            position.Add(obj.position);
            YRotation.Add(obj.YRotation);
            blockID.Add(obj.blockID);
            roomID.Add((int)obj.roomValue);
            viewID.Add(obj.viewID);
        }

        photonView.RPC("OnReceiveFurniture", RpcTarget.Others, furnitureIndex, (object)position.ToArray(), (object)YRotation.ToArray(), (object)blockID.ToArray(), (object)roomID.ToArray(), (object)viewID.ToArray());
    }

    public void SendOnReceiveBuildIDEnd()
    {
        //Debug.Log("SendOnReceiveBuildIDEnd");
        photonView.RPC("OnReceiveBuildIDEnd", RpcTarget.Others);
    }

    void EndLoadProcess()
    {
        gameManager.initScripts();
        SceneManager.UnloadSceneAsync(loadScene);
        loadCanvas.SetActive(false);
    }

    void EndLoadPool()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            state = LoadState.LoadID;
        }
        else
        {
            state = LoadState.LoadMap;
        }
    }

    void AddPlayerEndLoad(int actorNumber)
    {
        if (!PLayersEndLoad.Contains(actorNumber))
        {
            PLayersEndLoad.Add(actorNumber);
        }
    }

    #region RPC

    [PunRPC]
    public void OnPlayerLoadAllData(int _actorNumber)
    {
        //Debug.Log("Player " + _actorNumber + " has loaded all the pool ");
        AddPlayerEndLoad(_actorNumber);
    }

    [PunRPC]
    public void OnReceivePoolID(string _alimentName, AlimentState _alimentState, int _count, int[] _poolableIDArray)
    {
        //Debug.Log("i receive a pool : " + _alimentName + " / " + _alimentState + " / count : " + _count  + " nb1 ID = " + _poolableIDArray[0]);
        List<int> _poolableID = (_poolableIDArray).ToList();
        KeyValuePair<string, AlimentState> key = new KeyValuePair<string, AlimentState>(_alimentName, _alimentState);

        poolReceived.Add(key, _poolableID);
    }

    [PunRPC]
    public void OnLaunchGame()
    {
        loadCanvas.SetActive(false);

    }

    [PunRPC]
    public void OnReceiveCells(Vector2[] position, int[] index)
    {
        List<CellData> cells = new List<CellData>();

        for (int i = 0; i < position.Length; i++)
        {
            CellData obj = new CellData();
            obj.position = position[i].ToVector2Int();
            obj.index = index[i];
            cells.Add(obj);
        }

        resultData.cells = cells;
        //Debug.Log("OnReceiveCells");
    }

    [PunRPC]
    public void OnReceiveWalls(Vector2[] position, float[] YRotation)
    {
        List<ObjectData> walls = new List<ObjectData>();

        for (int i = 0; i < position.Length; i++)
        {
            ObjectData obj = new ObjectData();
            obj.position = position[i];
            obj.YRotation = YRotation[i];
            walls.Add(obj);
        }

        resultData.walls = walls;
        //Debug.Log("OnReceiveWalls");
    }

    [PunRPC]
    public void OnReceiveCorners(Vector2[] position, float[] YRotation)
    {
        List<ObjectData> corners = new List<ObjectData>();

        for (int i = 0; i < position.Length; i++)
        {
            ObjectData obj = new ObjectData();
            obj.position = position[i];
            obj.YRotation = YRotation[i];
            corners.Add(obj);
        }

        resultData.corners = corners.ToList();
        //Debug.Log("OnReceiveCorners");
    }

    [PunRPC]
    public void OnReceiveEntrances(int[] fromRoom, int[] toRoom, int[] type, Vector2[] position, float[] YRotation, int[] blockID, int[] viewID)
    {
        List<EntranceData> entrances = new List<EntranceData>();

        for (int i = 0; i < position.Length; i++)
        {
            EntranceData obj = new EntranceData();
            obj.objData = new ObjectData();
            obj.objData.position = position[i];
            obj.objData.YRotation = YRotation[i];
            obj.objData.blockID = blockID[i];
            obj.objData.viewID = viewID[i];
            obj.fromRoom = (BSGridCell.TileEnum)fromRoom[i];
            obj.toRoom = (BSGridCell.TileEnum)toRoom[i];
            obj.type = (Entrance.DoorType)type[i];

            entrances.Add(obj);
        }

        resultData.entrances = entrances.ToList();
        //Debug.Log("OnReceiveEntrances");

    }

    [PunRPC]
    public void OnReceiveFurniture(int furnitureIndex, Vector2[] position, float[] YRotation, int[] blockID, int[] roomID, int[] viewID)
    {
        List<ObjectData> furniture = new List<ObjectData>();

        for (int i = 0; i < position.Length; i++)
        {
            ObjectData obj = new ObjectData();
            obj.position = position[i];
            obj.YRotation = YRotation[i];
            obj.blockID = blockID[i];
            obj.roomValue = (BSGridCell.TileEnum)roomID[i];
            obj.viewID = viewID[i];
            furniture.Add(obj);
        }

        resultData.furniture[furnitureIndex] = furniture.ToList();
        //Debug.Log("OnReceiveFurniture");
    }

    [PunRPC]
    public void OnReceiveBuildIDEnd()
    {
        isDataBuildReceived = true;
        resultData.dataWasSet = true;
        //Debug.Log("OnReceiveBuildIDEnd");
    }

    [PunRPC]
    public void OnReceivePoolIDEnd()
    {
        isDataPoolReceived = true;
        //Debug.Log("OnReceivePoolIDEnd");
    }

    #endregion

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (loadScene.isLoaded)
        {
            if (PLayersEndLoad.Contains(otherPlayer.ActorNumber))
            {
                PLayersEndLoad.Remove(otherPlayer.ActorNumber);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (loadScene.isLoaded)
        {
            //Debug.Log("Master Leave during load all players leave");
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        if (loadScene.isLoaded)
        {
            Destroy(PoolManager.Instance.gameObject);
            if (PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("Lobby");
            }
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (loadScene.isLoaded)
        {
            SceneManager.LoadScene("ConnectionMenu");
        }
    }

    IEnumerator LoadProcess()
    {
        //Debug.Log("Start LoadProcess");

        //Debug.Log("Launch LoadPool");
        yield return StartCoroutine(LoadPool());

        //if (PhotonNetwork.CurrentRoom.PlayerCount > 1 && !PhotonNetwork.LocalPlayer.IsMasterClient)
        //{
        //    //Debug.Log("Launch LoadPoolID cause i'm not alone or i'm not the master");
        //    yield return StartCoroutine(LoadPoolID());
        //}

        //Debug.Log("Launch LoadMap");
        yield return StartCoroutine(LoadMap());

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //Debug.Log("Launch LoadMapBuildMaster");
            yield return StartCoroutine(LoadMapBuildMaster());
        }
        else
        {
            //Debug.Log("Launch LoadMapBuildClient");
            yield return StartCoroutine(LoadMapBuildClient());
        }

        yield return new WaitForSeconds(3.5f);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            state = LoadState.End;
            //Debug.Log("End Load Process");
            EndLoadProcess();
            yield break;
        }


        //Debug.Log("Launch WaitForPlayers");
        yield return StartCoroutine(WaitForPlayers());

        state = LoadState.End;
        //Debug.Log("End LoadProcess");
        EndLoadProcess();
    }

    IEnumerator LoadPool()
    {
        //Debug.Log("Execute LoadPool");
        poolManager.InitPool(ref loadStatesPos[(int)LoadState.LoadPool]);
        while (loadStatesPos[(int)LoadState.LoadPool] != 1)
        {
            yield return new WaitForFixedUpdate();
        }
        //Debug.Log("End LoadPool");
        //Debug.Log("Send OnReceivePoolIDEnd");
        photonView.RPC("OnReceivePoolIDEnd", RpcTarget.Others);
        EndLoadPool();
    }

    //IEnumerator LoadPoolID()
    //{
    //    //Debug.Log("Execute LoadPoolID");
    //    //Debug.Log("waiting for ID Data");

    //    while (!isDataPoolReceived)
    //    {
    //        yield return new WaitForFixedUpdate();
    //    }

    //    poolManager.SetPoolsViewId(poolReceived, ref loadStatesPos[(int)LoadState.LoadPool]);
    //    while (loadStatesPos[(int)LoadState.LoadPool] != 1)
    //    {
    //        yield return new WaitForFixedUpdate();
    //    }
    //    //Debug.Log("End LoadPoolID");
    //}

    IEnumerator LoadMap()
    {
        //Debug.Log("Execute LoadMap");
        loadScene = SceneManager.GetActiveScene();
        AsyncOperation operationAsync = SceneManager.LoadSceneAsync(resultData.linkedLevel, LoadSceneMode.Additive);

        while (!operationAsync.isDone)
        {
            loadStatesPos[(int)LoadState.LoadMap] = operationAsync.progress;
            yield return new WaitForFixedUpdate();
        }

        inGameScene = SceneManager.GetSceneByName(resultData.linkedLevel);
        SceneManager.SetActiveScene(inGameScene);

        mapCreator = BSMapCreator.Instance;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            state = LoadState.LoadMapBuild;
        }
        else
        {
            state = LoadState.WaitingBuildID;
        }

        //Debug.Log("End LoadMap");
    }

    IEnumerator LoadMapBuildMaster()
    {
        //Debug.Log("Execute LoadMapBuildMaster");
        mapCreator.InitiliasePrefabs();
        mapCreator.CreateMap(ref loadStatesPos[(int)LoadState.LoadMap]);
        while (loadStatesPos[(int)LoadState.LoadMap] != 1)
        {
            yield return new WaitForFixedUpdate();
        }

        BSIgGestion.SetColorsOnWalls(mapCreator.grid);
        BSIgGestion.FillWallNeighbors(mapCreator.grid);


        //Debug.Log("End LoadMapBuildMaster");
    }

    IEnumerator LoadMapBuildClient()
    {
        //Debug.Log("Execute LoadMapBuildClient");
        mapCreator.InitiliasePrefabs();
        while (!isDataBuildReceived)
        {
            yield return new WaitForFixedUpdate();
        }

        //Debug.Log("Received All Build ID");

        mapCreator.CreateMap(ref loadStatesPos[(int)LoadState.LoadMap]);

        while (loadStatesPos[(int)LoadState.LoadMap] != 1)
        {
            yield return new WaitForFixedUpdate();
        }

        BSIgGestion.SetColorsOnWalls(mapCreator.grid);
        BSIgGestion.FillWallNeighbors(mapCreator.grid);

        loadStatesPos[(int)LoadState.LoadMap] = 1;
        //Debug.Log("End LoadMapBuildClient");
    }

    IEnumerator WaitForPlayers()
    {
        //Debug.Log("Execute WaitForPlayers");

        photonView.RPC("OnPlayerLoadAllData", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber);

        while (PLayersEndLoad.Count != PhotonNetwork.CurrentRoom.PlayerCount - 1)
        {
            loadStatesPos[(int)LoadState.LoadMap] = (PLayersEndLoad.Count / PhotonNetwork.CurrentRoom.PlayerCount - 1);
            yield return new WaitForFixedUpdate();
        }
        loadStatesPos[(int)LoadState.LoadMap] = 1;

        //Debug.Log("End WaitForPlayers");
    }

    IEnumerator UpdateLoadScroll()
    {
            yield return new WaitForFixedUpdate();
    }

}
