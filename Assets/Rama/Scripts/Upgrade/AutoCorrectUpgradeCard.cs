using UnityEngine;

public class AutoCorrectUpgradeCard : UpgradeCardBehaviour
{
    public int autoCorrectIncrease;

    public override void Upgrade()
    {
        base.Upgrade(); // Panggil method Upgrade dari base class untuk mengurangi total coin
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.autoCorrectLevel += autoCorrectIncrease;
            if (mainScript.autoCorrectLevel > mainScript.maxAutoCorrectLevel)
            {
                mainScript.autoCorrectLevel = mainScript.maxAutoCorrectLevel; // Batasi level auto correct
            }
            mainScript.autoCorrectAvailable = mainScript.autoCorrectLevel; // Set jumlah auto correct yang tersedia sesuai level

            Debug.Log($"Auto Correct Level increased by {autoCorrectIncrease}. New Auto Correct Level: {mainScript.autoCorrectLevel}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}