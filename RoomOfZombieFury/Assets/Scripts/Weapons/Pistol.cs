using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun
{
    public override void Update()
    {
        base.Update();

        if (Input.GetButtonDown("Fire1"))
        {
            TryShoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }
    }
    public override void Shoot()
    {
        RaycastHit hit;
        Vector3 target = Vector3.zero;

        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        //if (Physics.Raycast(cameraTransform.position, cameraTransform.forward + d_currentRecoil, out hit, gunData.shootingRange, gunData.targetLayerMask))
        if (Physics.Raycast(ray, out hit, gunData.shootingRange, gunData.targetLayerMask))
        {
            Debug.Log(gunData.gunName + " hit " + hit.collider.name);
            target = hit.point;
        }
        else
        {
            //target = cameraTransform.position + (cameraTransform.forward + d_currentRecoil) * gunData.shootingRange;
            target = ray.origin + ray.direction * gunData.shootingRange;
        }

        StartCoroutine(BulletFire(target, hit));
    }

    private IEnumerator BulletFire(Vector3 target, RaycastHit hit)
    {
        GameObject bulletTrail = Instantiate(gunData.bulletTrailPrefab, gunMuzzle.position, Quaternion.identity);

        while (bulletTrail != null && Vector3.Distance(bulletTrail.transform.position, target) > 0.1f)
        {
            bulletTrail.transform.position = Vector3.MoveTowards(bulletTrail.transform.position, target, Time.deltaTime * gunData.bulletSpeed);
            yield return null;
        }
        Destroy(bulletTrail);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                BulletHitFX(hit);
            }
            else if (hit.collider.CompareTag("Enemy"))
            {
                ZombieAI enemy = hit.collider.GetComponent<ZombieAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(gunData.shootDamage);
                }
            }
        }
    }

    private void BulletHitFX(RaycastHit hit) 
    {
        Vector3 hitPosition = hit.point + hit.normal * 0.01f;

        GameObject bulletHole = Instantiate(bulletHolePrefab, hitPosition, Quaternion.LookRotation(hit.normal));
        //GameObject bulletParticle = Instantiate(bulletHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));

        bulletHole.transform.parent = hit.collider.transform;
        //bulletParticle.transform.parent = hit.collider.transform;

        Destroy(bulletHole, 5f);
        //Destroy(bulletParticle, 5f);
    }
}
