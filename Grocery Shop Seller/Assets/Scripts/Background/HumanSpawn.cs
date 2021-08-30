using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HumanSpawn : MonoBehaviour
{
    [SerializeField] private GameObject[] humans;
    private Transform _transform;
    private ObjectPooler _objPooler;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        _transform = GetComponent<Transform>();
        _objPooler = ObjectPooler.instance;
        
        for (var i = 0; i < 200; i++)
        {
            float interval = Random.Range(2, 5);

            var humanTag = humans[0].name;

            _objPooler.SpawnFromPool(humanTag, _transform.position, _transform.rotation);
            
            yield return new WaitForSeconds(interval);
        }
    }
}
