using System.Collections.Generic;
using UnityEngine;

public class KeywordDetector : MonoBehaviour
{
    public List<string> keywords = new List<string>();
    public float sensitivity = 0.7f;

    public bool isOn = false;
    public bool isDetected = false;

    public void SetKWS(bool isON)
    {
        isOn = isON;
        if (isOn)
        {
            Debug.Log("KWS is ON");
        }
        else
        {
            Debug.Log("KWS is OFF");
        }
    }

    public bool IsDetected()
    {
        return isDetected && isOn;
    }
}
