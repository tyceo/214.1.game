using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveLoad : MonoBehaviour
{
    private string saveFilePath;

    void Start()
    {
        // Set the save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "gameData.json");

        // Ensure this object persists across scenes
        DontDestroyOnLoad(gameObject);

        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            File.Delete(saveFilePath);
            SceneManager.LoadScene("Zone1");

        }


    }
    void OnDestroy()
    {
        // Unsubscribe from the sceneLoaded event when this object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called whenever a new scene is loaded
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        // Check if the current scene is "Scene1"
        if (SceneManager.GetActiveScene().name == "Zone1")
        {
            // Load the game data from the JSON file
            GameData gameData = LoadGameData();

            // If the JSON file contains a different scene, load that scene
            if (gameData != null && gameData.currentScene != "Zone1")
            {
                Debug.Log("Loading scene from JSON: " + gameData.currentScene);
                SceneManager.LoadScene(gameData.currentScene);
            }

            // Update the current scene in the save file
            UpdateCurrentScene(scene.name);
        }

    }

    // Update the current scene in the save file
    private void UpdateCurrentScene(string sceneName)
    {
        // Load existing data from the file
        GameData existingData = LoadGameData();

        // Update the current scene
        existingData.currentScene = sceneName;

        // Save the updated data back to the file
        SaveGameData(existingData.health, existingData.currentScene, existingData.inventoryItems);
    }

    // Save game data to JSON file
    public void SaveGameData(int health, string currentScene, List<string> inventoryItems)
    {
        // Create a GameData object
        GameData data = new GameData
        {
            health = health,
            currentScene = currentScene,
            inventoryItems = inventoryItems
        };

        // Convert to JSON
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        // Save to file
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game data saved to: " + saveFilePath);
    }

    // Load game data from JSON file
    public GameData LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            // Load JSON from file
            string json = File.ReadAllText(saveFilePath);

            // Convert JSON to GameData
            GameData data = JsonUtility.FromJson<GameData>(json);

            Debug.Log("Game data loaded: " + json);
            return data;
        }
        else
        {
            Debug.LogWarning("Save file not found. Returning default game data.");
            return new GameData
            {
                health = 5,
                currentScene = "Zone1",
                inventoryItems = new List<string>()
            };
        }
    }
}

// Serializable class to hold game data
[System.Serializable]
public class GameData
{
    public int health; // Player's health
    public string currentScene; // Current scene name
    public List<string> inventoryItems; // List of inventory items
}