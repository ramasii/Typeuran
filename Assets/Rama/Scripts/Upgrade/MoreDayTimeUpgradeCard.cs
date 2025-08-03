using UnityEngine;

public class MoreDayTimeUpgradeCard : UpgradeCardBehaviour
{
    public float dayTimeIncrease;

    public override void Upgrade()
    {
        base.Upgrade();
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.additionalDayTime += dayTimeIncrease;
            Debug.Log($"Day Time increased by {dayTimeIncrease}. New Day Time: {mainScript.additionalDayTime}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}