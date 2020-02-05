using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Dish : MonoBehaviour
{
    [SerializeField] string nameDish;
    [SerializeField] List<Aliment> listAliments;
    public ExpiryState expiryState;
    public float t_expiry;

    public Dish()
    {
        nameDish = "Empty";
        listAliments = new List<Aliment>();
    }

    public Dish(string _name, List<Aliment> _listAliments, float _t_expiry)
    {
        nameDish = _name;
        listAliments = _listAliments;
        t_expiry = _t_expiry;
    }


    public Dish(Dish _dish, float _t_expiry = 0f)
    {
        nameDish = _dish.name;
        listAliments = _dish.listAliments;
        t_expiry = _t_expiry == 0f ? _dish.t_expiry : _t_expiry;
    }

    void Start()
    {
    }

    void Update()
    {
        t_expiry -= Time.deltaTime;
    }

    /// <summary>
    /// Add aliment to the dish then change the name of the recipe
    /// </summary>
    public void AddAliment(Aliment _aliment)
    {
        bool hasFound;

        // Add new aliment
        Aliment newAliment = new Aliment();
        newAliment = _aliment;
        listAliments.Add(newAliment);

        // Find the recipe
        nameDish = FoodDatabase.GetDishRecipes(listAliments, out hasFound);

        if (hasFound)
        {
            //GetComponent<MeshRenderer>().materials = ;
            //GetComponent<MeshFilter>().mesh = ;
        }

        // Destroy the old aliment that is now in the dish
        if (_aliment.gameObject != null)
        {
            Destroy(_aliment.gameObject);
        }
    }


    /// <summary>
    /// Get nutriments of a Dish, use enum Nutriment
    /// </summary>
    public List<Nutriment> GetNutriments()
    {
        List<Nutriment> newListNutriments = new List<Nutriment>();

        bool isAlreadyInNewList = false;

        foreach (Aliment tmpAliment in listAliments)
        {
            foreach (Nutriment tmpNutrimentOfAliment in tmpAliment.nutriments)
            {
                foreach (Nutriment tmpNutrimentOfNewList in newListNutriments)
                {
                    if (tmpNutrimentOfAliment == tmpNutrimentOfNewList)
                    {
                        isAlreadyInNewList = true;
                        break;
                    }

                }

                if (!isAlreadyInNewList)
                {
                    newListNutriments.Add(tmpNutrimentOfAliment);
                }

                isAlreadyInNewList = false;
            }
        }

        return newListNutriments;
    }


}
