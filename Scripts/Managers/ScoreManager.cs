using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


public class ScoreManager : MonoBehaviour
{
    public enum SaveScoreMethod
    {
        BestTotalScore,
        BestInvidualValues,
        NoComparaison,
        COUNT
    }

    [Header("File")]
    [SerializeField] string adressFile = "Scores/";
    [SerializeField] string tmpPseudoPlayer = "Charles"; // temporaire, il faudra accéder au pseudo du joueur
    [SerializeField] string nameFile = "_score.dat";

    [Header("Timer")]
    [SerializeField] float t_inScene;
    [SerializeField] float t_gameStarted;

    [Header("List Error")]
    public Score myScore;

    void Start()
    {
        InitMapScores();
        InitCounter(true);
        InitTime();
    }

    void Update()
    {
        UpdateTime();
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        ResetTimeInScene();

        // Reset Score
        if (scene == SceneManager.GetSceneByName("Lobby"))
        {
            InitMapScores();
        }
    }


    // --------------------------------------------- Private Methods -------------------------------------------- //

    #region PRIVATE

    void InitMapScores()
    {
        // Load saved score from file
        //myScore = FileManager.Load<Score>(adressFile + tmpPseudoPlayer + nameFile);

        // if not exist, create
        //if (myScore == null || myScore == default(Score))
        //{
        myScore = new Score();
        //}

        //  SaveScore(SaveScoreMethod.BestTotalScore);
    }

    void InitCounter(bool _verify)
    {
        myScore.SetErrorValue(Score.HygieneCounter.WrongObjInColdRoom, 4f); // Integré
        myScore.SetErrorValue(Score.HygieneCounter.FoodOnGround, 2.5f); // Integré
        myScore.SetErrorValue(Score.HygieneCounter.DidntWashedHand, 3f); // Integré
        myScore.SetErrorValue(Score.HygieneCounter.CleanObjectInDirtyArea, 3.5f); // Integré
        myScore.SetErrorValue(Score.HygieneCounter.NoOutfit, 3f); // Integré
        myScore.SetErrorValue(Score.HygieneCounter.DecartonnageDirtyCardboard, 4f); // Integré

        myScore.SetErrorValue(Score.NutritionCounter.BadListDelivery, 2f); // Integré

        myScore.SetErrorValue(Score.BuilderCounter.RoomIsNonContiguous, 1.5f); // Integré
        myScore.SetErrorValue(Score.BuilderCounter.RoomMissing, 3f); // Integré
        myScore.SetErrorValue(Score.BuilderCounter.RoomWithoutDoor, 4f); // Integré
        myScore.SetErrorValue(Score.BuilderCounter.RoomInaccessible, 5f); // Integré
        myScore.SetErrorValue(Score.BuilderCounter.ElementMissing, 4f); // Integré

        // VERIF if all ErrorValue are set
        if (_verify)
        {
            if (Application.isEditor)
            {
                for (int i = 0; i < (int)Score.HygieneCounter.COUNT; i++)
                    myScore.GetErrorValue((Score.HygieneCounter)i);

                for (int i = 0; i < (int)Score.NutritionCounter.COUNT; i++)
                    myScore.GetErrorValue((Score.NutritionCounter)i);

                for (int i = 0; i < (int)Score.BuilderCounter.COUNT; i++)
                    myScore.GetErrorValue((Score.BuilderCounter)i);
            }
        }
    }

    #endregion


    // --------------------------------------------- Public Methods --------------------------------------------- //

    #region GESTION LOCAL

    /// <summary>
    /// Get score class.
    /// </summary>
    public Score Get()
    {
        return myScore;
    }

    public void Set(Score _score)
    {
        myScore = _score;
    }

    /// <summary>
    /// Add Score a player.
    /// </summary>
    public void AddValueToNote(Score.Type _type, float _value)
    {
        myScore.notes[(int)_type] += _value;
    }

    public void SetValueToNote(Score.Type _type, float _value)
    {
        myScore.notes[(int)_type] = _value;
    }

    /// <summary>
    /// Save the variable myScore in a local file.
    /// </summary>
    public void SaveScore(SaveScoreMethod _saveScoreMethod)
    {
        // Get
        Score bestScore = FileManager.Load<Score>(adressFile + tmpPseudoPlayer + nameFile);

        if (bestScore == null || bestScore == default(Score))
        {
            FileManager.Save<Score>(adressFile + tmpPseudoPlayer + nameFile, myScore);
            return;
        }

        if (_saveScoreMethod == SaveScoreMethod.BestTotalScore)
        {
            // Compare
            bestScore = bestScore.noteFinal > myScore.noteFinal ? bestScore : myScore;

            //Set
            FileManager.Save<Score>(adressFile + tmpPseudoPlayer + nameFile, bestScore);

        }
        else if (_saveScoreMethod == SaveScoreMethod.BestInvidualValues)
        {
            // Compare

            // a faire après, pas utile pour le moment

            //Set
            FileManager.Save<Score>(adressFile + tmpPseudoPlayer + nameFile, bestScore);
        }
        else if (_saveScoreMethod == SaveScoreMethod.NoComparaison)
        {
            //Set
            FileManager.Save<Score>(adressFile + tmpPseudoPlayer + nameFile, myScore);
        }
    }

    /// <summary>
    /// Update every notes then return
    /// </summary>
    public float[] UpdateNotePerType()
    {
        // reset notes
        for (int i = 0; i < myScore.notes.Length; i++) myScore.notes[i] = 100; // Valeur au pif, score qui descend de 100 selon le nombre d'erreur (???)

        // calcul it
        foreach (Score.Error error in myScore.listError)
        {
            AddValueToNote(error.type, -myScore.GetErrorValue(error.type, error.ID));
        }

        return myScore.notes;
    }

    /// <summary>
    /// Update a note then return
    /// </summary>
    public float UpdateNote(Score.Type _type)
    {
        // reset
        myScore.notes[(int)_type] = 0;

        // calcul it
        foreach (Score.Error error in myScore.listError)
        {
            if (error.type == _type)
            {
                AddValueToNote(error.type, -myScore.GetErrorValue(error.type, error.ID));
            }
        }

        return myScore.notes[(int)_type];
    }

    /// <summary>
    /// Update then return
    /// </summary>
    public float UpdateNoteFinal()
    {
        UpdateNotePerType();

        myScore.noteFinal = 0;

        for (int i = 0; i < myScore.notes.Length; i++)
        {
            myScore.noteFinal += myScore.notes[i];
        }

        myScore.noteFinal /= myScore.notes.Length;

        return myScore.noteFinal;
    }

    #endregion

    #region TIMER

    private void InitTime()
    {
        t_inScene = 0f;
        t_gameStarted = 0f;
    }

    private void UpdateTime()
    {
        t_inScene += Time.deltaTime;
        t_gameStarted += Time.deltaTime;
    }

    public void ResetTimeInScene()
    {
        t_inScene = 0f;
    }

    public float GetTime()
    {
        return t_inScene;
    }

    #endregion
}
