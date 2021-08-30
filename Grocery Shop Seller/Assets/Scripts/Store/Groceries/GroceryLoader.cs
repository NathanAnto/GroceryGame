using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GroceryLoader : MonoBehaviour
{    
    public static List<Grocery> AllGroceries { get; private set; }
    public TextAsset jsonFile;
    
    private static List<Grocery> allGroceries;

    private void Awake()
    {
        Groceries groceriesJson = JsonUtility.FromJson<Groceries>(jsonFile.text);
        AllGroceries = groceriesJson.groceries.ToList();
    }
}
