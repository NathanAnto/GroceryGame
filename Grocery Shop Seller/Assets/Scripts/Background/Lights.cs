using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class Lights : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Transform child = transform.GetChild(0);
        GameObject child2 = child.transform.GetChild(0).gameObject;
        child2.SetActive(true);
    }
}
