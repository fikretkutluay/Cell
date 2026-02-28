using UnityEngine;

[System.Serializable]
public class LootDrop
{
    public GameObject itemPrefab;
    [Range(0, 100)] public float dropChance;
}

[CreateAssetMenu(fileName = "NewLootTable", menuName = "CellGame/LootTable")]
public class LootTable : ScriptableObject
{
    public LootDrop[] possibleLoots;

    public GameObject GetRandomLoot()
    {
        foreach (var loot in possibleLoots)
        {
            float roll = Random.Range(0.0f, 100.0f);

            if (roll <= loot.dropChance)
            {
                return loot.itemPrefab;
            }
        }
        return null;
    }
}
