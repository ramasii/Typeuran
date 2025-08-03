using UnityEngine;

public class MorePatienceUpgradeCard : UpgradeCardBehaviour
{
    public float patienceIncrease;

    public override void Upgrade()
    {
        base.Upgrade();
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.additionalPatience += patienceIncrease;
            Debug.Log($"Patience increased by {patienceIncrease}. New Patience: {mainScript.additionalPatience}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}