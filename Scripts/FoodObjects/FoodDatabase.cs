using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Nutriment
{
    None = -1,
    Glucide,
    Lipid,
    Protein,
    Vitamins,
    Antioxidant,
    COUNT
}

public enum AlimentState
{
    None = -1,
    EmptyBox,
    Box,
    Stack,
    InContent,
    EmptyContent,
    Standard,
    Waste,
    Clean,
    Cut,
    Cooked,
    Sample,
    COUNT,
    Burned // maybe ?
}

public enum AlimentType
{
    Vegetable,
    Meat,
    Fish,
    BOF,
    Grocery,
    Starchy // Féculent
}

public enum ExpiryState
{
    Fresh,
    Good,
    Expired
}

public enum AlimentStepState
{
    Dirty,
    Deconditionning,
    Treated
}

public class FoodDatabase : MonoBehaviour
{
    [System.Serializable]
    public struct AlimentOfRecipe
    {
        public string name;
        public AlimentState state;
    }

    // ------------------------------------------------ DATABASE ----------------------------------------------- //
    public static Dictionary<string, AlimentObject> mapAlimentObject;
    public static Dictionary<KeyValuePair<string, AlimentState>, Sprite> mapSpriteAliment; // a voir, si besoin d'une réopti ------------@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    public static Dictionary<string, Recipe> mapRecipes;

    private static Object prefabCardboardBox;
    private static Object prefabStack;

    private static Object prefabEmptyObject;

    void Awake()
    {
        InitEmptyObject();
        InitCardboardBox();
        InitStack();
        InitMapAlimentObject();
        InitListRecipe();
    }

    // --------------------------------------------- Debug Methods --------------------------------------------- //
    #region DEBUG

    private void InitEmptyObject()
    {
        prefabEmptyObject = Resources.Load<Object>("Empty");
    }

    private void ShowMap()
    {
        foreach (KeyValuePair<string, AlimentObject> pair in mapAlimentObject)
        {
            Debug.Log(pair.Value.name);
        }
    }
    #endregion

    // -------------------------------------------- Aliment Methods -------------------------------------------- //
    #region ALIMENT

    private void InitMapAlimentObject()
    {
        mapAlimentObject = new Dictionary<string, AlimentObject>();

        AlimentObject[] tmpArray = Resources.LoadAll<AlimentObject>("Food/Datas/");

        for (int i = 0; i < tmpArray.Count(); i++)
        {
            mapAlimentObject.Add(tmpArray[i].name, tmpArray[i]);
        }

        InitMapSpriteAliment();
    }

    /// <summary>
    /// Create a dictionnary of sprite for different state of food
    /// </summary>
    private void InitMapSpriteAliment()
    {
        //KeyValuePair<string, AlimentState> newKey;

        //mapSpriteAliment = new Dictionary<KeyValuePair<string, AlimentState>, Sprite>();

        //foreach (KeyValuePair<string, AlimentObject> pair in mapAlimentObject)
        //{
        //    foreach (AlimentObject.InfoState info in pair.)
        //    {
        //        newKey = new KeyValuePair<string, AlimentState>(pair.Key, AlimentState.Standard);
        //        mapSpriteAliment.Add(newKey, pair.Value.listState[AlimentState.Standard].sprite);
        //    }
        //}


        mapSpriteAliment = new Dictionary<KeyValuePair<string, AlimentState>, Sprite>();

        KeyValuePair<string, AlimentState> newKey;

        foreach (AlimentObject alimentObject in mapAlimentObject.Values)
        {
            foreach (AlimentObject.InfoState infoState in alimentObject.listState)
            {
                newKey = new KeyValuePair<string, AlimentState>(alimentObject.name, infoState.state);
                mapSpriteAliment.Add(newKey, infoState.sprite);
            }
        }
    }

    private void InitListRecipe()
    {
        mapRecipes = new Dictionary<string, Recipe>();

        Recipe[] tmpArray = Resources.LoadAll<Recipe>("Food/Recipes/");

        for (int i = 0; i < tmpArray.Count(); i++)
        {
            mapRecipes.Add(tmpArray[i].name, tmpArray[i]);
        }
    }

    /// <summary>
    /// Instantiate an Aliment as GameObject
    /// </summary>
    public static GameObject InstantiateAliment(string _nameAliment, AlimentState _state)
    {
        if (mapAlimentObject.ContainsKey(_nameAliment))
        {
            // Create go

            Object toInstantiate = null;
            GameObject alimentGO = null;

            foreach (AlimentObject.InfoState infoState in mapAlimentObject[_nameAliment].listState)
            {
                if (infoState.state == _state)
                    toInstantiate = infoState.prefab;
            }

            if (toInstantiate != null)
            {
                alimentGO = Instantiate(toInstantiate) as GameObject;
            }

            // Set Aliment
            Aliment aliment = alimentGO.AddComponent<Aliment>();
            aliment.Init(mapAlimentObject[_nameAliment], _state);

            return alimentGO;
        }

        return null;
    }

