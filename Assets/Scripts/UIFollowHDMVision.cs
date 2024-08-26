using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowHDMVision : MonoBehaviour
{
    public Transform objectThatFollows; // The object to follow
    public Camera HMDCamera; // Reference to the main camera

    private Vector3 targetPosition; // The position to move towards
    public float distanceFromHMD = 5f; // The speed at which the object moves

    private bool isVisible = false; // Flag to indicate if the object is currently being followed
    private bool following = false; // Flag to indicate if the object is currently following the target object
    private void Update()
    {
        // Check if the object that follows is in view of the camera
        if (!following && !IsObjectInView())
        {
            // Stop following the target object when the object is no longer visible
            isVisible = false;
            following = true;
            StartCoroutine(FollowTarget());
        }
    }

    // Method to check if the object that follows is in view of the camera
    private bool IsObjectInView()
    {
        // Get the object's position in viewport coordinates
        Vector3 viewportPosition = HMDCamera.WorldToViewportPoint(objectThatFollows.position);

        // Check if the object is within the viewport
        bool isInView = (viewportPosition.x >= 0 && viewportPosition.x <= 1 && viewportPosition.y >= 0 && viewportPosition.y <= 1 && viewportPosition.z > 0);

        return isInView;
    }

    // Coroutine to move the object towards the camera
    IEnumerator FollowTarget()
    {
        while (!isVisible)
        {
            // Calculate the direction towards the camera
            targetPosition = HMDCamera.transform.position + (HMDCamera.transform.forward * distanceFromHMD); // Set the target position to the camera position

            // Move towards target position
            objectThatFollows.position = Vector3.Lerp(objectThatFollows.position, targetPosition, Time.deltaTime * 5f);
            //rotate the object towards the camera
            Vector3 lookDirection = HMDCamera.transform.position - objectThatFollows.position;
            objectThatFollows.rotation = Quaternion.LookRotation(-lookDirection);

            if (Vector3.Distance(objectThatFollows.position, targetPosition) < 0.1f)
            {
                // Stop following the target object when it is close enough to the camera
                isVisible = true;
                following = false;
            }

            yield return null;
        }
    }
}
