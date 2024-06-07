using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(ContentLoader))]
public class DataProcessor : MonoBehaviour
{
    AudioManager audioManager;
    ContentLoader contentLoader;
    void Start()
    {
        audioManager = GetComponent<AudioManager>();
        contentLoader = GetComponent<ContentLoader>();
    }

    public void ProcessData(string segment)
    {
        if (segment.Contains("[EoS]"))
        {
            // Debug.Log("finished");
        }
        else if (segment.Contains("[im]"))
        {
            var response = JsonUtility.FromJson<ImageData>(segment);
            contentLoader.LoadImage(response.image);
        }
        else if (segment.Contains("[timer]"))
        {
            Debug.Log("Timer started");
        }
        else if (segment.Contains("[audio]"))
        {
            var response = JsonUtility.FromJson<ResponseData>(segment);
            string textData = response.text;
            string emotionData = response.emotion;
            int indexData = response.index;
            string audio_base64 = response.audio;
            audioManager.QueueAudio(indexData, textData, emotionData, audio_base64);
        }
        else{
            Debug.Log("Unknown segment");
        }
    }

    private class ResponseData
    {
        public string text;
        public string emotion;
        public int index;
        public string audio;
    }

    private class ImageData
    {
        public string image;
    }


}