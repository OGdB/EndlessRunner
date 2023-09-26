using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float followSpeed = 5f;
    private Vector3 initialOffset;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // Calculate and store the initial offset between the camera and player
        initialOffset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        // Calculate the new position based on the player's position and initial offset
        Vector3 targetPos = target.position + initialOffset;

        // Smoothly move the camera towards the new position
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
