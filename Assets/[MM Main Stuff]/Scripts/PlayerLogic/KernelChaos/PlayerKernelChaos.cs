// [KC] Kernel Chaos - simple WASD mover with name label
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerKernelChaos : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Name Label (UGUI Text)")]
    public Text nameLabel; // World-space Canvas Text above avatar

    private Rigidbody _rb;
    private string _username = "Player";

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotation; // keep upright
    }

    public void Initialize(string username)
    {
        _username = string.IsNullOrWhiteSpace(username) ? "Player" : username;
        if (nameLabel != null) nameLabel.text = _username;
        Debug.Log($"🌽 [KC] Init player: {_username}");
    }

    void Update()
    {
        // Billboard the name to the main camera
        if (nameLabel != null && Camera.main != null)
        {
            nameLabel.transform.rotation = Quaternion.LookRotation(
                nameLabel.transform.position - Camera.main.transform.position
            );
        }
    }

    void FixedUpdate()
    {
        // Simple legacy input for WASD/Arrows
        float x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float z = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector3 input = new Vector3(x, 0f, z).normalized;
        Vector3 vel = input * moveSpeed;
        Vector3 worldVel = transform.TransformDirection(vel);

        // Preserve gravity on Y
        _rb.linearVelocity = new Vector3(worldVel.x, _rb.linearVelocity.y, worldVel.z);

        // Face movement direction
        if (input.sqrMagnitude > 0.001f)
        {
            Vector3 forward = new Vector3(worldVel.x, 0f, worldVel.z);
            if (forward.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(forward, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 0.2f);
            }
        }
    }
}
