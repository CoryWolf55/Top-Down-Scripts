/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ValidPlacementCheck : MonoBehaviour
{
    public BuildingPlacement BP;

    // cache last state to avoid repeated calls
    private bool lastBlocked = false;

    private void Update()
    {
        if (BP == null) return;

        // Compute the cell center based on the current transform and the BuildingPlacement cellSize
        float cell = BP.cellSize;
        Vector3 pos = transform.position;
        Vector3 cellCenter = new Vector3(
            Mathf.Round(pos.x / cell) * cell,
            pos.y,
            Mathf.Round(pos.z / cell) * cell
        );

        // Use an overlap box sized to the grid cell (slightly smaller than cell to avoid edge cases).
        // Ignore trigger colliders via QueryTriggerInteraction.Ignore (we don't want the preview's trigger to count).
        Vector3 halfExtents = new Vector3(cell * 0.49f, 5f, cell * 0.49f);
        Collider[] hits = Physics.OverlapBox(cellCenter, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

        bool blocked = false;
        foreach (var c in hits)
        {
            // Ignore the preview object itself (and its children)
            if (c.gameObject == gameObject) continue;
            if (c.transform.IsChildOf(transform)) continue;

            // Ignore common non-blocking things: ground, triggers (safety), terrain etc.
            if (c.isTrigger) continue;
            if (c.CompareTag("Ground")) continue;

            // If you have props or layers that should not block placement, add more ignores here:
            // e.g. if (c.CompareTag("Prop")) continue;

            // Any remaining collider inside the same cell => blocked
            blocked = true;
            break;
        }

        // only notify BP when state changes
        if (blocked != lastBlocked)
        {
            BP.OnTrigger(blocked);
            lastBlocked = blocked;
        }
    }

    // Visualize the cell being checked in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        if (BP == null) return;

        float cell = BP.cellSize;
        Vector3 pos = transform.position;
        Vector3 cellCenter = new Vector3(
            Mathf.Round(pos.x / cell) * cell,
            pos.y,
            Mathf.Round(pos.z / cell) * cell
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(cellCenter, new Vector3(cell, 0.01f, cell));
    }
}