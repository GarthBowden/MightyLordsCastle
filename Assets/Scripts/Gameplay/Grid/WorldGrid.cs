using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    public static WorldGrid Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Duplicate " + this + " detected. Deleting myself!");
            Destroy(this);
        }
    }

    [SerializeField] private Vector2 _GridSize;
    public Vector2 gridSize
    {
        get { return _GridSize; }
    }

    public Vector3 GridToWorldVector3 (int x, int y)
    {
        Vector3 w = new Vector3
        {
            x = (float)x * _GridSize.x,
            z = (float)y * _GridSize.y,
            y = 0.0f
        };
        return w;
    }
    public Vector3 GridToWorldVector3(WorldPlaceableObject.WorldGridPosition wgp)
    {
        return GridToWorldVector3(wgp.x, wgp.y);
    }

    public Vector2 WorldVector3ToGrid(Vector3 position)
    {
        float x_approx = position.x / _GridSize.x;
        float y_approx = position.z / _GridSize.y;
        return new Vector2
        {
            x = Mathf.RoundToInt(x_approx),
            y = Mathf.RoundToInt(y_approx)
        };
    }
}
