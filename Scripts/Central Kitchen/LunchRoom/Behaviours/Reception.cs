using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reception : MonoBehaviour, IInteractive
{
    Customer customer;
    [SerializeField] Transform[] posReceptionArray = new Transform[4];
    public List<Customer> customerList = new List<Customer>();

    public Customer Customer { get => customer; set => customer = value; }
    public Transform[] PosReceptionArray { get => posReceptionArray; private set => posReceptionArray = value; }

    ReceptionUI receptionUI;

    private void Awake()
    {
        int arrIndex = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (arrIndex < posReceptionArray.Length)
            {
                Transform checkedTrans = transform.GetChild(i);
                if (checkedTrans.tag == "ReceptionWaitingLine")
                {
                    posReceptionArray[arrIndex] = checkedTrans;
                    arrIndex++;
                }
            }
            else
            {
                break;
            }
        }
    }

    private void Start()
    {
        receptionUI = GetComponentInChildren<ReceptionUI>();
    }

    public void PopCustomerOfList()
    {
        customerList.RemoveAt(0);
        for (int i = 0; i < customerList.Count; i++)
        {
            customerList[i].Agent.destination = posReceptionArray[i].position;
        }
    }

    /// <summary>
    /// Send a customer to a free table if possible.
    /// </summary>
    public void SendCustomerToTable()
    {
        Table table = TableManager.Instance.GetFreeTable();

        if (table != null)
        {
            if (customer.SetTable(table)) //  false if Agent not at reception
            {
                table.IsFree = false;
                PopCustomerOfList();
                customer = null;
                receptionUI.SetState(ReceptionUI.State.Hidden);
            }
        }
    }

    /// <summary>
    /// Don't take the order of the customer and send him to the exit.
    /// </summary>
    public void SendCustomerToExit()
    {      
        customer.GoToExit();
        PopCustomerOfList();
        customer = null;
        receptionUI.SetState(ReceptionUI.State.Hidden);
    }

    // Physics functions
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Customer>() != null)
        {
            //customer = other.GetComponent<Customer>();
            customer = customerList[0];
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (customer != null && other.GetComponent<PlayerMovement>() != null 
            && receptionUI.state != ReceptionUI.State.CustomerData 
            && TableManager.Instance.GetFreeTable() != null)
        {
            receptionUI.SetState(ReceptionUI.State.Interaction);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            if (customer != null)
            {
                customer.DesactivateUI();
                receptionUI.SetState(ReceptionUI.State.Hidden);
            }
        }
    }

    // Interface functions
    public void Interact(PlayerController pController)
    {
        if (customer != null && TableManager.Instance.GetFreeTable() != null)
        {
            receptionUI.SetState(ReceptionUI.State.CustomerData);           
        }
    }

    public void StopInteraction()
    {
        if (customer != null)
        {
            receptionUI.SetState(ReceptionUI.State.Interaction);
        }
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

	public bool CanInteract(PlayerController pController)
	{
		throw new System.NotImplementedException();
	}

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
