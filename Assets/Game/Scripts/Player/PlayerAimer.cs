using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimer : MonoBehaviour
{
    [HideInInspector] public Vector3 aimInput;
    [HideInInspector] public Vector3 angledAimInput;
    public float aimTurnInputThreshold = .1f;
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        angledAimInput = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up) * aimInput;
    }

    public void OnAim(InputValue value)
    {
        var inputValue = value.Get<Vector2>();
        aimInput = new Vector3(inputValue.x, 0f, inputValue.y);
    }
}