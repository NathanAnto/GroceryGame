using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Car : MonoBehaviour, IPooledObject
{
    private Transform _transform;
    public float speed;
    
    // Start is called before the first frame update
    public void OnObjectSpawn()
    {
        _transform = gameObject.transform;
        speed = Random.Range(4, 7);
    }

    // Update is called once per frame
    void Update()
    {
        _transform.position += _transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Vehicle"))
        {
            speed = collider.GetComponent<Car>().speed;
        }
    }
}
