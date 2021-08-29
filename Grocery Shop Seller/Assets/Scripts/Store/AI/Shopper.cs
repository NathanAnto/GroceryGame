using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Aisle))]
public class Shopper : MonoBehaviour
{
    public enum AiState
    {
        Patrol = 0,
        Wait = 1,
        Follow = 2,
        Pay = 3,
        Leave = 4
    }

    public bool foundGrocery = false;
    public GameObject popupPrefab;
    private NavMeshAgent agent;
    private Player player;
    private GameObject[] waypoints;
    private Animator animator;
    private string currentAnimaton;
    private const int MAXSEARCHCOUNT = 3;
    private const int GROCERYCOUNT = 3;
    private int searchCount; // Track searches count to for the shopper to ask for help
    private Aisle currentAisle;
    private List<GameObject> popups;
    
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
                case AiState.Wait:
                    StartCoroutine(StateWait());
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
    [SerializeField] private Transform helpTransform;

    private void Awake()
    {
        popups = new List<GameObject>();
        groceries = new List<Grocery>();
        waypoints = GameObject.FindGameObjectsWithTag("Waypoints");
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        animator = GetComponent<Animator>();
        searchCount = MAXSEARCHCOUNT;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentState = AiState.Patrol;
        groceries = GetRandGroceries();
    }

    private void Update()
    {
        PlayAnimations();
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
                currentWaypoint = GetRandomWaypoint();
                yield return new WaitForSeconds(Random.Range(3, 10));
                CheckForGroceries();
            }
            
            if (searchCount <= 0)
                CurrentState = AiState.Wait;
            else
                agent.stoppingDistance = 0.5f;
            

            yield return null;
        }
    }

    public IEnumerator StateWait()
    {
        int timer = 15;
        
        while (CurrentState == AiState.Wait)
        {
            if (timer <= 0)
                CurrentState = AiState.Leave;
            timer--;
            yield return new WaitForSeconds(1);
        }
    }
    
    public IEnumerator StateFollow()
    {
        print("FOLLOWING PLAYER");
        player.SetFollower(this);
        
        if (popupPrefab)
        {
            ShowTarget(groceries[0]);
        }
        
        while (CurrentState == AiState.Follow)
        {
            if (foundGrocery)
            {
                agent.stoppingDistance = 0.5f;

                if (ReachedDestination())
                {
                    yield return new WaitForSeconds(3);
                    CurrentState = AiState.Patrol;
                }
            }
            else
            {
                agent.SetDestination(player.transform.position);
                agent.stoppingDistance = 2f;
                foundGrocery = false;
            }

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
            if (CurrentAisle && CurrentAisle.HasGrocery(grocery))
            {
                print($"HAS GROCERY {grocery.name}");
                removedGrocery = grocery;
                foundItem = true;
            }
        }

        if (foundItem)
        {
            groceries.Remove(removedGrocery);
            ResetSearch();
        }
        else searchCount--;

        if (groceries.Count <= 0)
            CurrentState = AiState.Pay;
    }
    
    // POP UP FUNCTIONS
    private void ShowTarget(Grocery grocery)
    {
        ShowPopup(helpTransform, grocery.name);
        ShowTargetAisle(grocery.aisleName);
    }
    
    private void ShowTargetAisle(string aisleName)
    {
        GameObject[] gameObjectAisles = GameObject.FindGameObjectsWithTag("Aisle");
        Aisle aisle = new Aisle();
        foreach (GameObject g in gameObjectAisles)
        {
            var a = g.GetComponent<Aisle>();
            if (a.GetName() == aisleName)
                aisle = a;
        }

        ShowPopup(aisle.transform, aisle.GetName());
    }

    private void ShowPopup(Transform transform, string text)
    {
        // Instantiate UI
        var popup = Instantiate(popupPrefab, transform.position, Quaternion.identity);
        // Set parent to canvas
        popup.transform.parent = GameObject.Find("Canvas").transform;
        // Activate Pop-up
        popup.GetComponent<Popup>().ActivatePopup(transform);
        var popupText = popup.transform.GetChild(0);
        popupText.GetComponent<TextMeshProUGUI>().text = text;
        
        popups.Add(popup);
    }

    public bool ReachedDestination()
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

    public void ResumeShopping(Grocery grocery, Transform aisleWaypoint)
    {
        foreach (var popup in popups)
        {
            Destroy(popup);
        }
        popups.Clear();
        groceries.Remove(grocery);
        foundGrocery = true;
        currentWaypoint = aisleWaypoint.gameObject;
        ResetSearch();
    }
    
    private void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }

    private void PlayAnimations()
    {
        if(ReachedDestination())
            ChangeAnimationState("Idle");
        else
            ChangeAnimationState("Walk");
    }

    public List<Grocery> GetGroceries()
    {
        return groceries;
    }

    public void ResetSearch()
    {
        searchCount = MAXSEARCHCOUNT;
    }
    
    #endregion
}
