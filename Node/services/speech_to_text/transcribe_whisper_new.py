import os
import sys
import argparse
import subprocess
import sounddevice as sd
import threading
import numpy as np
import time
import csv
from datetime import datetime

def recognize_from_mic(peer, device_id=None):
    whisper_bin = os.path.join(
        os.path.dirname(__file__),
        'whisper.cpp',
        'build',
        'bin',
        'stream'
    )

    model_path = os.path.join(
        os.path.dirname(__file__),
        'whisper.cpp',
        'models',
        'ggml-base.en.bin'
    )

    cmd = [
        whisper_bin,
        '-m', model_path,
        '--step', '2000',
        '--length', '5000',
        '-t', '8',
        '--keep', '400',
        '-kc',
        # '--stdin' 
    ]

    process = subprocess.Popen(
        cmd,
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        bufsize=0,
    )

    audio_start_time = time.time()  # Record when audio input starts

    csv_file = 'latency_log.csv'
    # Write CSV header if the file doesn't exist
    if not os.path.exists(csv_file):
        with open(csv_file, 'w', newline='', encoding='utf-8') as f:
            writer = csv.writer(f)
            writer.writerow(['timestamp', 'peer', 'latency_seconds', 'text'])

    # Function to handle Whisper output
    def handle_output():
        print("Whisper process started. Listening for audio input...")
        for line in process.stdout:
            line = line.decode('utf-8').strip()
            if line:
                current_time = time.time()
                latency = current_time - audio_start_time
                timestamp = datetime.now().isoformat(timespec='seconds')
                print(f"> {line} (Latency: {latency:.2f}s)")

                # Write to CSV
                with open(csv_file, 'a', newline='', encoding='utf-8') as f:
                    writer = csv.writer(f)
                    writer.writerow([timestamp, peer, f"{latency:.2f}", line])

    # Start output handling in background
    output_thread = threading.Thread(target=handle_output, daemon=True)
    output_thread.start()

    # Audio callback: send mic data into Whisper
    def audio_callback(indata, frames, time_info, status):
        if status:
            print(f"Status: {status}")

        if process.poll() is None:
            try:
                process.stdin.write(indata.tobytes())
                process.stdin.flush()
            except BrokenPipeError:
                print("Broken pipe - Whisper process may have terminated")
                return
            except Exception as e:
                print(f"Error sending audio data: {e}")
                return

    try:
        stream = sd.InputStream(
            samplerate=16000,
            channels=1,
            dtype='int16',
            callback=audio_callback,
            device=device_id,
            blocksize=32000,
        )
        
        print(f"Using audio device: {sd.query_devices(device_id)['name'] if device_id is not None else 'default'}")
        
        with stream:
            print("Stream started. Speak into the microphone...")
            try:
                while process.poll() is None:
                    sd.sleep(100)
            except KeyboardInterrupt:
                print("Interrupted. Stopping transcription...")
    except Exception as e:
        print(f"Error opening audio stream: {e}")
    finally:
        if process.poll() is None:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
        
        print(f"Whisper process for peer {peer} has stopped.")


def main():
    parser = argparse.ArgumentParser(description="Real-time audio transcription using Whisper")
    parser.add_argument("--peer", type=str, default="00000000-0000-0000-0000-000000000000")
    parser.add_argument("--device", type=int, default=None, help="Audio device ID for the microphone")
    parser.add_argument("--list-devices", action="store_true", help="List available audio devices")
    args = parser.parse_args()

    recognize_from_mic(args.peer, args.device)

if __name__ == "__main__":
    main()
