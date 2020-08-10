using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class PerfectOverride
{
    public int referenceOrthographicSize;
    public float referencePixelsPerUnit;
}

[ExecuteInEditMode]
public class PerfectPixel : MonoBehaviour
{
    [Tooltip("The reference resolution to which your game is made for (e.g. 768px)")]
    public int referenceOrthographicSize;
    [Tooltip("Reference main PPU (e.g. 32, 64 etc...")]
    public float referencePixelsPerUnit;
    public List<PerfectOverride> overrides;
    private int currentHeight = 0;

    // Use this for initialization
    void Start()
    {
        UpdateOrthographicSize();
    }

    PerfectOverride FindOverride(int size)
    {
        return overrides.FirstOrDefault(x => x.referenceOrthographicSize == size);
    }

    void UpdateOrthographicSize()
    {
        this.currentHeight = Screen.height;

        // first find the reference orthoSize
        float refOrthoSize = (referenceOrthographicSize / referencePixelsPerUnit) * 0.5f;

        // then find the current orthoSize
        var overRide = FindOverride(currentHeight);
        float ppu = overRide != null ? overRide.referencePixelsPerUnit : referencePixelsPerUnit;
        float orthoSize = (currentHeight / ppu) * 0.5f;

        // the multiplier is to make sure the orthoSize is as close to the reference as possible
        float multiplier = Mathf.Max(1, Mathf.Round(orthoSize / refOrthoSize));

        // then we rescale the orthoSize by the multipler
        orthoSize /= multiplier;

        // set it
        this.GetComponent<Camera>().orthographicSize = orthoSize;
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        if (currentHeight != Screen.height)
        {
            UpdateOrthographicSize();
        }
        #endif
    }
}