using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour, IPooledObject
{
    private Transform _transform;
    public float speed = 1.5f;
    
    // Start is called before the first frame update
    public void OnObjectSpawn()
    {
        _transform = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        _transform.position += _transform.forward * speed * Time.deltaTime;
    }
}
