using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
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

    public bool foundGrocery;
    public GameObject popupPrefab;
    
    [SerializeField] private AiState currentState;
    [SerializeField] private GameObject currentWaypoint;
    [SerializeField] private List<Grocery> groceries;
    [SerializeField] private Transform helpTransform;
    
    private NavMeshAgent agent;
    private Player player;
    private GameObject[] waypoints;
    private Animator animator;
    private string currentAnimaton;
    private const int MAXSEARCHCOUNT = 3;
    private const int GROCERYCOUNT = 3;
    private int searchCount; // Track searches count to for the shopper to ask for help
    private List<GameObject> popups;
    private Grocery groceryTarget;

    public AiState CurrentState
    {
        get => currentState;
        set
        {
            StopAllCoroutines();
            currentState = value;

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

    public Aisle CurrentAisle { get; set; }
    
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        waypoints = GameObject.FindGameObjectsWithTag("Waypoints");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        popups = new List<GameObject>();
        groceries = new List<Grocery>();
        searchCount = MAXSEARCHCOUNT;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentState = AiState.Patrol;
        groceries = GetRandGroceries();
    }

    private void Update() => PlayAnimations();

    #region State Routines
    private IEnumerator StatePatrol()
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
                CurrentState = IsListEmpty() ? AiState.Pay : AiState.Wait;
            else
                agent.stoppingDistance = 0.5f;

            yield return null;
        }
    }

    private IEnumerator StateWait()
    {
        var timer = 15;
        if (popupPrefab) 
            ShowTarget(groceries[0]);
        
        while (CurrentState == AiState.Wait)
        {
            if (timer <= 0)
                CurrentState = AiState.Leave;
            timer--;
            yield return new WaitForSeconds(1);
        }
    }
    
    private IEnumerator StateFollow()
    {
        ShowTargetAisle(groceryTarget.aisleName);
        print("FOLLOWING PLAYER");
        player.SetFollower(this);
        foundGrocery = false;
        
        while (CurrentState == AiState.Follow)
        {
            if (foundGrocery)
            {
                if (ReachedDestination())
                {
                    yield return new WaitForSeconds(3);
                    CurrentState = IsListEmpty() ? AiState.Pay : AiState.Patrol;
                }
            }
            else
            {
                agent.SetDestination(player.transform.position);
                agent.stoppingDistance = 2f;
            }

            yield return null;
        }
    }

    private IEnumerator StatePay()
    {
        while (CurrentState == AiState.Pay)
        {
            var cashRegister = GameObject.Find("GroceryStore/Waypoints/Waypoint Checkout").transform;
            agent.SetDestination(cashRegister.position);

            if (ReachedDestination())
            {
                yield return new WaitForSeconds(3);
                CurrentState = AiState.Leave;
            }

            yield return null;
        }
    }
    
    private IEnumerator StateLeave()
    {
        foreach (var popup in popups) {
            Destroy(popup);
        }
        
        while (CurrentState == AiState.Leave)
        {
            var exit = GameObject.Find("GroceryStore/Waypoints/Waypoint Exit").transform;
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
    }

    private bool IsListEmpty()
    {
        return groceries.Count <= 0;
    }
    
    // POP UP FUNCTIONS
    private void ShowTarget(Grocery grocery)
    {
        ShowPopup(helpTransform, grocery.name);
        groceryTarget = grocery;
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

    private void ShowPopup(Transform tr, string text)
    {
        // Instantiate UI
        var popup = Instantiate(popupPrefab, tr.position, Quaternion.identity);
        // Set parent to canvas
        popup.transform.parent = GameObject.Find("Canvas").transform;
        // Activate Pop-up
        popup.GetComponent<Popup>().ActivatePopup(tr);
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
        agent.stoppingDistance = 0.5f;
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
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CurrentState == AiState.Wait)
        {
            CurrentState = AiState.Follow;
        }
    }
    
    #endregion
}
