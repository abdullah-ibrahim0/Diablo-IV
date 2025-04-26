using UnityEngine;

public class CamFollowPlayer : MonoBehaviour
{
    public Transform player; // Reference to the player
    public Vector3 offset; // Offset from the player
    public float smoothSpeed = 0.125f; // Smoothness factor

    void LateUpdate()
    {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(player); // Keeps the camera looking at the player
    }
}