using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public Tile[] tileOptions;

    public int gridX;
    public int gridY;

    public void CreateCell(bool collapseState, Tile[] tiles)
    {
        collapsed = collapseState;
        tileOptions = tiles;
    }

    public void RecreateCell(Tile[] tiles)
    {
        tileOptions = tiles;
    }

/*    void OnDrawGizmos()
    {
        Vector3 pos = transform.position + Vector3.up * 0.25f;
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.Label(pos, $"({gridX}, {gridY})");
    }
*/
}

