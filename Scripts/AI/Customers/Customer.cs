using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    Reception reception;

    Table actualTable;
    NavMeshAgent agent;
    NavMeshObstacle agentObstacle;

    LunchRoom lunchRoom;

    float timerRageQuit;

    public enum NameCustomer
    {
        David,
        Nathan,
        Guillaume,
        Clement,
        Laurent,
        Aurelia,
        Ophelie,
        Marine,
        Julien,
        COUNT
    }

    enum MovementState
    {
        GoToReception,
        AtReception,
        GoToTable,
        AtTable,
        GoToExit
    }

    public NameCustomer nameCLient { get; private set; }

    MovementState actualMovementState;

    public NavMeshAgent Agent { get => agent; }

    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentObstacle = GetComponent<NavMeshObstacle>();
        lunchRoom = LevelManager.Instance.LunchRoom;
        reception = lunchRoom.GetComponentInChildren<Reception>();

        // Affect initial position in reception
        reception.customerList.Add(this);
        agent.destination = reception.PosReceptionArray[reception.customerList.Count - 1].position;
        nameCLient = (NameCustomer)Random.Range(0, (int)NameCustomer.COUNT);

        timerRageQuit = 20;
    }

    void Update()
    {
        switch (actualMovementState)
        {
            case MovementState.GoToReception:
                UpdateGoToReception();
                break;
            case MovementState.AtReception:
                UpdateAtReception();
                break;
            case MovementState.GoToTable:
                UpdateGoToTable();
                break;
            case MovementState.AtTable:
                UpdateAtTable();
                break;
            case MovementState.GoToExit:
                UpdateGoToExit();
                break;
            default:
                break;
        }
    }

    bool IsAtDestination()
    {
        return agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;
    }

    void UpdateGoToReception()
    {
        if (IsAtDestination())
        {
            actualMovementState = MovementState.AtReception;
        }
    }

    void UpdateAtReception()
    {
        if (actualTable != null)
        {
            Vector3 posCHair = actualTable.ChairPos.position;
            agent.destination = posCHair;
            actualMovementState = MovementState.GoToTable;
        }

    }
    void UpdateGoToTable()
    {
        if (IsAtDestination())
        {
            actualMovementState = MovementState.AtTable;
            SetStatic(true);
        }
    }

    void UpdateAtTable()
    {
        if (timerRageQuit > 0)
        {
            timerRageQuit -= Time.deltaTime;
        }
        else
        {
            SetStatic(false);
            actualTable.IsFree = true;
            actualMovementState = MovementState.GoToExit;
            agent.destination = LevelManager.Instance.ExitTransform.position;
            agent.stoppingDistance = 1.5f;
        }
    }

    void UpdateGoToExit()
    {
        if (IsAtDestination())
        {
            Destroy(gameObject);
        }
    }

    void SetStatic(bool _state)
    {
        if (_state)
        {
            agent.enabled = false;
            agentObstacle.enabled = true;
        }
        else
        {
            agentObstacle.enabled = false;
            agent.enabled = true;
        }
    }

    public void ActivateUI()
    {
       
    }

    public void DesactivateUI()
    {
        
    }

    public bool SetTable(Table _table)
    {
        if (actualMovementState != MovementState.AtReception)
        {
            return false;
        }

        actualTable = _table;
        return true;
    }

    public void GoToExit()
    {
        actualMovementState = MovementState.GoToExit;
        agent.destination = LevelManager.Instance.ExitTransform.position;
        agent.stoppingDistance = 1.5f;
    }
}
