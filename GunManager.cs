using System.Collections;
using System.Collections.Generic;
using TinyTanks.Code;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GunManager : MonoBehaviour
{
  public int currentWeaponIndex = 0;
  public GameObject[] WeaponModels;
  public WeaponBluprint[] weaponsBlu;
  public Button buyButton;
  public Button upgradeMsButton;
  public Button upgradeProjectileButton;
  [SerializeField]
  string pressButtonSound = "ButtonPress";
  AudioManagerNew audioManager;
  private GameObject currentGun; // Reference to the current gun GameObject
  
  void Start()
  {
      foreach (WeaponBluprint weapon in weaponsBlu)
      {
          if (weapon.price == 0)
              weapon.isUnlocked = true;
          else
              weapon.isUnlocked = PlayerPrefs.GetInt(weapon.name, 0) == 0 ? false : true;
      }
      currentWeaponIndex = PlayerPrefs.GetInt("GunSelected", 0);
      foreach (GameObject weapon in WeaponModels)
          weapon.SetActive(false);
      WeaponModels[currentWeaponIndex].SetActive(true);
      audioManager = AudioManagerNew.instance;
      if (audioManager == null)
      {
          Debug.Log("No audio manager found on scene");
      }
      currentGun = WeaponModels[currentWeaponIndex].GetComponentInChildren<Gun>().gameObject;
  }
  
  void Update()
  {
      UpdateUI();
  }
  
  public void ChangeNext()
  {
      audioManager.PlaySound(pressButtonSound);
      WeaponModels[currentWeaponIndex].SetActive(false);
      currentWeaponIndex++;
      if (currentWeaponIndex == WeaponModels.Length)
          currentWeaponIndex = 0;
      WeaponModels[currentWeaponIndex].SetActive(true);
      WeaponBluprint w = weaponsBlu[currentWeaponIndex];
      if (!w.isUnlocked)
          return;
      PlayerPrefs.SetInt("GunSelected", currentWeaponIndex);
      currentGun = WeaponModels[currentWeaponIndex].GetComponentInChildren<Gun>().gameObject;
  }

  
  public void ChangePrevious()
  {
      audioManager.PlaySound(pressButtonSound);
      WeaponModels[currentWeaponIndex].SetActive(false);
      currentWeaponIndex--;
      if (currentWeaponIndex == -1)
          currentWeaponIndex = WeaponModels.Length - 1;
      WeaponModels[currentWeaponIndex].SetActive(true);
      WeaponBluprint w = weaponsBlu[currentWeaponIndex];
      if (!w.isUnlocked)
          return;
      PlayerPrefs.SetInt("GunSelected", currentWeaponIndex);
      currentGun = WeaponModels[currentWeaponIndex].GetComponentInChildren<Gun>().gameObject;
  }
  

  public void UnlockWeapon()
  {
      audioManager.PlaySound(pressButtonSound);
      WeaponBluprint w = weaponsBlu[currentWeaponIndex];
      PlayerPrefs.SetInt(w.name, 1);
      PlayerPrefs.SetInt("GunSelected", currentWeaponIndex);
      w.isUnlocked = true;
      PlayerPrefs.SetInt("CoinsCollected", PlayerPrefs.GetInt("CoinsCollected", 0) - w.price);
  }
  
  public void UpgradeMsBetweenShots()
  {
      audioManager.PlaySound(pressButtonSound);
      Gun gun = currentGun.GetComponent<Gun>();
      gun.msBetweenShots -= 2;
      gun.upgradedMsBetweenShots = gun.msBetweenShots;
      PlayerPrefs.Save();
      PlayerData.SetUpgradedMsBetweenShots(gun.upgradedMsBetweenShots);
      weaponsBlu[currentWeaponIndex].upgradedMsBetweenShots = gun.upgradedMsBetweenShots;
  }
  
  public void UpgradeProjectilesPerMag()
  {
      audioManager.PlaySound(pressButtonSound);
      Gun gun = currentGun.GetComponent<Gun>();
      gun.projectilesPerMag += 2;
      gun.upgradedProjectilesPerMag = gun.projectilesPerMag;
      PlayerPrefs.Save();
      PlayerData.SetUpgradedProjectilesPerMag(gun.upgradedProjectilesPerMag);
      weaponsBlu[currentWeaponIndex].upgradedProjectilesPerMag = gun.upgradedProjectilesPerMag;
  }
  

  private void UpdateUI()
  {
      WeaponBluprint w = weaponsBlu[currentWeaponIndex];
      if (w.isUnlocked)
      {
          buyButton.gameObject.SetActive(false);
      }
      else
      {
          buyButton.gameObject.SetActive(true);
          buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy: " + w.price;
          if (w.price < PlayerPrefs.GetInt("CoinsCollected", 0))
          {
              buyButton.interactable = true;
              
          }
          else
          {
              buyButton.interactable = false;
          }
      }
  }
}
