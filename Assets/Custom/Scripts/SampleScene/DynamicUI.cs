using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DynamicUI : MonoBehaviour
{
    public SpriteRenderer sr;
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        image.sprite = sr.sprite;
    }
}
