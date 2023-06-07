using UnityEngine;
using System.Collections;
using EZCameraShake;
using TinyTanks.Code;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
public class Gun : MonoBehaviour
  {

     public enum FireMode { Auto, Burst, Single };
     public FireMode fireMode;
     [Header("GunSettings")]
     public Transform[] projectileSpawn;
     Projectile projectile;
     public float msBetweenShots = 100;
     public float muzzleVelocity = 35;
     public int burstCount;
     public int projectilesPerMag;
     public float reloadTime = .3f;
     public float MinYSpread = -1f;
     public float MaxYSpread = 1f;
     TextMeshProUGUI ammoText;
     [Header("Recoil")]
     public Vector2 kickMinMax = new Vector2(.05f, .2f);
     public Vector2 recoilAngleMinMax = new Vector2(3, 5);
     public float recoilMoveSettleTime = .1f;
     public float recoilRotationSettleTime = .1f;
     [Header("Effects")]
     public Transform shell;
     public Transform shellEjection;
     public AudioClip shootAudio;
     public AudioClip reloadAudio;
     AudioSource audioSource;
     MuzzleFlash muzzleflash;
     float nextShotTime;
     bool triggerReleasedSinceLastShot;
     int shotsRemainingInBurst;
     public int projectilesRemainingInMag;
     bool isReloading;
     Vector3 recoilSmoothDampVelocity;
     float recoilRotSmoothDampVelocity;
     float recoilAngle;
     
     [Header("CameraShake")]
     public float xShake = 0.7f;
     public float yShake = 0.7f;
     public float upgradedMsBetweenShots; // Upgraded value of msBetweenShots
     public int upgradedProjectilesPerMag; // Upgraded value of projectilesPerMag
     public float defaultMsBetweenShots;
     public int defaultProjectilesPerMag;
     public static Gun currentGun;
     
     void Start()
     {
        audioSource = GetComponent<AudioSource>();
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
        
        ammoText = GameObject.FindWithTag("AmmoText").GetComponent<TextMeshProUGUI>();
       
        defaultMsBetweenShots = msBetweenShots;
        defaultProjectilesPerMag = projectilesPerMag;
       
        // Load upgraded values from PlayerPrefs
        upgradedMsBetweenShots = PlayerPrefs.GetFloat("UpgradedMsBetweenShots", defaultMsBetweenShots);
        upgradedProjectilesPerMag = PlayerPrefs.GetInt("UpgradedProjectilesPerMag", defaultProjectilesPerMag);
       
        // Apply upgrades if available
        if (upgradedMsBetweenShots < defaultMsBetweenShots)
        {
           msBetweenShots -= (defaultMsBetweenShots - upgradedMsBetweenShots);
        }
        if (upgradedProjectilesPerMag >= defaultProjectilesPerMag)
        {
           //projectilesPerMag = upgradedProjectilesPerMag;
           projectilesPerMag = upgradedProjectilesPerMag;
           projectilesRemainingInMag = upgradedProjectilesPerMag;
        }
       
        ammoText.text = projectilesRemainingInMag.ToString();
     }
     
     void LateUpdate()
     {
        // animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
        
        if (!isReloading && projectilesRemainingInMag == 0)
        {
           Reload();
        }
     }

     void Shoot()
     {
        
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
        {
           if (fireMode == FireMode.Burst)
           {
              if (shotsRemainingInBurst == 0)
              {
                 return;
              }
              shotsRemainingInBurst--;
           }
           else if (fireMode == FireMode.Single)
           {
              if (!triggerReleasedSinceLastShot)
              {
                 return;
              }
           }

           for (int i = 0; i < projectileSpawn.Length; i++)
           {
              if (projectilesRemainingInMag == 0)
              {
                 break;
              }
              projectilesRemainingInMag--;
              nextShotTime = Time.time + msBetweenShots / 1000;
              //AmmoText gui
             
              //this is the object pooling shooting
              //this vat controlls the left and right spread
              var spread = Random.Range(MinYSpread, MaxYSpread);
          
              GameObject obj = ObjectPooler.current.GetPooledObject();
              if (obj == null) return;
              obj.transform.position = projectileSpawn[i].position;
              obj.transform.rotation = projectileSpawn[i].rotation * Quaternion.Euler(Random.Range(0.8f,-2f), spread, 0);
              obj.SetActive(true);




              CameraShaker.Instance.ShakeOnce(xShake, yShake, 0.07f, 0.07f);
           }
          
           //Instantiate(shell, shellEjection.position, shellEjection.rotation);
           GameObject obj1 = ObjectPoolingShell.current.GetPooledObject1();
           if (obj1 == null) return;
           obj1.transform.position = shellEjection.position;
           obj1.transform.rotation = shellEjection.rotation;
           obj1.SetActive(true);
          
           muzzleflash.Activate();
           transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
           recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
           recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
           audioSource.PlayOneShot(shootAudio);
        }
       
     }
     private void Update()
  {
     UpdateAmmo(0);
     ammoText.text = projectilesRemainingInMag.ToString();
  }
  
  public void Reload()
     {
        if (!isReloading && projectilesRemainingInMag != upgradedProjectilesPerMag)
        {
           StartCoroutine(AnimateReload());
        }
     }
  IEnumerator AnimateReload()
     {
        isReloading = true;
        yield return new WaitForSeconds(.2f);
        audioSource.PlayOneShot(reloadAudio);
       
        float reloadSpeed = 1.2f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;


        while (percent < 1)
        {
           percent += Time.deltaTime * reloadSpeed;
           float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
           float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
           transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
           yield return null;
        }
       
        isReloading = false;
        projectilesRemainingInMag = upgradedProjectilesPerMag;
        ammoText.text = projectilesRemainingInMag.ToString();
     }
     
     public void Aim(Vector3 aimPoint)
     {
        if (!isReloading)
        {
           transform.LookAt(aimPoint);
        }
     }
     
     public void OnTriggerHold()
     {
        Shoot();
        triggerReleasedSinceLastShot = false;
     }
     public void OnTriggerRelease()
     {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
     }
     public void UpdateAmmo(int ammo)
     {
        projectilesRemainingInMag += ammo;
        if (projectilesRemainingInMag > upgradedProjectilesPerMag)
        {
           projectilesRemainingInMag = upgradedProjectilesPerMag;
        }
        ammoText.text = projectilesRemainingInMag.ToString(); // Update the ammo text immediately
        if (isReloading && projectilesRemainingInMag == upgradedProjectilesPerMag)
        {
           isReloading = false;
        }
     }








}
