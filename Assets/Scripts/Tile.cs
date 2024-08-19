using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileType tileType; // e.g., "Room", "Corridor", etc.
    public bool connectsUp;
    public bool connectsDown;
    public bool connectsLeft;
    public bool connectsRight;
}

public enum TileType
{
    Room,
    Corridor,
}