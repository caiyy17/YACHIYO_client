import sys
import os
import json
import requests
import base64

def generate_image(prompt):
    response = requests.post('http://localhost:5050/image', json={'prompt': prompt})
    imageBase64 = response.json()["image"]
    text = response.json()["text"]
    image = base64.b64decode(imageBase64)
    with open("Assets/custom/Test/image.jpg", "wb") as f:
        f.write(image)
    return text

if __name__ == "__main__":
    answer = generate_image("一个猫咪在沙发上睡觉。")
    print(answer)