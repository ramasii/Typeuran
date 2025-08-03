using UnityEngine;

public class ReverseModeUpgradeCard : UpgradeCardBehaviour
{
    public override void Upgrade()
    {
        base.Upgrade();
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.reverseModeUpgrade = true; // Aktifkan reverse mode
            Debug.Log("Reverse Mode activated.");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}