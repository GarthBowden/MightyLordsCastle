using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType : int { Closed, DeadEnd, Corner, Passage, OneWall, Open}
    [SerializeField]
    public TileType ttype;

}
