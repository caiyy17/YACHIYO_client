import sys
import os
import json
import requests
import base64

def tts(prompt, language = "zh"):
    response = requests.post('http://localhost:5050/tts', json={'prompt': prompt, 'language': language})
    audioBase64 = response.json()["audio"]
    text = response.json()["text"]
    audio = base64.b64decode(audioBase64)
    with open("Assets/custom/Test/audio.wav", "wb") as f:
        f.write(audio)
    return text

if __name__ == "__main__":
    answer = tts("穿过县界长长的大道，我看到了一片广阔的田野。")
    print(answer)