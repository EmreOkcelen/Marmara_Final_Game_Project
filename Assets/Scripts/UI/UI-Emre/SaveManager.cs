using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public int level;
    public float health;
    // Diðer veriler
}

public class SaveManager : MonoBehaviour
{
    public static void SaveGame()
    {
        SaveData data = new SaveData();
        data.playerPosition = GameObject.FindWithTag("Player").transform.position;
        data.level = 3;
        data.health = 75;

        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public static void LoadGame()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            GameObject.FindWithTag("Player").transform.position = data.playerPosition;
            // Diðer verileri uygula
        }
    }
}
