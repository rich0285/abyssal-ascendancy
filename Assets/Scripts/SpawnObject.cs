using System.Linq;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject[] objects = { };
    // Start is called before the first frame update
    void Start()
    {
        if (objects!.Any())
        {
            Instantiate(objects[0], transform.position, Quaternion.identity);
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
