using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] GameObject customerPrefab;
    public List<Customer> allCustomers = new List<Customer>();
    
    Reception reception;

    IEnumerator InstantiateCustomer()
    {
        if( reception.customerList.Count < reception.PosReceptionArray.Length)
        {
            GameObject customer = Instantiate(customerPrefab, LevelManager.Instance.LunchRoom.PosEnterOfRestaurant);
            customer.name = reception.customerList.Count.ToString();
            allCustomers.Add(customer.GetComponent<Customer>());
        }

        yield return new WaitForSeconds(2.0f);
        StartCoroutine("InstantiateCustomer");
    }

    // Update is called once per frame
    void Start()
    {
        reception = LevelManager.Instance.LunchRoom.GetComponentInChildren<Reception>();

        StartCoroutine("InstantiateCustomer");
    }
}
