from flask import Flask, request, jsonify, Response
import json

app = Flask(__name__)

from transformers import AutoTokenizer, AutoModel
tokenizer = AutoTokenizer.from_pretrained("THUDM/chatglm3-6b", trust_remote_code=True)
model = AutoModel.from_pretrained("THUDM/chatglm3-6b", trust_remote_code=True).quantize(4).cuda()
model = model.eval()

@app.route('/chatglm', methods=['POST'])
def chatglm_custom():
    data = request.json
    prompt = data['prompt']
    history = data['history']
    try:
        print(prompt, history)
        # response, history = model.chat(tokenizer, prompt, history=history)
        # print(response)
        current_length = 0
        stop_stream = False
        for response, history in model.stream_chat(tokenizer, prompt, history=history):
            if stop_stream:
                stop_stream = False
                break
            else:
                print(response[current_length:], end="", flush=True)
                current_length = len(response)
        print("")
        return jsonify({'response': response, 'history': history})
    except Exception as e:
        print(e)
        return jsonify({'error': "error"})

@app.route('/chatglm_stream', methods=['POST'])
def chatglm_custom_stream():
    data = request.json
    prompt = data['prompt']
    history = data['history']

    def generate(prompt, history):
        try:
            print(prompt, history)
            current_length = 0
            stop_stream = False
            for response, history in model.stream_chat(tokenizer, prompt, history=history):
                if stop_stream:
                    stop_stream = False
                    break
                else:
                    print(response[current_length:], end="", flush=True)
                    yield json.dumps({'response': response[current_length:], 'history': history}) + '\n'
                    current_length = len(response)
            print("")
        except Exception as e:
            print(e)
            yield json.dumps({'error': "error"}) + '\n'

    return Response(generate(prompt, history), content_type='application/json')

if __name__ == '__main__':
    app.run(debug=True, port=5002)