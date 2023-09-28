using UnityEngine;
using System.IO;

[System.Serializable]
public class LevelGenerationSettings
{
    private static readonly string SAVEPATH = Path.Combine(Application.persistentDataPath, "LevelGenerationSettings.json");

    public int numberOfVisibleObstacleRows = 10;
    public float distanceToFirstRow = 15;
    public float distanceBetweenRows = 7.5f;
    public int startNumberOfLanes = 3;
    public int seed;

    public LevelGenerationSettings(int numberOfVisibleObstacleRows = 10,
                                   float distanceToFirstRow = 15,
                                   float distanceBetweenRows = 7.5f,
                                   int startNumberOfLanes = 3,
                                   int seed = default)
    {
        this.numberOfVisibleObstacleRows = numberOfVisibleObstacleRows;
        this.distanceToFirstRow = distanceToFirstRow;
        this.distanceBetweenRows = distanceBetweenRows;
        this.startNumberOfLanes = startNumberOfLanes;
        if (seed == default) seed = Random.Range(0, int.MaxValue);
        this.seed = seed;
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
            string directoryPath = Path.GetDirectoryName(SAVEPATH);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            LevelGenerationSettings settings = new();
            string json = JsonUtility.ToJson(settings);
            File.WriteAllText(SAVEPATH, json);

            Debug.LogWarning("No JSON file at filepath, generating new.");
            return settings;
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
