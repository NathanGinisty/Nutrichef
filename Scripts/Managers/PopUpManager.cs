using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PopUpManager : MonoBehaviour
{
    [SerializeField] int nbInScene = 0;

    private Dictionary<string, Object> prefabs;
    private List<GameObject> popUpList;
    private Transform canvas;

    void Start()
    {
        prefabs = new Dictionary<string, Object>();

        foreach (Object obj in Resources.LoadAll<Object>("Objects/PopUps"))
        {
            prefabs.Add(obj.name, obj);
        }

        popUpList = new List<GameObject>();
        canvas = transform.GetChild(0);
    }

    void Update()
    {
        CleanPopUpList();
        nbInScene = popUpList.Count();
    }

    // --------------------------------------------- Private Methods -------------------------------------------- //

    private void CleanPopUpList()
    {
        popUpList.RemoveAll(x => x as GameObject == null);
    }

    // --------------------------------------------- Public Methods --------------------------------------------- //

    #region Text2D
    /// <summary>
    /// Create a PopUp Text in 2D environment and choose an animation.
    /// </summary>
    public GameObject CreateText(string str, int size, Vector2 pos, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs["text2D"] as GameObject, canvas);
        PopUp popUp = go.GetComponent<PopUp>();
        Text text = (popUp.component as Text);
        text.text = str;
        text.fontSize = size;
        popUp.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a PopUp Text in 2D environment and choose an animation (With rotation).
    /// </summary>
    public GameObject CreateText(string str, int size, Vector2 pos, Quaternion rota, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs["text2D"] as GameObject, canvas);
        PopUp popUp = go.GetComponent<PopUp>();
        Text text = (popUp.component as Text);
        text.text = str;
        text.fontSize = size;
        popUp.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        popUp.transform.localRotation = rota;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    #endregion

    #region Text3D
    /// <summary>
    /// Create a PopUp Text in 3D environment that DON'T FOLLOW THE CAMERA and maybe choose an animation.
    /// </summary>
    public GameObject CreateText3D(string str, int size, Vector3 pos, Quaternion rota, Transform parent, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs["text3D"] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        TextMesh text = (popUp.component as TextMesh);
        text.text = str;
        text.fontSize = size;
        popUp.transform.localPosition = pos;
        popUp.transform.localRotation = rota;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a PopUp Text in 3D environment that FOLLOW THE CAMERA and maybe choose an animation.
    /// </summary>
    public GameObject CreateText3D(string str, int size, Vector3 pos, Transform parent, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs["text3D"] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        TextMesh text = (popUp.component as TextMesh);
        text.text = str;
        text.fontSize = size;
        popUp.transform.localPosition = pos;
        popUp.followCam = true;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a PERMANENT Text in 3D environment that DON'T FOLLOW THE CAMERA and maybe choose an animation.
    /// </summary>
    public GameObject CreateText3D(string str, int size, Vector3 pos, Quaternion rota, Transform parent, string animName = null)
    {
        GameObject go = Instantiate(prefabs["text3D"] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        TextMesh text = (popUp.component as TextMesh);
        text.text = str;
        text.fontSize = size;
        popUp.transform.localPosition = pos;
        popUp.transform.localRotation = rota;
        popUp.hasTimer = false;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a PERMANENT Text in 3D environment that FOLLOW THE CAMERA and maybe choose an animation.
    /// </summary>
    public GameObject CreateText3D(string str, int size, Vector3 pos, Transform parent, string animName = null)
    {
        //GameObject go = Instantiate(Resources.Load<Object>("Objects/PopUps/text3D") as GameObject, parent);
        GameObject go = Instantiate(prefabs["text3D"] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        TextMesh text = (popUp.component as TextMesh);
        text.text = str;
        text.fontSize = size;
        popUp.transform.localPosition = pos;
        popUp.followCam = true;
        popUp.hasTimer = false;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    #endregion

    #region Prefab2D
    /// <summary>
    /// Create a prefabs in 2D environment, the name is the same as the one he has in the file.
    /// </summary>
    public GameObject CreatePrefab(string nameObj, Vector2 size, Vector2 pos, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs[nameObj] as GameObject, canvas);
        PopUp popUp = go.GetComponent<PopUp>();
        popUp.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        ((RectTransform)popUp.transform).sizeDelta = size;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a prefabs in 2D environment and choose an animation (With rotation).
    /// </summary>
    public GameObject CreatePrefab(string nameObj, Vector2 size, Vector2 pos, Vector2 rota, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs[nameObj] as GameObject, canvas);
        PopUp popUp = go.GetComponent<PopUp>();
        popUp.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        popUp.transform.localRotation = Quaternion.Euler(rota.x, rota.y, 0);
        ((RectTransform)popUp.transform).sizeDelta = size;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    #endregion

    #region Prefab3D

    /// <summary>
    /// Create a temporary prefabs in 3D environment that DON'T FOLLOW THE CAMERA, the name is the same as the one he has in the file.
    /// </summary>
    public GameObject CreatePrefab3D(string nameObj, Vector2 scale, Vector3 pos, Quaternion rota, Transform parent, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs[nameObj] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        popUp.transform.localPosition = pos;
        popUp.transform.localRotation = rota;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a temporary prefabs in 3D environment that FOLLOW THE CAMERA, the name is the same as the one he has in the file.
    /// </summary>
    public GameObject CreatePrefab3D(string nameObj, Vector2 scale, Vector3 pos, Transform parent, float duration, string animName = null)
    {
        GameObject go = Instantiate(prefabs[nameObj] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        popUp.transform.localPosition = pos;
        popUp.followCam = true;
        popUp.timer = duration;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }


    /// <summary>
    /// Create a permanent prefabs in 3D environment that DON'T FOLLOW THE CAMERA, the name is the same as the one he has in the file.
    /// </summary>
    public GameObject CreatePrefab3D(string nameObj, Vector2 scale, Vector3 pos, Quaternion rota, Transform parent, string animName = null)
    {
        GameObject go = Instantiate(prefabs[nameObj] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        popUp.transform.localPosition = pos;
        popUp.transform.localRotation = rota;
        popUp.hasTimer = false;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a permanent prefabs in 3D environment that FOLLOW THE CAMERA, the name is the same as the one he has in the file.
    /// </summary>
    public GameObject CreatePrefab3D(string nameObj, Vector2 scale, Vector3 pos, Transform parent, string animName = null)
    {
        GameObject go = Instantiate(prefabs[nameObj] as GameObject, parent);
        PopUp popUp = go.GetComponent<PopUp>();
        popUp.transform.localPosition = pos;
        popUp.followCam = true;
        popUp.hasTimer = false;
        if (animName != null) popUp.animator.Play(animName);

        popUpList.Add(go);

        return go;
    }

    #endregion

    #region Previsualization
    /// <summary>
    /// Create a previsualisation. Use Previsualization.Set() after.
    /// </summary>
    public Previsualization CreatePrevisualization(Transform _parent, bool _follow, bool _useInternalRaycast)
    {
        GameObject go = Instantiate(prefabs["Previsualization"] as GameObject, _parent);

        Previsualization prev = go.GetComponent<Previsualization>();
        prev.follow = _follow;
        prev.useInternalRaycast = _useInternalRaycast;

        popUpList.Add(go);

        return prev;
    }
    #endregion

    #region TextWindow

    /// <summary>
    /// Create a text window that set his size on the string size.
    /// </summary>
    [System.Obsolete("Don't use that for the moment, its in production, use the surcharge instead!")]
    public GameObject CreateTextWindow(string _str, int _fontSize, Vector2 _pos)
    {
        GameObject go = Instantiate(prefabs["textWindow2D"] as GameObject, canvas);
        UI_TextWindow2D textWindow = go.GetComponent<UI_TextWindow2D>();

        textWindow.SetAsTextPriority(_str, _fontSize);

        go.transform.localPosition = new Vector3(_pos.x, _pos.y, 0);

        popUpList.Add(go);

        return go;
    }

    /// <summary>
    /// Create a text window that set his font size on the window size.
    /// </summary>
    public GameObject CreateTextWindow(string _str, Vector2 _sizeWindow, Vector2 _pos)
    {
        GameObject go = Instantiate(prefabs["textWindow2D"] as GameObject, canvas);
        UI_TextWindow2D textWindow = go.GetComponent<UI_TextWindow2D>();

        textWindow.SetAsWindowPriority(_str, _sizeWindow);

        go.transform.localPosition = new Vector3(_pos.x, _pos.y, 0);

        popUpList.Add(go);

        return go;
    }

    #endregion

    #region SelectionFrame

    /// <summary>
    /// Create a selection frame. The method set is used during the creation.
    /// </summary>
    public SelectionFrame CreateSelectionFrame(Transform _parent, Vector3 _pos, Vector3 _size, Material _material = null)
    {
        GameObject go = Instantiate(prefabs["SelectionFrame"] as GameObject, _parent);
        SelectionFrame selectionFrame = go.GetComponent<SelectionFrame>();
        selectionFrame.Set(_pos, _size, _material);
    
        popUpList.Add(go);

        return selectionFrame;
    }

    #endregion
}
