using UnityEngine;
public class HealUpgradeCard : UpgradeCardBehaviour
{
    public int healAmount;

    public override void Upgrade()
    {
        base.Upgrade();
        MainScript mainScript = FindObjectOfType<MainScript>();
        if (mainScript != null)
        {
            mainScript.hp += healAmount;
            if (mainScript.hp > mainScript.maxHP)
            {
                mainScript.hp = mainScript.maxHP;
            }
            Debug.Log($"Healed by {healAmount}. New HP: {mainScript.hp}");
        }
        else
        {
            Debug.LogWarning("MainScript not found!");
        }
    }
}