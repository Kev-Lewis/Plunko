using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private const string GameDataFileName = "data.qnd";
    private const string RunSaveFileName = "run_save.json";

    public static void Save(GameData data) {
        if (data == null) {
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream fileStream = new FileStream(GetGameDataPath(), FileMode.Create)) {
            formatter.Serialize(fileStream, data);
        }
    }

    public static GameData Load() {
        string path = GetGameDataPath();

        if (!File.Exists(path)) {
            GameData emptyData = new GameData();
            Save(emptyData);
            return emptyData;
        }

        try {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(path, FileMode.Open)) {
                GameData data = formatter.Deserialize(fileStream) as GameData;

                if (data != null) {
                    return data;
                }
            }
        }
        catch {
            Debug.LogWarning("GameData save file could not be loaded. Creating a new save file.");
        }

        GameData fallbackData = new GameData();
        Save(fallbackData);
        return fallbackData;
    }

    public static void SaveRun(RunSaveData data) {
        if (data == null) {
            return;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetRunSavePath(), json);
    }

    public static RunSaveData LoadRun() {
        string path = GetRunSavePath();

        if (!File.Exists(path)) {
            return null;
        }

        try {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<RunSaveData>(json);
        }
        catch {
            Debug.LogWarning("Run save file could not be loaded.");
            return null;
        }
    }

    public static bool HasRunSave() {
        return File.Exists(GetRunSavePath());
    }

    public static void DeleteRunSave() {
        string path = GetRunSavePath();

        if (File.Exists(path)) {
            File.Delete(path);
        }
    }

    public static string GetGameDataPath() {
        return Path.Combine(Application.persistentDataPath, GameDataFileName);
    }

    public static string GetRunSavePath() {
        return Path.Combine(Application.persistentDataPath, RunSaveFileName);
    }
}