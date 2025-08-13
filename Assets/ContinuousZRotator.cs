using UnityEngine;

public class ContinuousZRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationDuration = 4f; // Time in seconds for one full rotation

    [Header("Collision Settings")]
    [SerializeField] private string playerTag = "Player"; // Tag to identify the player

    private Vector3 initialRotation;
    private float currentTime = 0f;

    void Start()
    {
        // Store the initial rotation of the object
        initialRotation = transform.eulerAngles;
    }

    void Update()
    {
        // Increment time
        currentTime += Time.deltaTime;

        // Calculate the current rotation angle (0 to 360 degrees)
        float currentZRotation = (currentTime / rotationDuration) * 360f;

        // Reset time when we complete a full rotation to avoid floating point precision issues
        if (currentTime >= rotationDuration)
        {
            currentTime = 0f;
        }

        // Apply rotation while preserving initial X and Y rotations
        Vector3 targetRotation = new Vector3(
            initialRotation.x,
            initialRotation.y,
            initialRotation.z + currentZRotation
        );

        transform.eulerAngles = targetRotation;
    }

    // Called when another collider enters the trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Disable the player game object when collision occurs
            other.gameObject.SetActive(false);
        }
    }

    // Called when another collider enters (for non-trigger colliders)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            // Disable the player game object when collision occurs
            collision.gameObject.SetActive(false);
        }
    }
}


// Alternative smoother approach using Quaternion Lerp
/*
public class ContinuousZRotatorSmooth : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationDuration = 4f; // Time in seconds for one full rotation
    
    private Quaternion initialRotation;
    private float currentTime = 0f;
    
    void Start()
    {
        // Store the initial rotation as a quaternion
        initialRotation = transform.rotation;
    }
    
    void Update()
    {
        // Increment time
        currentTime += Time.deltaTime;
        
        // Calculate rotation progress (0 to 1)
        float progress = (currentTime / rotationDuration) % 1f;
        
        // Create target rotation (360 degrees on Z-axis)
        Quaternion zRotation = Quaternion.AngleAxis(progress * 360f, Vector3.forward);
        
        // Apply the rotation to initial rotation
        transform.rotation = initialRotation * zRotation;
        
        // Reset time to prevent overflow
        if (currentTime >= rotationDuration)
        {
            currentTime = 0f;
        }
    }
}
*/