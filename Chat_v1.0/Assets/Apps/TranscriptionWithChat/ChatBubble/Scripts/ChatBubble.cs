using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Ubiq.Networking;
using Ubiq.Dictionaries;
using Ubiq.Messaging;
using Ubiq.Logging.Utf8Json;
using Ubiq.Rooms;
using System;
using System.Text;
using Ubiq.Avatars;
using Avatar = Ubiq.Avatars.Avatar;


namespace Ubiq.Samples
{
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private TMP_Text textMeshPro;
        public float typingSpeed = 0.05f;
        private string mypeerid = "";
        private NetworkId networkId = new NetworkId(99);
        private NetworkContext context;

        public Avatar avatar;

        private void Start()
        {
            context = NetworkScene.Register(this, networkId);
            backgroundSpriteRenderer = transform.Find("Canvas").GetComponent<SpriteRenderer>();
            textMeshPro = transform.Find("Canvas/Transcript").GetComponent<TMP_Text>();
            avatar = GetComponentInParent<Avatar>();

            Setup("Waiting for messages...");

            // // Register for network messages
            // NetworkScene.Register(this);
        }

        private void Update()
        {
            UpdateChatBubbleOrientation();
        }

        [Serializable]
        private struct Message
        {
            public string data;
            public string peer;
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage data)
        {
            if (avatar != null && avatar.Peer != null)
            {
                mypeerid = avatar.Peer.uuid;
            }
            else
            {
                Debug.LogError("Avatar or Peer not initialized.");
                return;
            }

            Message message = data.FromJson<Message>();
            if (avatar != null && avatar.Peer != null && message.peer == avatar.Peer.uuid)
            {
                // Update the chat text for the local peer
                textMeshPro.text = message.data;
                textMeshPro.ForceMeshUpdate();
                UpdateBackgroundSize();
            }
        }

        // Function to send the chat message to the network
        public void SendMessageToPeers(string messageText)
        {
            if (avatar != null && avatar.Peer != null)
            {
                mypeerid = avatar.Peer.uuid;
                Message message = new Message
                {
                    data = messageText,
                    peer = avatar.Peer.uuid
                };

                // Send the message to all peers
                string messageJson = JsonUtility.ToJson(message);
                context.Send(JsonUtility.ToJson(message));
            }
        }

        private void Setup(string text)
        {
            textMeshPro.SetText(text);
            textMeshPro.ForceMeshUpdate();
            UpdateBackgroundSize();
        }

        private void UpdateBackgroundSize()
        {
            Bounds textBounds = textMeshPro.textBounds;
            Vector2 padding = new Vector2(0.6f, 1.7f);
            float backgroundWidth = (float)textBounds.size.x * 0.067f + padding.x;
            float backgroundHeight = (float)textBounds.size.y * 0.05f + padding.y;
            backgroundSpriteRenderer.size = new Vector2(backgroundWidth, backgroundHeight);
        }

        private void UpdateChatBubbleOrientation()
        {
            var cameraTransform = Camera.main.transform;
            var headTransform = transform.parent;
            var headToCamera = (cameraTransform.position - headTransform.position).normalized;
            transform.forward = Vector3.ProjectOnPlane(headToCamera, Vector3.up).normalized;
        }
    }
}
