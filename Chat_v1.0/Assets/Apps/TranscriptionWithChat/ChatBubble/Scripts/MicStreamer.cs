// using System;
// using System.IO;
// using System.Net.Sockets;
// using System.Threading;
// using UnityEngine;

// public class MicStreamer : MonoBehaviour
// {
//     private TcpClient client;
//     private NetworkStream stream;
//     private string micDevice;
//     private AudioClip micClip;
//     private const int sampleRate = 48000;
//     private const int chunkSize = 1024;
//     private int lastSample = 0;

//     void Start()
//     {
//         micDevice = Microphone.devices[0];
//         Debug.Log("Using microphone: " + micDevice);
//         micClip = Microphone.Start(micDevice, true, 1, sampleRate);

//         Thread clientThread = new Thread(ConnectToServer);
//         clientThread.IsBackground = true;
//         clientThread.Start();
//     }

//     void ConnectToServer()
//     {
//         try
//         {
//             client = new TcpClient("192.168.12.109", 7778); // Localhost, match with Python server
//             stream = client.GetStream();
//             Debug.Log("Connected to server");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError("Failed to connect: " + e.Message);
//         }
//     }

//     void Update()
//     {
//         if (stream == null || !stream.CanWrite) return;

//         int currentPosition = Microphone.GetPosition(micDevice);
//         int samplesToSend = (currentPosition - lastSample + micClip.samples) % micClip.samples;

//         float[] samples = new float[samplesToSend * micClip.channels];
//         micClip.GetData(samples, lastSample);

//         byte[] byteData = new byte[samples.Length * 2]; // 16-bit PCM
//         for (int i = 0; i < samples.Length; i++)
//         {
//             short val = (short)(samples[i] * short.MaxValue);
//             byte[] bytes = BitConverter.GetBytes(val);
//             byteData[i * 2] = bytes[0];
//             byteData[i * 2 + 1] = bytes[1];
//         }

//         stream.Write(byteData, 0, byteData.Length);
//         stream.Flush();
//         lastSample = currentPosition;
//     }

//     private void OnApplicationQuit()
//     {
//         if (stream != null) stream.Close();
//         if (client != null) client.Close();
//         Microphone.End(micDevice);
//     }
// }
