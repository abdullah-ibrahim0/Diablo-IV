using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target; // The target the camera follows
    public float smoothSpeed = 8f; // The speed at which the camera will follow the target
    public Vector3 offset; // The offset between the camera and the target (e.g., the camera should be above and behind the player)

    void Start()
    {
        // Find the active Wanderer in the scene (there should only be one active Wanderer)
    }

    void Update()
    { 
        GameObject[] wanderers = GameObject.FindGameObjectsWithTag("Wanderer");
        foreach (GameObject wanderer in wanderers)
        {
            // Check if the Wanderer is active in the hierarchy
            if (wanderer.activeInHierarchy)
            {
                Debug.Log("A7777777777777777777777777777777AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                target = wanderer.transform; // Set the camera target to the active Wanderer
                break; // Exit loop once the active Wanderer is found
            }
        }

        if (target == null)
        {
            Debug.LogError("No active Wanderer found in the scene.");
        }
        // If the target is null (i.e., no Wanderer is active), do nothing
        if (target == null) return;

        // Calculate the desired position of the camera
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Update the camera's position
        transform.position = smoothedPosition;
    }
}