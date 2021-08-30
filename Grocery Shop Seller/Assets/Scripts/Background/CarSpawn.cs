using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarSpawn : MonoBehaviour
{
    [SerializeField] private GameObject[] cars;
    private Transform _transform;
    private ObjectPooler _objPooler;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _objPooler = ObjectPooler.instance;
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        for (var i = 0; i < 200; i++)
        {
            float interval = Random.Range(4, 10);

            var carTag = cars[Random.Range(0, cars.Length)].name;

            _objPooler.SpawnFromPool(carTag, _transform.position, _transform.rotation);
            
            yield return new WaitForSeconds(interval);
        }
    }
}
