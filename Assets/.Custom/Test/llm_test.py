import sys
import os
import json
import requests

def send_question(prompt, id = 0):
    response = requests.post('http://localhost:5050/llm', json={'prompt': prompt, 'id': id})
    answer = response.json()["answer"]
    return answer

def send_question_stream(prompt, id = 0):
    response = requests.post('http://localhost:5050/llm_stream', json={'prompt': prompt, 'id': id}, stream=True)
    answer = ""
    for line in response.iter_lines():
        if line:
            parsed_line = json.loads(line.decode('utf-8'))
            if "answer" in parsed_line:
                text = parsed_line["answer"]
                answer += text
                print(text, end="", flush=True)
        print()
    return answer

if __name__ == "__main__":
    answer = send_question_stream("讲一个故事。")
    print(answer)