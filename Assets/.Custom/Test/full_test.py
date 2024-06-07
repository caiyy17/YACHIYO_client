import sys
import os
import json
import requests
import base64

addr = "http://localhost:5050"

def ask_tts(prompt, id = 0):
    response = requests.post(addr + '/llm_tts', json={'prompt': prompt, 'id': id}, stream=True)
    for line in response.iter_lines():
        if line:
            parsed_line = json.loads(line.decode('utf-8'))
            # 如果有audio字段，就播放音频
            if "text" in parsed_line:
                text = parsed_line["text"]
                audioBase64 = parsed_line["audio"]
                audio = base64.b64decode(audioBase64)
                index = parsed_line["index"]
                with open("Assets/custom/Test/audio_" + str(index) + ".wav", "wb") as f:
                    f.write(audio)
    return parsed_line

def asr_llm_tts(audio_file):
    response = requests.post(addr + '/asr_llm_tts', files={'file': audio_file}, stream=True)
    for line in response.iter_lines():
        if line:
            parsed_line = json.loads(line.decode('utf-8'))
            # 如果有audio字段，就播放音频
            if "text" in parsed_line:
                text = parsed_line["text"]
                audioBase64 = parsed_line["audio"]
                audio = base64.b64decode(audioBase64)
                index = parsed_line["index"]
                with open("Assets/.Custom/Test/audio_" + str(index) + ".wav", "wb") as f:
                    f.write(audio)
    return parsed_line

if __name__ == "__main__":
    # answer = ask_tts("请用一句话介绍东京", 0)
    path = "Assets/.Custom/Test/recording.wav"
    with open(path, 'rb') as audio_file:
        answer = asr_llm_tts(audio_file)
    print(answer)