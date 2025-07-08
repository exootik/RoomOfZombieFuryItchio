using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("Données & Animator")]
    public GunData gunData;
    public Transform gunMuzzle;
    public Animator animator;
    public GameObject bulletHolePrefab; 
    //public GameObject bulletHitParticlePrefab;
    public AudioSource audioSource;
    public PlayerController playerController;
    public Transform cameraTransform;

    private float currentAmmo = 0f;
    private float nextTimeToFire = 0f;

    //private Vector3 d_targetRecoil = Vector3.zero;
    //[HideInInspector] public Vector3 d_currentRecoil = Vector3.zero;
     
    private bool isReloading = false;

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentAmmo = gunData.magazineSize;

        playerController = transform.root.GetComponent<PlayerController>();
        //cameraTransform = playerController.virtualCamera.transform;
        cameraTransform = Camera.main.transform;

        audioSource = GetComponent<AudioSource>();
    }

    public virtual void Update()
    {
    //    playerController.ResetAimRecoil(gunData);
    //    ResetDirectionalRecoil();
    }

    public void TryReload()
    {
        if (!isReloading && currentAmmo < gunData.magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log(gunData.gunName + " is reloading....");
        animator.SetTrigger("Reload");

        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        animator.SetBool("IsEmpty", false);
        animator.SetBool("LastBullet", false);
        isReloading = false;

        Debug.Log(gunData.gunName + " is reloaded");
    }

    public void TryShoot()
    {
        if (isReloading || Time.time < nextTimeToFire)
        {
            return;
        }

        if (currentAmmo <= 0f)
        {
            animator.SetBool("IsEmpty", true);
            Debug.Log(gunData.gunName + " IsEmpty\", true");
            return;
        }
        if (currentAmmo == 1f)
        {
            animator.SetBool("LastBullet", true);
        }


        nextTimeToFire = Time.time + (1 / gunData.fireRate);
        HandleShoot();

    }

    private void HandleShoot()
    {
        animator.SetTrigger("Shoot");
        currentAmmo--;
        Debug.Log(gunData.gunName + " Shot! , Bullets left : " + currentAmmo);
        
        // playerController.ApplyAimRecoil(gunData);
        // ApplyDirectionalRecoil();

        PlayFireSound();
        Shoot();
    }

    //public void ApplyDirectionalRecoil()
    //{
    //    float recoilX = Random.Range(-gunData.d_maxRecoil.x, gunData.d_maxRecoil.x) * gunData.d_recoilAmount;
    //    float recoilY = Random.Range(-gunData.d_maxRecoil.y, gunData.d_maxRecoil.y) * gunData.d_recoilAmount;

    //    d_targetRecoil += new Vector3(recoilX, recoilY, 0);

    //    d_currentRecoil = d_targetRecoil;
    //}

    //public void ResetDirectionalRecoil()
    //{
    //    d_currentRecoil = Vector3.MoveTowards(d_currentRecoil, Vector3.zero, Time.deltaTime * gunData.d_resetRecoilSpeed);
    //    d_targetRecoil = Vector3.MoveTowards(d_targetRecoil, Vector3.zero, Time.deltaTime * gunData.d_resetRecoilSpeed);
    //}

    private void PlayFireSound()
    {
        if (gunData.fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gunData.fireSound);
        }
    }

    public abstract void Shoot();
}
