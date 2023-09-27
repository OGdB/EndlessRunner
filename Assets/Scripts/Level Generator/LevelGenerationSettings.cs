using UnityEngine;
using System.IO;

[System.Serializable]
public class LevelGenerationSettings
{
    private static readonly string SAVEPATH = Path.Combine(Application.persistentDataPath, "LevelGenerationSettings.json");

    public int numberOfVisibleObstacleRows = 10;
    public float distanceToFirstRow = 15;
    public float distanceBetweenRows = 7.5f;
    public float greyObstacleChance = 0.5f;
    public float blueObstacleChance = 0.25f;
    public int startNumberOfLanes = 3;
    public int seed;

    // Constructor
    public LevelGenerationSettings(int numberOfVisibleObstacleRows = default, float distanceToFirstRow = default, float distanceBetweenRows = default, float greyObstacleChance = default, float blueObstacleChance = default, int startNumberOfLanes = default, int seed = default)
    {
        this.numberOfVisibleObstacleRows = numberOfVisibleObstacleRows;
        this.distanceToFirstRow = distanceToFirstRow;
        this.distanceBetweenRows = distanceBetweenRows;
        this.greyObstacleChance = greyObstacleChance;
        this.blueObstacleChance = blueObstacleChance;
        this.seed = seed;
        this.startNumberOfLanes = startNumberOfLanes;
    }

    public void ExportToJson()
    {
        string json = JsonUtility.ToJson(this);

        // Create the directory if it doesn't exist
        string directoryPath = Path.GetDirectoryName(SAVEPATH);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        Debug.Log($"Save with seed: {this.seed}");
        // Write the JSON data to the file
        File.WriteAllText(SAVEPATH, json);
    }

    public static LevelGenerationSettings ImportFromJson()
    {
        if (File.Exists(SAVEPATH))
        {
            string json = File.ReadAllText(SAVEPATH);
            LevelGenerationSettings settings = JsonUtility.FromJson<LevelGenerationSettings>(json);

            bool validSettings = ValidSettings(ref settings);
            if (validSettings)
            {
                return settings;
            }
            else
            {
                Debug.LogWarning("Error: Invalid settings.");
                return settings;
            }
        }
        else
        {
            Debug.LogWarning("Error: JSON file does not exist.");
            return null;
        }
    }

    private static bool ValidSettings(ref LevelGenerationSettings settings)
    {
        bool valid = true;

        if (settings.numberOfVisibleObstacleRows < 1)
        {
            Debug.LogWarning("Invalid numberOfVisibleObstacleRows: set to default.");
            settings.numberOfVisibleObstacleRows = 10; // Set a reasonable default value
            valid = false;
        }

        if (settings.distanceToFirstRow < 1)
        {
            Debug.LogWarning("Invalid distanceToFirstRow: set to default.");
            settings.distanceToFirstRow = 15; // Set a reasonable default value
            valid = false;
        }

        if (settings.distanceBetweenRows < 1)
        {
            Debug.LogWarning("Invalid distanceBetweenRows: set to default.");
            settings.distanceBetweenRows = 7.5f; // Set a reasonable default value
            valid = false;
        }

        if (settings.greyObstacleChance < 0 || settings.greyObstacleChance > 1f)
        {
            Debug.LogWarning("Invalid greyObstacleChance: set to default.");
            settings.greyObstacleChance = 0.5f; // Set a reasonable default value
            valid = false;
        }

        if (settings.blueObstacleChance < 0 || settings.blueObstacleChance > 1f)
        {
            Debug.LogWarning("Invalid blueObstacleChance: set to default.");
            settings.blueObstacleChance = 0.25f; // Set a reasonable default value
            valid = false;
        }

        if (settings.startNumberOfLanes < 1)
        {
            Debug.LogWarning("Invalid startNumberOfLanes: set to default.");
            settings.startNumberOfLanes = 3; // Set a reasonable default value
            valid = false;
        }

        if (settings.seed < 0)
        {
            Debug.LogWarning("Invalid Seed: set to default.");
            settings.seed = Random.Range(0, int.MaxValue); // Set a reasonable default value
            valid = false;
        }

        return valid;
    }
}
