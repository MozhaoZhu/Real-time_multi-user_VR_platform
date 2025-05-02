// using System;
// using UnityEngine;
// using Ubiq.Messaging;

// public class PortReceiver : MonoBehaviour
// {
//     public MicStreamer micStreamer;

//     private NetworkContext context;

//     private struct PortMessage
//     {
//         public int port;
//     }

//     void Start()
//     {
//         context = NetworkScene.Register(this);
//     }

//     public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
//     {
//         var data = message.FromJson<PortMessage>();
//         Debug.Log($"[PortReceiver] Received port {data.port} for audio streaming");

//         if (micStreamer != null)
//         {
//             micStreamer.ConnectToServer(data.port);
//         }
//     }
// }
