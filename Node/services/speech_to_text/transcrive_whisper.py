import whisper

model = whisper.load_model("base")
result = model.transcribe("/Users/zhumozhao/Desktop/audio.m4a")
print(result["text"])