﻿using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour {
    [SerializeField] GameObject check;
    [SerializeField] Image gauge;
    [SerializeField] Image image;
    [SerializeField] TMPro.TextMeshProUGUI colorNumberText;
    [SerializeField] uint colorUint;

    public bool Check {
        get => check.activeSelf;
        set {
            foreach (Transform t in transform.parent) {
                var pb = t.GetComponent<PaletteButton>();
                if (pb != null) {
                    pb.check.SetActive(t == transform);
                }
            }
        }
    }

    public float ColoredRatio {
        set => gauge.fillAmount = value;
    }

    public void SetColor(uint colorUint) {
        this.colorUint = colorUint;
        image.color = BlackConvert.GetColor(colorUint);
        colorNumberText.color = image.color.grayscale < 0.5f ? Color.white : Color.black;
    }

    public Color PaletteColor => image.color;
    public uint ColorUint => colorUint;

    public int ColorIndex {
        get => int.Parse(colorNumberText.text);
        set => colorNumberText.text = value.ToString();
    }

    void Awake() {
        ColoredRatio = 0;
    }
}
