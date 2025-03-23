using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class Streaming : MonoBehaviour
{
    public string soundFileName = "Box.wav"; 
    public string textureFileName = "LoudBlock.png"; 

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(LoadTexture());
            StartCoroutine(LoadSound());
        }
        else
        {
            Debug.LogError("there is no streaming folder");
        }

        
        
    }

    IEnumerator LoadTexture()
    {
        // Construct the file path
        string texturePath = Path.Combine(Application.streamingAssetsPath, textureFileName);
        // chekin if file is there
        if (File.Exists(texturePath))
        {
            // Load the texture file
            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(texturePath);
            yield return textureRequest.SendWebRequest();
            if (textureRequest.result == UnityWebRequest.Result.Success)
            {
            // Apply the texture to the SpriteRenderer
            Texture2D texture = DownloadHandlerTexture.GetContent(textureRequest);
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
            Debug.LogError("Failed to load texture: " + textureRequest.error);
            }
        }
        

        
    }

    IEnumerator LoadSound()
    {
        // Construct the file path
        string soundPath = Path.Combine(Application.streamingAssetsPath, soundFileName);
        if (File.Exists(soundPath))
        {
            // Load the sound file
            UnityWebRequest soundRequest = UnityWebRequestMultimedia.GetAudioClip(soundPath, AudioType.WAV);
            yield return soundRequest.SendWebRequest();

            if (soundRequest.result == UnityWebRequest.Result.Success)
            {
            // Assign the audio clip to the AudioSource
            audioSource.clip = DownloadHandlerAudioClip.GetContent(soundRequest);
            }
            else
            {
            Debug.LogError("Failed to load sound: " + soundRequest.error);
            }
        }
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player is near the square
        if (other.CompareTag("Player"))
        {
            // Play the sound
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }
}
