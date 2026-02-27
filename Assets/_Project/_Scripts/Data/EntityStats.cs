using UnityEngine;

[CreateAssetMenu(fileName = "NewStats", menuName = "CellGame/Stats")]
public class EntityStats : ScriptableObject
{
    public float maxHealth;
    public float moveSpeed;
    public float damage;
}