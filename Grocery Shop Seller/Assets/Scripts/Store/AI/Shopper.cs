using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Shopper : MonoBehaviour
{
    public enum AiState
    {
        Patrol = 0,
        Follow = 1,
        Pay = 2,
        Leave = 3
    }
    
    private NavMeshAgent agent;
    private Transform playerTransform;
    private GameObject[] waypoints;
    private Animator animator;
    private string currentAnimaton;
    private const int MAXSEARCHCOUNT = 3;
    private const int GROCERYCOUNT = 3;
    private int searchCount; // Track searches count to for the shopper to ask for help
    private Aisle currentAisle;
    
    public AiState CurrentState
    {
        get { return _currentState; }
        set
        {
            StopAllCoroutines();
            _currentState = value;

            switch (CurrentState)
            {
                case AiState.Patrol:
                    StartCoroutine(StatePatrol());
                    break;
                case AiState.Follow:
                    StartCoroutine(StateFollow());
                    break;
                case AiState.Pay:
                    StartCoroutine(StatePay());
                    break;
                case AiState.Leave:
                    StartCoroutine(StateLeave());
                    break;
            }
        }
    }

    public Aisle CurrentAisle
    {
        get { return currentAisle; }
        set
        {
            currentAisle = value;
        }
    }
    
    [SerializeField] private AiState _currentState = AiState.Patrol;
    [SerializeField] private GameObject currentWaypoint;
    [SerializeField] private List<Grocery> groceries;

    private void Awake()
    {
        groceries = new List<Grocery>();
        waypoints = GameObject.FindGameObjectsWithTag("Waypoints");
        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        searchCount = MAXSEARCHCOUNT;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentState = AiState.Patrol;
        groceries = GetRandGroceries();
    }

    #region State Routines
    public IEnumerator StatePatrol()
    {
        currentWaypoint = GetRandomWaypoint();

        while (CurrentState == AiState.Patrol)
        {
            agent.SetDestination(currentWaypoint.transform.position);

            if (ReachedDestination())
            {
                ChangeAnimationState("Idle");
                yield return new WaitForSeconds(Random.Range(3, 10));
                CheckForGroceries();
                currentWaypoint = GetRandomWaypoint();
            }
            ChangeAnimationState("Walk");

            yield return null;
        }
    }
    
    public IEnumerator StateFollow()
    {
        while (CurrentState == AiState.Follow)
        {
            agent.SetDestination(playerTransform.position);
            
            yield return null;
        }
    }

    public IEnumerator StatePay()
    {
        while (CurrentState == AiState.Pay)
        {
            Transform cashRegister = GameObject.Find("GroceryStore/Waypoints/Waypoint Checkout").transform;
            agent.SetDestination(cashRegister.position);

            if (ReachedDestination())
            {
                ChangeAnimationState("Idle");
                yield return new WaitForSeconds(3);
                CurrentState = AiState.Leave;
            }
            
            yield return null;
        }
    }
    
    public IEnumerator StateLeave()
    {
        while (CurrentState == AiState.Leave)
        {
            Transform exit = GameObject.Find("GroceryStore/Waypoints/Waypoint Exit").transform;
            agent.SetDestination(exit.position);

            if (ReachedDestination()) 
                Destroy(gameObject);
            
            yield return null;
        }
    }

    #endregion

    
    #region Functions

    private GameObject GetRandomWaypoint()
    {
        return waypoints[Random.Range(0, waypoints.Length)];
    }

    private void CheckForGroceries()
    {
        var removedGrocery = new Grocery();
        var foundItem = false;
        
        foreach (var grocery in groceries)
        {
            if (currentAisle.HasGrocery(grocery))
            {
                print($"HAS GROCERY {grocery.name}");
                removedGrocery = grocery;
                foundItem = true;
                searchCount = MAXSEARCHCOUNT;
            }
            else searchCount--;
        }
        if(foundItem)
            groceries.Remove(removedGrocery);
        
        if (groceries.Count <= 0)
            CurrentState = AiState.Pay;
    }

    private bool ReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<Grocery> GetRandGroceries()
    {
        List<Grocery> newGroceries = new List<Grocery>();
        var allGroceries = GroceryLoader.AllGroceries;
        
        for (int i = 0; i < 3; i++)
        {
            newGroceries.Add(allGroceries[Random.Range(0, allGroceries.Count)]);
        }

        return newGroceries;
    }
    
    private void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }    
    
    #endregion
}
