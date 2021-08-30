using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Aisle : MonoBehaviour
{
    [SerializeField] private string aisleName;
    [SerializeField] private Transform[] aisleWaypoints;

    [SerializeField] private List<Grocery> groceries;

    private Player player;

    private void Start()
    {
        groceries = (from item in GroceryLoader.AllGroceries
            where item.aisleName == aisleName
            select item).ToList();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Set Shopper current aisle
        if (other.CompareTag("Shopper"))
        {
            if(other.GetComponent<Shopper>().CurrentAisle == null)
                other.GetComponent<Shopper>().CurrentAisle = this;
        }
        
        // Check if player found correct aisle
        if (other.CompareTag("Player") && player.ShopperFollower)
        {
            var grocery = player.GetFollowerGrocery();
            
            if (groceries.Contains(grocery))
            {
                print("FOUND CORRECT AISLE");
                player.ShopperFollower.foundGrocery = true;
                player.ShopperFollower.ResumeShopping(grocery, aisleWaypoints[0]);
                player.ShopperFollower = null;
            }
            else 
                print("FOUND WRONG AISLE");
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

    public string GetName()
    {
        return aisleName;
    }

}
