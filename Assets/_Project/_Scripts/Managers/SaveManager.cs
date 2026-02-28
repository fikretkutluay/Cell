using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] PlayerStats playerStats;

    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "cell_save.json");
    }

    private void Start()
    {
        LoadGame();
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void SaveGame()
    {
        string json = JsonUtility.ToJson(playerStats);
        File.WriteAllText(savePath, json);
    }

    private void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            JsonUtility.FromJsonOverwrite(json, playerStats);
        }
        else
        {
            playerStats.ResetValues();
        }
    }

    [ContextMenu("Delete Save Files")]
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}