    public static GameObject InstantiateAliment(AlimentObject _alimentObject, AlimentState _state)
    {
        if (_alimentObject != null)
        {

            // Create go

            Object toInstantiate = null;
            GameObject alimentGO = null;

            foreach (AlimentObject.InfoState infoState in _alimentObject.listState)
            {
                if (infoState.state == _state)
                    toInstantiate = infoState.prefab;
            }

            if (toInstantiate != null)
            {
                alimentGO = Instantiate(toInstantiate) as GameObject;
            }
            else
            {
                Debug.Log("_alimentObject: " + _alimentObject.name + " state " + _state);
            }

            // Set Aliment
            Aliment aliment = alimentGO.AddComponent<Aliment>();
            aliment.Init(_alimentObject, _state);

            return alimentGO;
        }

        //Debug.Log("Aliment " + _nameAliment + " doesn't exist !");
        return null;
    }

    /// <summary>
    /// Instantiate an random Aliment as GameObject
    /// </summary>
    public static GameObject InstantiateRandomAliment()
    {
        string[] arrayKey;
        string randKey;
        AlimentState randState;

        // randomize key
        arrayKey = mapAlimentObject.Keys.ToArray();
        randKey = arrayKey[Random.Range(0, arrayKey.Count())];
        randState = (AlimentState)Random.Range(0, (int)AlimentState.COUNT);

        GameObject go = InstantiateAliment(randKey, randState);
        //Debug.Log("Aliment " + randKey + " has spawn !");

        return go;
    }

    /// <summary>
    /// Search and find the most suiting recipes
    /// </summary>
    public static string GetDishRecipes(List<Aliment> _listAliments, out bool _hasFound)
    {
        // First Init
        string result;
        int nbSameAliment;

        // At each use of the methods
        nbSameAliment = 0;

        // Search
        foreach (KeyValuePair<string, Recipe> pair in mapRecipes)
        {
            // Continue searching if same nb of aliment
            if (pair.Value.listAlimentsOfRecipe.Count == _listAliments.Count)
            {
                foreach (AlimentOfRecipe alimentOfRecipe in pair.Value.listAlimentsOfRecipe)
                {
                    foreach (Aliment alimentOfDish in _listAliments)
                    {
                        if (alimentOfRecipe.name == alimentOfDish.alimentName &&
                            alimentOfRecipe.state == alimentOfDish.alimentState)
                        {
                            nbSameAliment++;
                        }
                    }
                }
            }

            // Found !
            if (nbSameAliment == pair.Value.listAlimentsOfRecipe.Count)
            {
                result = pair.Value.name;
                _hasFound = true;
                return result;
            }

            // Reset
            nbSameAliment = 0;
        }

        _hasFound = false;
        return result = "Weird Soup";
    }

    #endregion

    // -------------------------------------------- Stockage Methods ------------------------------------------- //
    #region STOCKAGE
    // -------------------------@ CARDBOARD BOX

    private void InitCardboardBox()
    {
        prefabCardboardBox = Resources.Load<Object>("Objects/Cardboard Box");
    }

    /// <summary>
    /// Instantiate a Cardboard Box based on an aliment
    /// </summary>
    public static GameObject InstantiateCardboardBox(float _t_expiry, AlimentObject _alimentObject)
    {
        GameObject go = Instantiate(prefabCardboardBox) as GameObject;
        go.GetComponent<CardboardBox>().Init(_t_expiry, _alimentObject);

        return go;
    }

    // -------------------------@ STACK

    private void InitStack()
    {
        prefabStack = Resources.Load<Object>("Objects/Stack");
    }

    /// <summary>
    /// Instantiate a Stack Of Aliments from the information of a CardboardBox then destroy it
    /// </summary>
    public static GameObject InstantiateStackFromCardboardBox(GameObject _go2)
    {
        GameObject go1 = Instantiate(prefabStack) as GameObject;

        CardboardBox cardboardBox = _go2.GetComponent<CardboardBox>();

        StackOfAliment stackOfAliment = go1.GetComponent<StackOfAliment>();
        stackOfAliment.Init(cardboardBox.alimentObject, cardboardBox.t_expiry);

        // Destruction dégueux pour le moment
        Destroy(_go2);

        return go1;
    }

    #endregion

}