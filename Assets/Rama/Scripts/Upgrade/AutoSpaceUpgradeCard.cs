using UnityEngine;

public class AutoSpaceUpgradeCard : UpgradeCardBehaviour
{
    public override void Upgrade()
    {
        base.Upgrade();
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.autoSpaceUpgrade = true; // Aktifkan auto space
            Debug.Log("Auto Space activated.");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}