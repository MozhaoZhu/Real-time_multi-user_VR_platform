import socket
import os
import azure.cognitiveservices.speech as speechsdk
from azure.cognitiveservices.speech.audio import AudioStreamFormat, AudioConfig

def recognize_from_stream(client_socket):
    speech_config = speechsdk.SpeechConfig(
        subscription=os.environ["SPEECH_KEY"],
        region=os.environ["SPEECH_REGION"]
    )
    speech_config.speech_recognition_language = "en-US"
    audio_format = AudioStreamFormat(48000, 16, 1)
    push_stream = speechsdk.audio.PushAudioInputStream(stream_format=audio_format)
    audio_config = AudioConfig(stream=push_stream)

    recognizer = speechsdk.SpeechRecognizer(speech_config, audio_config)

    recognizer.recognized.connect(lambda evt: print(">" + evt.result.text))
    recognizer.start_continuous_recognition()

    try:
        while True:
            data = client_socket.recv(4096)
            if not data:
                break
            push_stream.write(data)
    except KeyboardInterrupt:
        pass
    finally:
        recognizer.stop_continuous_recognition()
        client_socket.close()

def start_server():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind(("192.168.12.109", 7778))
    server_socket.listen(1)
    print("Server listening on port 7778...")
    client_socket, _ = server_socket.accept()
    print("Client connected!")
    recognize_from_stream(client_socket)

if __name__ == "__main__":
    start_server()
