import sys
import subprocess
import argparse
import os
from collections import defaultdict

# A dictionary to store the content spoken by each peer, grouped by their UUID.
peer_transcripts = defaultdict(str)

def store_transcript(peer_uuid, content):
    """Store or append the transcript for a specific peer UUID, ensuring no repeated phrases."""
    current_transcript = peer_transcripts[peer_uuid]
    
    # Check if the new content starts with the last word of the current transcript (indicating continuation)
    if current_transcript.strip() and content.strip() and content.strip() != current_transcript.strip().split()[-1]:
        peer_transcripts[peer_uuid] += content + "\n"

def run_whisper(peer_uuid):
    SCRIPT_DIR = os.path.dirname(os.path.realpath(__file__))

    whisper_binary = os.path.join(SCRIPT_DIR, "whisper.cpp/build/bin/stream")
    model_path = os.path.join(SCRIPT_DIR, "whisper.cpp/models/ggml-base.en.bin")
    
    # Construct Whisper command
    whisper_command = [
        whisper_binary,
        "-m", model_path,
        "--step", "500",
        "--length", "5000",
        "-t", "8"
    ]

    print(f"Starting Whisper for peer: {peer_uuid}", file=sys.stderr)

    # Start the Whisper subprocess
    process = subprocess.Popen(
        whisper_command,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        bufsize=1,
        universal_newlines=True
    )

    last_line = ""
    for line in process.stdout:
        if line.strip():
            last_line = f"> {line.strip()}"
            # Store this line for the peer, ensuring no repetition
            store_transcript(peer_uuid, last_line)
            # Print and overwrite previous line
            print(f"\r{last_line}\n", end="", flush=True)

    # Print a newline after Whisper finishes
    print()

    process.stdout.close()
    process.wait()

def save_transcripts():
    """Save the collected transcripts for all peers to a file."""
    with open("transcripts.txt", "w") as file:
        for peer_uuid, transcript in peer_transcripts.items():
            file.write(f"--- Transcript for Peer {peer_uuid} ---\n")
            file.write(transcript + "\n")

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--peer", type=str, default="00000000-0000-0000-0000-000000000000")
    args = parser.parse_args()

    run_whisper(args.peer)

    # After processing, save the transcripts to a file
    save_transcripts()

if __name__ == "__main__":
    main()
