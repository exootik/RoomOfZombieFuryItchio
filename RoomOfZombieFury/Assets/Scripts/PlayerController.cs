using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AudioSource footstepSound;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintTransitionSpeed = 10f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float mouseSensibility;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private bool canDoubleJump = true; 

    private float verticalVelocity;
    private float currentSpeed;
    private float currentSpeedMultiplier;
    private float xRotation;

    [Header("Footstep Settings")]
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float stepInterval = 1f;

    private float nextStepTimer = 0;

    // Pour le tremblement de la camera :
    [Header("Camera Bob")]
    [SerializeField] private float bobFrequency = 1f;
    [SerializeField] private float bobAmplitude = 1f;

    private CinemachineBasicMultiChannelPerlin noiseComponent;

    // Pour les sons :
    [Header("SFX")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] gravelFootsteps;

    // Pour les Input du player :
    [Header("Input")]
    private float moveInput;
    private float turnInput;
    private float mouseX;
    private float mouseY;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        InputManagement();
        Movement();

        PlayFootstepSound();
    }

    private void LateUpdate()
    {
        // CameraBob();
    }

    private void Movement()
    {
        GroundMovement();
        Turn();
    }

    //Pour les mouvement du joueur
    private void GroundMovement()
    {
        Vector3 localMove = new Vector3(turnInput, 0, moveInput);
        Vector3 worldMove = transform.TransformDirection(localMove);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeedMultiplier = sprintSpeedMultiplier;
        }
        else
        {
            currentSpeedMultiplier = 1f;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitionSpeed * Time.deltaTime);

        worldMove.y = VerticalForceCalculation();
        controller.Move(worldMove * currentSpeed * Time.deltaTime);
    }

    //pour tourner la camera et le player :
    private void Turn()
    {
        mouseX *= mouseSensibility * Time.deltaTime;
        mouseY *= mouseSensibility * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }

    //Pour le tremblement de la camera :
    private void CameraBob()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            noiseComponent.m_AmplitudeGain = bobAmplitude * currentSpeedMultiplier;
            noiseComponent.m_FrequencyGain = bobFrequency * currentSpeedMultiplier;
        }
        else
        {
            noiseComponent.m_AmplitudeGain = 0.0f;
            noiseComponent.m_FrequencyGain = 0.0f;
        }
    }

    //Pour le son des bruits de pas
    private void PlayFootstepSound()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            if (Time.time >= nextStepTimer)
            {
                AudioClip[] footstepClips = DetermineAudioClips();

                if (footstepClips.Length > 0)
                {
                    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

                    footstepSound.PlayOneShot(clip);
                }

                nextStepTimer = Time.time + (stepInterval / currentSpeedMultiplier);
            }
        }
    }

    //Pour determiner quelle est le type de sol pour choisir le bon son :
    private AudioClip[] DetermineAudioClips()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.5f, terrainLayerMask))
        {
            string tag = hit.collider.tag;

            switch (tag)
            {
                case "Ground":
                    return groundFootsteps;
                case "Grass":
                    return grassFootsteps;
                case "Gravel":
                    return gravelFootsteps;
                default:
                    return groundFootsteps;
            }
        }
        return groundFootsteps;
    }
     
    // Pour la force de gravite :
    private float VerticalForceCalculation()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
                canDoubleJump = true;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            if (Input.GetButtonDown("Jump") && canDoubleJump)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
                canDoubleJump = false;
            }
        }

        return verticalVelocity;
    }

    // Pour tous les Input du player :
    private void InputManagement()
    { 
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
