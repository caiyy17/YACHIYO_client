import requests

def call_flask_api(user_input):
    url = "http://localhost:8080/process_text"
    data = {"user_input": user_input}
    response = requests.post(url, json=data)
    if response.status_code == 200:
        result = response.json()
        bot_response = result['bot_response']
        image_name = result['image_name']
        print("Bot Response:", bot_response)
        print("Image Name:", image_name)
    else:
        print("Error:", response.text)

if __name__ == "__main__":
    user_input = input("Enter your question: ")
    call_flask_api(user_input)