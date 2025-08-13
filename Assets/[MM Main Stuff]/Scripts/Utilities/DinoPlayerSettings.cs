using UnityEngine;

public class DinoPlayerSettings : MonoBehaviour {
    public static DinoPlayerSettings Instance;

    [Header("Movement")]
    public float forwardSpeed = 2f;
    public float sideSpeed = 1.5f;
    public float jumpForce = 5f;
    public float turnSpeed = 5f;
    public float interpolation = 10f;

    [Header("Name Label")]
    public float nameTagYOffset = 2.5f;
    public float nameTagScale = 0.2f;
    public float nameFontSize = 4f;
    public Color nameColor = Color.white;
    public Color outlineColor = Color.black;
    public float outlineWidth = 0.2f;

    void Awake () {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
