using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UnityEngine;

public class LoadImageBundle : MonoBehaviour
{
    void Start()
    {
        // Path to the asset bundle
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "imagebundle");

        // Load the asset bundle
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }

        // Load the image from the bundle
        Texture2D image = bundle.LoadAsset<Texture2D>("hazard"); // Replace "myImage" with the actual asset name
        if (image != null)
        {
            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f));

            // Create a GameObject to display the image
            GameObject imageObject = new GameObject("Loaded Image");
            SpriteRenderer renderer = imageObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;

            Debug.Log("Image loaded and displayed!");
        }
        else
        {
            Debug.LogError("Failed to load image from AssetBundle!");
        }

        // Unload the asset bundle
        bundle.Unload(false);
    }
}