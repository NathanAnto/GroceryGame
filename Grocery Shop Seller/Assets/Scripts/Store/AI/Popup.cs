using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    private Transform currentTarget;

    private void Update()
    {
        Vector3 namePos = Camera.main.WorldToScreenPoint(currentTarget.position);
        gameObject.transform.position = namePos;
    }

    public void ActivatePopup(Transform target)
    {
        currentTarget = target;
        gameObject.SetActive(true);
    }
}
