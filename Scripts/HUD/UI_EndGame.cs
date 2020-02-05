using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class UI_EndGame : MonoBehaviour
{
    public struct ErrorsAndTransform
    {
        public List<Score.Error> listError;
        public Transform obj;
    }

    [Header("General Panel")]
    [SerializeField] Transform generalPanel;
    [SerializeField] Transform errorTransform;
    [Header("Detail Panel")]
    [SerializeField] Transform detailPanel;
    [Header("Objects")]
    [SerializeField] GameObject goErrorLine;

    private Dictionary<KeyValuePair<Score.Type, int>, ErrorsAndTransform> mapError;
    private Transform[] linesTransform;
    private Dictionary<string, Sprite> mapSprite;

    private UI_TextWindow2D textWindow;
    private Animator animator;

    private bool firstEnable = true;

    private void OnEnable()
    {
        if (firstEnable)
        {
            //CreateAllError();
            firstEnable = false;
        }

        Init();
        GetLinesTransform();
        GameManager.Instance.FreeMouse();
        //InitTextWindow();

    }

    private void OnDisable()
    {
        RemoveAllLines();
    }

    private void Update()
    {
        //ShowErrorInfo();

    }

    // --------------------------------- PRIVATE METHODS --------------------------------- //

    private void Init()
    {
        animator = GetComponent<Animator>();

        mapSprite = new Dictionary<string, Sprite>();

        foreach (Sprite tmp in Resources.LoadAll<Sprite>("Errors"))
        {
            mapSprite.Add(tmp.name, tmp);
        }

        mapError = new Dictionary<KeyValuePair<Score.Type, int>, ErrorsAndTransform>();

        foreach (Score.Error error in GameManager.Instance.Score.myScore.listError)
        {
            // Create the Key
            KeyValuePair<Score.Type, int> key = new KeyValuePair<Score.Type, int>(error.type, error.ID);

            if (!mapError.ContainsKey(key))
            {
                // Create GameObject
                GameObject go = Instantiate(goErrorLine, errorTransform);
                go.name = "Error Line (" + GetNameForError(error) + ")";
                go.SetActive(true);

                // Create the Value
                ErrorsAndTransform value = new ErrorsAndTransform();
                value.listError = new List<Score.Error>();
                value.obj = go.transform;

                mapError.Add(key, value);

                Transform childName = value.obj.transform.GetChild(0);
                childName.GetComponent<Text>().text = " " + GetNameForError(error);

            }

            // Add error into the list
            mapError[key].listError.Add(error);

            Transform childValue = mapError[key].obj.GetChild(1);
            childValue.GetComponent<Text>().text = "x" + mapError[key].listError.Count;
        }
    }

    private void GetLinesTransform()
    {
        linesTransform = new RectTransform[errorTransform.childCount];
        linesTransform = errorTransform.GetChilds();
    }

    private void InitTextWindow()
    {
        //GameObject go = GameManager.Instance.PopUp.CreateTextWindow("", new Vector2(400,600), Vector2.zero);
        GameObject go = GameManager.Instance.PopUp.CreateTextWindow("", 30, Vector2.zero);
        textWindow = go.GetComponent<UI_TextWindow2D>();
        textWindow.SetAsTextPriority("", 30);
        go.SetActive(false);
    }

    private void ShowErrorInfo()
    {
        for (int i = 0; i < errorTransform.childCount; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(linesTransform[i] as RectTransform, Input.mousePosition))
            {
                foreach (ErrorsAndTransform value in mapError.Values)
                {
                    if (linesTransform[i] == value.obj)
                    {
                        textWindow.gameObject.SetActive(true);
                        textWindow.SetText(value.listError[0].info);
                        textWindow.SetPos(Input.mousePosition);
                        return;
                    }
                }
            }
        }

        textWindow.gameObject.SetActive(false);
    }

    private void RemoveAllLines()
    {
        foreach (RectTransform line in linesTransform)
        {
            Destroy(line.gameObject);
        }
        linesTransform = null;

        HideDetail();
    }

    // --- Tools

    private List<Score.Error> GetErrorFromButton(Transform _buttonTransform)
    {
        foreach (ErrorsAndTransform value in mapError.Values)
        {
            if (_buttonTransform == value.obj)
            {
                return value.listError;
            }
        }
        return null;
    }

    private Sprite GetSpriteForError(Score.Error error)
    {
        switch (error.type)
        {
            case Score.Type.Hygiene:
                return mapSprite[((Score.HygieneCounter)error.ID).ToString()];
            case Score.Type.Nutrition:
                return mapSprite[((Score.NutritionCounter)error.ID).ToString()];
            case Score.Type.Building:
                return mapSprite[((Score.BuilderCounter)error.ID).ToString()];
            default:
                return null;
        }
    }

    private string GetTextForError(Score.Error error)
    {
        if (error.type == Score.Type.Hygiene)
        {
            Score.HygieneCounter newID = (Score.HygieneCounter)error.ID;

            switch (newID)
            {
                case Score.HygieneCounter.WrongObjInColdRoom:
                    return "Un objet qui n'est pas censé être dans la chambre froide y a été entreposé.";
                case Score.HygieneCounter.FoodOnGround:
                    return "De la nourriture est tombé sur le sol.";
                case Score.HygieneCounter.DidntWashedHand:
                    return "Les mains n'ont pas été nettoyées et désinfectées avant d'effectuer une action sur un objet propre.";
                case Score.HygieneCounter.CleanObjectInDirtyArea:
                    return "Un objet propre est entré dans une zone salle.";
                case Score.HygieneCounter.NoOutfit:
                    return "Le cuisinier ne portait pas son uniforme lors d'une activité de cuisine.";
                case Score.HygieneCounter.DecartonnageDirtyCardboard:
                    return "Décartonnage d'un carton étant ni au norme, ni en bonne état.";
                default:
                    return Application.isEditor == true ? "NULL ID HYGIENE" : "";
            }
        }

        else if (error.type == Score.Type.Nutrition)
        {
            Score.NutritionCounter newID = (Score.NutritionCounter)error.ID;

            switch (newID)
            {
                case Score.NutritionCounter.BadListDelivery:
                    return "La livraison est incorrecte, la vérification de la commande à mal été faite.";
                default:
                    return Application.isEditor == true ? "NULL ID NUTRITION" : "";
            }
        }

        else if (error.type == Score.Type.Building)
        {
            Score.BuilderCounter newID = (Score.BuilderCounter)error.ID;

            switch (newID)
            {
                case Score.BuilderCounter.RoomIsNonContiguous:
                    return "Il y a plusieurs fois la meme salle.";
                case Score.BuilderCounter.RoomMissing:
                    return "Une salle est manquante.";
                case Score.BuilderCounter.RoomWithoutDoor:
                    return "Une salle n'a pas de porte.";
                case Score.BuilderCounter.RoomInaccessible:
                    return "Une salle est inaccessible.";
                case Score.BuilderCounter.ElementMissing:
                    return "Un atelier de travail est manquant.";
                default:
                    return Application.isEditor == true ? "NULL ID BUILDING" : "";
            }
        }

        return Application.isEditor == true ? "NULL TYPE" : "";
    }

    private string GetNameForError(Score.Error error)
    {
        if (error.type == Score.Type.Hygiene)
        {
            Score.HygieneCounter newID = (Score.HygieneCounter)error.ID;

            switch (newID)
            {
                case Score.HygieneCounter.WrongObjInColdRoom:
                    return "Mauvais objet en chambre froide";
                case Score.HygieneCounter.FoodOnGround:
                    return "Nourriture au sol";
                case Score.HygieneCounter.DidntWashedHand:
                    return "Main pas lavée";
                case Score.HygieneCounter.CleanObjectInDirtyArea:
                    return "Aliment propre en zone sale";
                case Score.HygieneCounter.NoOutfit:
                    return "Oubli de l'uniforme";
                case Score.HygieneCounter.DecartonnageDirtyCardboard:
                    return "Décartonnage invalide";
                default:
                    return Application.isEditor == true ? "NULL ID HYGIENE" : "";
            }
        }

        else if (error.type == Score.Type.Nutrition)
        {
            Score.NutritionCounter newID = (Score.NutritionCounter)error.ID;

            switch (newID)
            {
                case Score.NutritionCounter.BadListDelivery:
                    return "Mauvaise verification des cartons";
                default:
                    return Application.isEditor == true ? "NULL ID NUTRITION" : "";
            }
        }

        else if (error.type == Score.Type.Building)
        {
            Score.BuilderCounter newID = (Score.BuilderCounter)error.ID;

            switch (newID)
            {
                case Score.BuilderCounter.RoomIsNonContiguous:
                    return "Salle multiple";
                case Score.BuilderCounter.RoomMissing:
                    return "Salle manquante";
                case Score.BuilderCounter.RoomWithoutDoor:
                    return "Salle sans porte";
                case Score.BuilderCounter.RoomInaccessible:
                    return "Salle inaccessible";
                case Score.BuilderCounter.ElementMissing:
                    return "Atelier de travail manquant";
                default:
                    return Application.isEditor == true ? "NULL ID BUILDING" : "";
            }
        }

        return Application.isEditor == true ? "NULL TYPE" : "";
    }

    private void HideErrorLine(Transform _errorLine)
    {
        _errorLine.GetComponent<Image>().enabled = false;
        foreach (Transform child in _errorLine.GetChilds())
        {
            child.gameObject.SetActive(false);
        }
    }

    private void ShowErrorLine(Transform _errorLine)
    {
        _errorLine.GetComponent<Image>().enabled = true;
        foreach (Transform child in _errorLine.GetChilds())
        {
            child.gameObject.SetActive(true);
        }
    }

    // --- Debug Tools

    private void CreateAllError()
    {
        for (int i = 0; i < (int)Score.HygieneCounter.COUNT; i++)
        {
            GameManager.Instance.Score.myScore.AddError((Score.HygieneCounter)i, Vector3Int.zero, "test erreur lolilol");
        }
        for (int i = 0; i < (int)Score.BuilderCounter.COUNT; i++)
        {
            GameManager.Instance.Score.myScore.AddError((Score.BuilderCounter)i, Vector3Int.zero, "test erreur lolilol");
        }
        for (int i = 0; i < (int)Score.NutritionCounter.COUNT; i++)
        {
            GameManager.Instance.Score.myScore.AddError((Score.NutritionCounter)i, Vector3Int.zero, "test erreur lolilol");
        }
    }

    // --------------------------------- PUBLIC METHODS ---------------------------------- //

    public void ShowDetail(Transform _buttonTransform)
    {
        // Show the detail panel and hide general panel
        if (!animator.GetBool("isOpen"))
        {
            animator.Play("Open Detail");
            animator.SetBool("isOpen", true);
        }

        detailPanel.gameObject.SetActive(true);

        // Get Error value
        List<Score.Error> errors = GetErrorFromButton(_buttonTransform);

        // Give Value to the detail panel
        if (errors != null)
        {
            Text text = detailPanel.Find("Text").GetComponent<Text>();
            text.text = GetTextForError(errors[0]);

            Image image = detailPanel.Find("Image Error").GetComponent<Image>();
            image.sprite = GetSpriteForError(errors[0]);

            Text title = detailPanel.Find("Title").GetComponent<Text>();
            title.text = GetNameForError(errors[0]);
        }
        else
        {
            Debug.LogError("That's is not supposed to happen !!!");
        }
    }

    public void HideDetail()
    {
        if (animator.GetBool("isOpen"))
        {
            animator.Play("Close Detail");
            animator.SetBool("isOpen", false);
        }

        detailPanel.gameObject.SetActive(false);
    }

    public void ReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
        GameManager.Instance.FreeMouse();
        if (PhotonNetwork.OfflineMode)
        {
            SceneManager.LoadScene("Lobby");
            return;
        }
    }
}
