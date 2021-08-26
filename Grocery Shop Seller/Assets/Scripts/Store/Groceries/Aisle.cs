using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Aisle : MonoBehaviour
{
    [SerializeField] private string aisleName;
    [SerializeField] private Transform[] aisleWaypoints;
    
    [SerializeField] private List<Grocery> groceries;

    private void Awake()
    {
        groceries = (from item in GroceryLoader.AllGroceries
            where item.aisleName == aisleName
            select item).ToList();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shopper"))
        {
            other.GetComponent<Shopper>().CurrentAisle = this;
            print($"Current aisle {aisleName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shopper"))
        {
            other.GetComponent<Shopper>().CurrentAisle = null;
        }
    }

    public bool HasGrocery(Grocery grocery)
    {
        return groceries.Contains(grocery);
    }
}
