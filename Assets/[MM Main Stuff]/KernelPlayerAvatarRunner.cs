using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class KernelPlayerAvatarRunner : MonoBehaviour
{
    public string assignedUsername;
    public bool isControlled = false;

    public Animator animator;

    private Rigidbody rb;
    private bool isGrounded = false;
    private bool jumpQueued = false;
    private float lastJumpTime = 0f;
    private float jumpCooldown = 0.25f;

    private List<Collider> collisions = new();
    private GameObject nameTextObj;
    private TextMeshPro nameText;

    private Vector3 forwardDir;

    private float targetX = 0f;
    private float sideStep = 1.5f;
    private float sideLerpSpeed = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!animator) { animator = GetComponentInChildren<Animator>(); }
        forwardDir = transform.forward;

        if (string.IsNullOrEmpty(assignedUsername))
        {
            assignedUsername = $"DevTester_{Random.Range(1000, 9999)}";
        }
    }

    void Start()
    {
        CreateNameLabel();

        sideStep = DinoPlayerSettings.Instance?.sideSpeed ?? 1.5f;
        targetX = transform.position.x;
    }

    void CreateNameLabel()
    {
        var settings = DinoPlayerSettings.Instance;

        nameTextObj = new GameObject("NameTag");
        nameTextObj.transform.SetParent(transform);
        nameTextObj.transform.localPosition = new Vector3(0f, settings?.nameTagYOffset ?? 2.5f, 0f);
        nameTextObj.transform.localScale = Vector3.one * (settings?.nameTagScale ?? 0.2f);

        nameText = nameTextObj.AddComponent<TextMeshPro>();
        nameText.text = assignedUsername;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize = settings?.nameFontSize ?? 4f;
        nameText.color = settings?.nameColor ?? Color.white;
        nameText.outlineColor = settings?.outlineColor ?? Color.black;
        nameText.outlineWidth = settings?.outlineWidth ?? 0.2f;
    }

    void Update()
    {
        if (nameTextObj && Camera.main)
        {
            nameTextObj.transform.rotation = Quaternion.LookRotation(
                nameTextObj.transform.position - Camera.main.transform.position
            );
        }
    }

    void FixedUpdate()
    {
        if (!isControlled || PlayerTracker.Instance == null) return;

        bool gameHasStarted = PlayerTracker.Instance.gameStarted;
        var settings = DinoPlayerSettings.Instance;

        animator?.SetBool("Grounded", isGrounded);

        float forwardSpeed = settings?.forwardSpeed ?? 2f;
        float turnSpeed = settings?.turnSpeed ?? 5f;

        if (gameHasStarted)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.fixedDeltaTime * sideLerpSpeed);
            Vector3 forward = forwardDir.normalized * forwardSpeed * Time.fixedDeltaTime;
            Vector3 nextPos = new Vector3(newX, transform.position.y, transform.position.z) + forward;

            rb.MovePosition(nextPos);
        }

        animator?.SetFloat("MoveSpeed", gameHasStarted ? forwardSpeed : 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.identity,
            Time.fixedDeltaTime * turnSpeed
        );

        if (jumpQueued && gameHasStarted)
        {
            TryJump();
            jumpQueued = false;
        }
    }

    public void Move(float vx, float vy, float speed)
    {
        if (!isControlled) return;

        Vector3 movementDirection = new Vector3(vx, 0f, vy);
        movementDirection.Normalize();

        transform.position += movementDirection * speed * Time.deltaTime;

        if (movementDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * sideLerpSpeed);
        }
    }

    private void TryJump()
    {
        var jumpForce = DinoPlayerSettings.Instance?.jumpForce ?? 5f;
        if (!isGrounded || Time.time - lastJumpTime < jumpCooldown) return;
        lastJumpTime = Time.time;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                if (!collisions.Contains(collision.collider)) collisions.Add(collision.collider);
                isGrounded = true;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        bool valid = false;
        foreach (var contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f) valid = true;
        }

        if (valid)
        {
            if (!collisions.Contains(collision.collider)) collisions.Add(collision.collider);
            isGrounded = true;
        }
        else
        {
            if (collisions.Contains(collision.collider)) collisions.Remove(collision.collider);
            if (collisions.Count == 0) isGrounded = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collisions.Contains(collision.collider)) collisions.Remove(collision.collider);
        if (collisions.Count == 0) isGrounded = false;
    }

    public void SetControl(bool enable)
    {
        isControlled = enable;
        Debug.Log($"[Avatar] SetControl({enable}) for {assignedUsername}");
    }
}
