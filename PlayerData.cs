using UnityEngine;


public class PlayerData : MonoBehaviour
{
    private const string UpgradedMsBetweenShotsKey = "UpgradedMsBetweenShots";
    private const string UpgradedProjectilesPerMagKey = "UpgradedProjectilesPerMag";


    public static float GetUpgradedMsBetweenShots()
    {
        return PlayerPrefs.GetFloat(UpgradedMsBetweenShotsKey, 0f);
    }


    public static void SetUpgradedMsBetweenShots(float value)
    {
        PlayerPrefs.SetFloat(UpgradedMsBetweenShotsKey, value);
        PlayerPrefs.Save();
    }


    public static int GetUpgradedProjectilesPerMag()
    {
        return PlayerPrefs.GetInt(UpgradedProjectilesPerMagKey, 0);
    }


    public static void SetUpgradedProjectilesPerMag(int value)
    {
        PlayerPrefs.SetInt(UpgradedProjectilesPerMagKey, value);
        PlayerPrefs.Save();
    }
}