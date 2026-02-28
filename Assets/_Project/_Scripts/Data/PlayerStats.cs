using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "CellGame/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Temel Deðerler (Baþlangýç)")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;
    public float baseDamage = 10f;
    public float baseFireRate = 0.5f;

    [Header("Anlýk Deðerler (Oyun Ýçi)")]
    public float currentMaxHealth;
    public float currentMoveSpeed;
    public float currentDamage;
    public float currentFireRate;

    // Oyun baþladýðýnda veya öldüðümüzde deðerleri sýfýrlamak için
    public void ResetValues()
    {
        currentMaxHealth = baseMaxHealth;
        currentMoveSpeed = baseMoveSpeed;
        currentDamage = baseDamage;
        currentFireRate = baseFireRate;
    }

    // Seviye atlayýnca bu fonksiyonu çaðýracaðýz
    public void UpgradeDamage(float amount)
    {
        currentDamage += amount;
    }
}