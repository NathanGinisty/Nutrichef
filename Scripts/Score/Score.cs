using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Score
{
    public float noteFinal;
    public float[] notes;
    public List<Error> listError;
    public Dictionary<KeyValuePair<Type, int>, float> mapErrorValue;

    public enum Type
    {
        Hygiene,
        Nutrition,
        Building,
        COUNT
    }

    public enum HygieneCounter
    {
        WrongObjInColdRoom,
        FoodOnGround,
        DidntWashedHand, // /!\ Ajouter un bool isHandDirty dans le player
        CleanObjectInDirtyArea,
        NoOutfit,
        DecartonnageDirtyCardboard,
        COUNT
    }

    public enum NutritionCounter // Anciennement nutrition, sera a changé pour autre chose, en attendant, ce sera la catégorie "Autre"
    {
        BadListDelivery,
        COUNT
    }

    public enum BuilderCounter
    {
        RoomIsNonContiguous, // une piece non contigüe ( genre 5 block a coté et 1 tout seul)
        RoomMissing, // toutes les pieces n'ont pas ete posees
        RoomWithoutDoor, // toutes les pieces n ont pas au moins une porte
        RoomInaccessible, // toutes les pieces ne sont pas accessibles ( depuis l entree de la cuisine )
        ElementMissing, // il manque un meuble/outil
        COUNT
    }

    // ------------------------------------------

    public Score()
    {
        noteFinal = 100;
        notes = new float[(int)Type.COUNT];
        for (int i = 0; i < notes.Length; i++) notes[i] = 100;
        listError = new List<Error>();
        mapErrorValue = new Dictionary<KeyValuePair<Type, int>, float>();
    }

    /// <summary>
    /// set an errorValue
    /// </summary>
    public void SetErrorValue(HygieneCounter _ID, float _value)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(Type.Hygiene, (int)_ID);
        mapErrorValue.Add(tmpKey, _value);
    }

    public void SetErrorValue(NutritionCounter _ID, float _value)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(Type.Nutrition, (int)_ID);
        mapErrorValue.Add(tmpKey, _value);
    }

    public void SetErrorValue(BuilderCounter _ID, float _value)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(Type.Building, (int)_ID);
        mapErrorValue.Add(tmpKey, _value);
    }

    /// <summary>
    /// get an errorValue
    /// </summary>
    public float GetErrorValue(HygieneCounter _ID)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(Type.Hygiene, (int)_ID);
        return mapErrorValue[tmpKey];
    }

    public float GetErrorValue(NutritionCounter _ID)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(Type.Nutrition, (int)_ID);
        return mapErrorValue[tmpKey];
    }

    public float GetErrorValue(BuilderCounter _ID)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(Type.Building, (int)_ID);
        return mapErrorValue[tmpKey];
    }

    public float GetErrorValue(Type _type, int _ID)
    {
        KeyValuePair<Type, int> tmpKey = new KeyValuePair<Type, int>(_type, _ID);
        return mapErrorValue[tmpKey];
    }

    /// <summary>
    /// Add a new error to the list
    /// </summary>
    public void AddError(HygieneCounter _ID, Vector3Int _pos, string _info = null)
    {
        Error tmp = new Error();
        tmp.type = Type.Hygiene;
        tmp.ID = (int)_ID;
        tmp.t_date = GameManager.Instance.Score.GetTime();
        tmp.pos = _pos;
        tmp.info = _info != null ? _info : null;

        listError.Add(tmp);
    }

    public void AddError(NutritionCounter _ID, Vector3Int _pos, string _info = null)
    {
        Error tmp = new Error();
        tmp.type = Type.Nutrition;
        tmp.ID = (int)_ID;
        tmp.t_date = GameManager.Instance.Score.GetTime();
        tmp.pos = _pos;
        tmp.info = _info != null ? _info : null;

        listError.Add(tmp);
    }

    public void AddError(BuilderCounter _ID, Vector3Int _pos, string _info = null)
    {
        Error tmp = new Error();
        tmp.type = Type.Building;
        tmp.ID = (int)_ID;
        tmp.t_date = GameManager.Instance.Score.GetTime();
        tmp.pos = _pos;
        tmp.info = _info != null ? _info : null;

        listError.Add(tmp);
    }

    public void AddError(Type _type, int _ID, Vector3Int _pos, string _info = null)
    {
        Error tmp = new Error();
        tmp.type = _type;
        tmp.ID = _ID;
        tmp.t_date = GameManager.Instance.Score.GetTime();
        tmp.pos = _pos;
        tmp.info = _info != null ? _info : null;

        listError.Add(tmp);
    }

    // ------------------------------------------

    [System.Serializable]
    public class Error
    {
        public Type type;
        public int ID; // enum: HygieneCounter, NutritionCounter ou BuilderCounter

        public float t_date;
        public Vector3Int pos;
        [TextArea] public string info;
    }
}
