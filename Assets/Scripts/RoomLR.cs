using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomLR : MonoBehaviour
{
    public GameObject[] SpawnPoints = { };

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate wall object from x -4.5 to 4.5 where there is a spawn point
        // with 1 x between each spawn point and y either -4.5 or 4.5
        for (float x = -4.5f; x <= 4.5f; x += 1.0f)
        {
            // Instantiate at (x, -4.5)
            Instantiate(SpawnPoints![0], new Vector3(x, -4.5f, 0), Quaternion.identity);

            // Instantiate at (x, 4.5)
            Instantiate(SpawnPoints![0], new Vector3(x, 4.5f, 0), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}