using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile[] upNeighbours;
    public Tile[] rightNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;

    [Range(1, 100)]
    public int weight = 1; // ← NEW: weight for probability

    private void Awake()
    {
        transform.localScale = Vector3.zero;

        transform.DOScale(Vector3.one, 1f)
            .SetEase(Ease.OutElastic);
    }
/*    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"W: {weight}");
    }*/

}
