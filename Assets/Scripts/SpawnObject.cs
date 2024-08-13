using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public List<GameObject> objects = [];
    // Start is called before the first frame update
    void Start()
    {
        if (objects.Any())
        {
            Instantiate(objects[0]);
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
