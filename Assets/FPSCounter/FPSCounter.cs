using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI fpsCounterText;
    [Tooltip("The number of frame samples we want to take from, by default set to 50")]
    [SerializeField] protected int frameSamples = 50;
    [Tooltip("Do we want it to be uneffected when we pause the game using time scale")]
    [SerializeField] protected bool useUnscaledDeltaTime = true;
    protected int lastFrameIndex;
    protected float[] frameDeltaTimeArray;
    protected int currentFPSCount;

    // Start is called before the first frame update
    void Awake()
    {
        frameDeltaTimeArray = new float[frameSamples];
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFrameCount();
        DisplayFrameCount();
    }

    /// <summary>
    /// takes the current delta time between frames and store it.
    /// </summary>
    protected virtual void UpdateFrameCount()
    {
        frameDeltaTimeArray[lastFrameIndex] = useUnscaledDeltaTime? Time.unscaledDeltaTime : Time.deltaTime;
        lastFrameIndex = (lastFrameIndex+1) % frameDeltaTimeArray.Length;
        currentFPSCount = Mathf.RoundToInt(ReturnCalculatedFPS());   
    }

    /// <summary>
    /// Displays our frame count on the UI if it exists
    /// </summary>
    protected virtual void DisplayFrameCount()
    {
        if (fpsCounterText != null)
        {
            fpsCounterText.text = currentFPSCount.ToString() + "fps";
        }
    }

    /// <summary>
    /// Calculate our Frames Per Second using our stored data of the frame deltas.
    /// </summary>
    /// <returns></returns>
    protected virtual float ReturnCalculatedFPS()
    {
        float total = 0f;
        for (int i = 0; i < frameDeltaTimeArray.Length; i++)
        {
            total += frameDeltaTimeArray[i];
        }
        return frameDeltaTimeArray.Length / total;
    }
}
