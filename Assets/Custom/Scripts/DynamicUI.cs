using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicUI : MonoBehaviour
{
    public SpriteRenderer sr;
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update () {
        image.sprite = sr.sprite;
    }
}
