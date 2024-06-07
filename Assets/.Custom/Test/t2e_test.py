import sys
import os
import json
import requests

def t2e(prompt, language = "en"):
    response = requests.post('http://localhost:5050/t2e', json={'prompt': prompt, 'language': language})
    return response.json()['emotion']

if __name__ == "__main__":
    answer = t2e("All it takes to embark on the greatest adventure of our lives is the courage to take the first step.")
    print(answer)
