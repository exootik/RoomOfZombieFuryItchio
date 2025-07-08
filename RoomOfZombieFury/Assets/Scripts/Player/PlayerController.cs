using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    //public CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AudioSource footstepSound;

    [Header("Player Health & Damage")]
    public int maxHealth = 10;
    public int currentHealth;
    public Image damagePanel;
    public float flashAlpha = 0.5f;
    public float fadeDuration = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintTransitionSpeed = 10f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float mouseSensibility;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private bool canDoubleJump = true;
    // [SerializeField] private float rotSpeed = 50f;

    private float verticalVelocity;
    private float currentSpeed;
    private float currentSpeedMultiplier;
    private float xRotation;

    // Pour le recul du viseur
    //[Header("Recoil")]
    //private Vector3 a_targetRecoil = Vector3.zero;
    //private Vector3 a_currentRecoil = Vector3.zero;
    //private GunData lastUsedGunData;

    [Header("Footstep Settings")]
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float stepInterval = 1f;

    private float nextStepTimer = 0;

    // Pour le tremblement de la camera :
    //[Header("Camera Bob")]
    //[SerializeField] private float bobFrequency = 1f;
    //[SerializeField] private float bobAmplitude = 1f;

    //private CinemachineBasicMultiChannelPerlin noiseComponent;

    // Pour les sons :
    [Header("SFX")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] gravelFootsteps;

    // Pour les Input du player :
    [Header("Input")]
    private float moveInput;
    private float turnInput;
    //private float mouseX;
    //private float mouseY;
    private float escape;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        //noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        currentHealth = maxHealth;
    }

    private void Update()
    {
        InputManagement();
        Movement();

        PlayFootstepSound();

        if (escape != 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        // CameraBob();
        //if (lastUsedGunData != null)
        //    ResetAimRecoil(lastUsedGunData);
    }

    private void Movement()
    {
        GroundMovement();
        //Turn();
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
    //private void Turn()
    //{
    //    mouseX *= mouseSensibility * Time.deltaTime;
    //    mouseY *= mouseSensibility * Time.deltaTime;

    //    xRotation -= mouseY;

    //    xRotation = Mathf.Clamp(xRotation, -90, 90);

    //    // virtualCamera.transform.localRotation = Quaternion.Slerp(virtualCamera.transform.localRotation, Quaternion.Euler(xRotation + a_currentRecoil.y, a_currentRecoil.x, 0), Time.deltaTime * rotSpeed);
    //    virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

    //    transform.Rotate(Vector3.up * mouseX);
    //}

    //public void ApplyAimRecoil(GunData gunData)
    //{
    //    lastUsedGunData = gunData;

    //    float recoilX = Random.Range(-gunData.a_maxRecoil.x, gunData.a_maxRecoil.x) * gunData.a_recoilAmount;
    //    float recoilY = Random.Range(-gunData.a_maxRecoil.y, gunData.a_maxRecoil.y) * gunData.a_recoilAmount;

    //    a_targetRecoil += new Vector3(recoilX, recoilY, 0);

    //    a_currentRecoil = Vector3.MoveTowards(a_currentRecoil, a_targetRecoil, Time.deltaTime * gunData.a_recoilSpeed);
    //}

    //public void ResetAimRecoil(GunData gunData)
    //{
    //    a_currentRecoil = Vector3.MoveTowards(a_currentRecoil, Vector3.zero, Time.deltaTime * gunData.a_resetRecoilSpeed);
    //    a_targetRecoil = Vector3.MoveTowards(a_targetRecoil, Vector3.zero, Time.deltaTime * gunData.a_resetRecoilSpeed);
    //}

    //Pour le tremblement de la camera :
    //private void CameraBob()
    //{
    //    if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
    //    {
    //        noiseComponent.m_AmplitudeGain = bobAmplitude * currentSpeedMultiplier;
    //        noiseComponent.m_FrequencyGain = bobFrequency * currentSpeedMultiplier;
    //    }
    //    else
    //    {
    //        noiseComponent.m_AmplitudeGain = 0.0f;
    //        noiseComponent.m_FrequencyGain = 0.0f;
    //    }
    //}

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
        //mouseX = Input.GetAxis("Mouse X");
        //mouseY = Input.GetAxis("Mouse Y");
        escape = Input.GetAxis("Cancel");
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        StopCoroutine("FlashRoutine");
        StartCoroutine("FlashRoutine");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player mort");
        Destroy(gameObject);
    }

    private IEnumerator FlashRoutine()
    {
        SetPanelAlpha(flashAlpha);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(flashAlpha, 0f, elapsed / fadeDuration);
            SetPanelAlpha(a);
            yield return null;
        }

        SetPanelAlpha(0f);
    }

    private void SetPanelAlpha(float a)
    {
        if (damagePanel == null) return;
        var color = damagePanel.color;
        color.a = a;
        damagePanel.color = color;
    }
}
