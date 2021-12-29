﻿using System;
using System.Collections.Generic;
using System.Linq;
using ConditionalDebug;
using UnityEngine;

public class PaletteButtonGroup : MonoBehaviour
{
    static bool Verbose => false;
    
    [SerializeField]
    List<PaletteButton> paletteButtonList;

    [SerializeField]
    PaletteButton paletteButtonPrefab;

    [SerializeField]
    GameObject poofPrefab;

    readonly Dictionary<uint, int> paletteIndexbyColor = new Dictionary<uint, int>();

    StageData stageData;

    void Awake()
    {
        DestroyAllPaletteButtons();
    }

    public Color CurrentPaletteColor
    {
        get
        {
            foreach (Transform t in transform)
            {
                var pb = t.GetComponent<PaletteButton>();
                if (pb.Check) return pb.PaletteColor;
            }

            return Color.white;
        }
    }

    public uint CurrentPaletteColorUint
    {
        get
        {
            foreach (Transform t in transform)
            {
                var pb = t.GetComponent<PaletteButton>();
                if (pb.Check)
                {
                    if (Verbose) ConDebug.Log($"CurrentPaletteColorUint: {pb.ColorUint} (0x{pb.ColorUint:X8})");
                    return pb.ColorUint;
                }
            }

            return 0xffffffff;
        }
    }

    public void CreatePalette(StageData stageData)
    {
        DestroyAllPaletteButtons();

        var colorUintArray = stageData.islandDataByMinPoint.Select(e => e.Value.rgba).Distinct().OrderBy(e => e)
            .ToArray();
        var paletteIndex = 0;
        paletteButtonList.Clear();
        foreach (var colorUint in colorUintArray)
        {
            if ((colorUint & 0x00ffffff) == 0x00ffffff)
                Debug.LogError("CRITICAL ERROR: Palette color cannot be WHITE!!!");
            var paletteButton = Instantiate(paletteButtonPrefab, transform).GetComponent<PaletteButton>();
            paletteButton.SetColor(colorUint);
            paletteIndexbyColor[colorUint] = paletteIndex;
            paletteButton.ColorIndex = paletteIndex + 1;
            paletteIndex++;
            paletteButtonList.Add(paletteButton);
        }

        this.stageData = stageData;
    }

    void DestroyAllPaletteButtons()
    {
        foreach (var t in transform.Cast<Transform>().ToArray()) Destroy(t.gameObject);
    }

    public int GetPaletteIndexByColor(uint color)
    {
        return paletteIndexbyColor[color];
    }

    public void UpdateColoredCount(uint color, int count)
    {
        var paletteButton = paletteButtonList[GetPaletteIndexByColor(color)];
        
        var oldRatio = paletteButton.ColoredRatio;
        var newRatio = (float) count / stageData.islandCountByColor[color];
        
        paletteButton.ColoredRatio = newRatio;

        // 다 칠한 팔레트는 사라진다.
        paletteButton.gameObject.SetActive(newRatio < 1.0f);

        if (oldRatio >= 1.0f || newRatio < 1.0f) return;

        if (poofPrefab == null) return;
        
        // 이번에 칠해서 사라졌다. 펑 효과 보여주자.
        var poof = Instantiate(poofPrefab, GetComponentInParent<Canvas>().transform).GetComponent<Poof>();
        var poofTransform = poof.transform;
        poofTransform.position = paletteButton.transform.position;
        poofTransform.localScale = Vector3.one;
    }
}