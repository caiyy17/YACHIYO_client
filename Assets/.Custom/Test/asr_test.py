import sys
import os
import json
import requests

def transcribe(audio_file):
    response = requests.post('http://localhost:5050/asr', files={'file': audio_file})
    answer = response.json()["answer"]
    return answer

if __name__ == "__main__":
    path = "Assets/.Custom/Test/recording.wav"
    with open(path, 'rb') as audio_file:
        answer = transcribe(audio_file)
    print(answer)