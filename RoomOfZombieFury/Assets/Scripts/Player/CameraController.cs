using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Références")]
    public Transform playerTransform;

    [Header("Sensibilité et limites")]
    public float sensibility = 1f;
    public float minXAngle = -90f;
    public float maxXAngle = 90f;

    public float smoothSpeed = 10f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibility;
        float mouseY = Input.GetAxis("Mouse Y") * sensibility;

        rotationY += mouseX;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minXAngle, maxXAngle);

        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}
