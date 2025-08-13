using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAvatar : MonoBehaviour {
    public string assignedUsername;
    public bool isControllable = false;

    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded = true;

    private float currentInput = 0f;
    private float inputTimer = 0f;
    private float inputDuration = 0.2f;
    private bool jumpRequested = false;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public void SetControl(bool enabled) {
        isControllable = enabled;
        Debug.Log($"[Avatar] SetControl({enabled}) for {assignedUsername}");
    }

    public void Move(string action) {
        if (!isControllable) {
            Debug.Log($"[Avatar] Ignored move '{action}' — {assignedUsername} is not controllable");
            return;
        }

        switch (action) {
            case "moveLeft":
                currentInput = -1f;
                inputTimer = inputDuration;
                Debug.Log($"[Avatar] {assignedUsername} moveLeft → velocity target = {-moveSpeed}");
                break;
            case "moveRight":
                currentInput = 1f;
                inputTimer = inputDuration;
                Debug.Log($"[Avatar] {assignedUsername} moveRight → velocity target = {moveSpeed}");
                break;
            case "jump":
                if (isGrounded) {
                    jumpRequested = true;
                    Debug.Log($"[Avatar] {assignedUsername} jump queued");
                }
                break;
            default:
                Debug.LogWarning($"[Avatar] Unknown action: {action}");
                break;
        }
    }

    void FixedUpdate() {
        if (!isControllable) return;

        if (inputTimer > 0f) {
            inputTimer -= Time.fixedDeltaTime;
        } else {
            currentInput = 0f;
        }

        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentInput * moveSpeed, velocity.y, 0f);

        if (jumpRequested && isGrounded) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            jumpRequested = false;
            Debug.Log($"[Avatar] {assignedUsername} performed jump");
        }

        Debug.Log($"[Avatar] {assignedUsername} velocity = {rb.linearVelocity}");
    }

    void OnCollisionEnter(Collision col) {
        foreach (var contact in col.contacts) {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f) {
                isGrounded = true;
                Debug.Log($"[Avatar] {assignedUsername} grounded");
                break;
            }
        }
    }
}
