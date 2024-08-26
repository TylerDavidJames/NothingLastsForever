using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowHMD : MonoBehaviour
{
    public Transform targetTransform; // Reference to the target transform
    public Transform targetHMD; // Distance from the target transform
    public Vector3 UIOffsetFromAnchor = Vector3.zero; // Offset from the target transform

    void Update()
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("Target Follow Transform is not assigned.");
            return;
        }

        // Get the gaze direction from the target transform
        Vector3 forwardDirection = targetTransform.position + UIOffsetFromAnchor;

        // Set the position of the UI element to follow the gaze
        transform.position = forwardDirection; // Adjust the distance as needed

        // Calculate the direction from the UI element to the target
        Vector3 lookDirection = targetHMD.position - transform.position;

        // Optionally, make the UI element face away from the target
        transform.rotation = Quaternion.LookRotation(-lookDirection);
    }
}
