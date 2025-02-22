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

namespace Ubiq.Samples{

    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private TMP_Text textMeshPro;
        public float typingSpeed = 1.0f;
        private string logFilePath;
        public int maxWordLimit = 30;
        private string mypeerid = "";


        private NetworkId networkId = new NetworkId(99);
        private NetworkContext context;

        public Avatar avatar;


        private void Start()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            logFilePath = Path.Combine(desktopPath, "ChatBubbleLogs.txt");
            // Debug.Log("" + logFilePath);
            File.WriteAllText(logFilePath, "ChatBubble Log Initialized\n");

            // Initialize the chat bubble with a default message
            Setup("Waiting for messages...");
            context = NetworkScene.Register(this, networkId);
            backgroundSpriteRenderer = transform.Find("Canvas").GetComponent<SpriteRenderer>();
            textMeshPro = transform.Find("Canvas/Transcript").GetComponent<TMP_Text>();
            avatar = GetComponentInParent<Avatar>();
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
            // Avatar check 
            if (avatar != null && avatar.Peer != null)
            {
                mypeerid = avatar.Peer.uuid;
                Debug.Log("Avatar id: " + mypeerid);
            }
            else
            {
                Debug.LogError("Avatar or Peer not initialized.");
            }

            // Message Handling
            Message message = data.FromJson<Message>();
            Debug.Log("message id: " + message.peer);
            if (message.peer == mypeerid)
            {
                // Debug.Log("Message received from my peer: " + message.data);
                Log("Message received: " + message.data);

                // Start the typing effect and display the message
                StartCoroutine(TypeText(message.data));
            }
        }

        private void Setup(string text)
        {
            textMeshPro.SetText(text); 
            textMeshPro.ForceMeshUpdate();
            UpdateBackgroundSize();
        }

        private IEnumerator TypeText(string message)
        {
            textMeshPro.text = "";  // Clear the existing text
            string[] words = message.Split(' '); // Split the message into words
            List<string> displayedWords = new List<string>(); // List to store displayed words

            // Loop through each word in the message
            foreach (string word in words)
            {
                // Add the current word to the displayed words list
                displayedWords.Add(word);

                // Update the text with the new word added
                textMeshPro.text = string.Join(" ", displayedWords);
                textMeshPro.ForceMeshUpdate();

                // Adjust the background size dynamically
                UpdateBackgroundSize();

                // Wait for the typing effect for adding the word
                yield return new WaitForSeconds(typingSpeed);

                // Check if the total word count exceeds the word limit
                while (displayedWords.Count > maxWordLimit)
                {
                    // Remove the first word with a delay for effect
                    displayedWords.RemoveAt(0);

                    // Update the text with the word removed
                    textMeshPro.text = string.Join(" ", displayedWords);
                    textMeshPro.ForceMeshUpdate();

                    // Adjust the background size dynamically
                    UpdateBackgroundSize();

                    // Wait for the typing effect for removing the word
                    yield return new WaitForSeconds(typingSpeed);
                }
            }
        }




        private void UpdateBackgroundSize()
        {
            // Get the bounds of the text
            Bounds textBounds = textMeshPro.textBounds;

            Vector2 padding = new Vector2(0.6f, 1.7f); // Add desired padding
            // // Vector2 padding = new Vector2(2.0f, 0.f); // Add desired padding
            // Log("text bounds: " + textBounds);
            // Log("text bounds center: " + textBounds.center);
            // Log("text bounds size: " + textBounds.size);

            // Get the width and height of the text
            float backgroundWidth = (float)textBounds.size.x * 0.067f + padding.x;
            float backgroundHeight = (float)textBounds.size.y * 0.05f + padding.y;

            // Set the background size
            backgroundSpriteRenderer.size = new Vector2(backgroundWidth, backgroundHeight);
            // Log("backgroundSpriteRenderer size: " + backgroundSpriteRenderer.size);

            // Align the background position to the top-left corner of the text
            Vector3 localCenter = new Vector3(textBounds.center.x, textBounds.center.y, 0f);
            Vector3 localTopLeftCorner = new Vector3(textBounds.min.x, textBounds.max.y, 0f);
            Vector3 localBottomRightCorner = new Vector3(textBounds.max.x, textBounds.min.y, 0f);
            // Log("localCenter: "+ localCenter);
            // Log("localTopLeftCorner: "+ localTopLeftCorner);
            // Log("localBottomRightCorner: "+ localBottomRightCorner);

            Vector3 worldCenter = textMeshPro.transform.TransformPoint(localCenter);
            Vector3 worldTopLeftCorner = textMeshPro.transform.TransformPoint(localTopLeftCorner);
            Vector3 worldBottomRightCorner = textMeshPro.transform.TransformPoint(localBottomRightCorner);
            // Log("worldCenter: " + worldCenter);
            // Log("worldTopLeftCorner: " + worldTopLeftCorner);
            // Log("worldBottomRightCorner: " + worldBottomRightCorner);

            float diffX = (worldBottomRightCorner.x - worldTopLeftCorner.x);
            float diffY = (worldTopLeftCorner.y - worldBottomRightCorner.y) / 2.0f;


            // Bounds spriteBounds = backgroundSpriteRenderer.bounds;
            // Vector3 spriteSize = spriteBounds.size; // World-space width and height

            // // Calculate offset (e.g., move pivot point to the top-left corner)
            // Vector3 topLeftOffset = new Vector3(-spriteSize.x / 2, spriteSize.y / 2, 0);

            // // Apply the offset
            // backgroundSpriteRenderer.transform.position += topLeftOffset;

            // Adjust the background's position to grow rightward and downward
            // backgroundSpriteRenderer.transform.position = new Vector3(
            //     worldCenter.x, // Align X with the parent
            //     worldCenter.y - diffY, // Adjust downward
            //     worldCenter.z // Keep Z consistent
            // );

            // // Log("Background Position: " + backgroundSpriteRenderer.transform.position);
            // textMeshPro.transform.position = new Vector3(worldCenter.x, worldCenter.y, textMeshPro.transform.position.z);

            // float textWidth = textMeshPro.rectTransform.rect.width;
            // float textHeight = textMeshPro.rectTransform.rect.height;


            // // Apply new size to the background
            // backgroundSpriteRenderer.size = new Vector2(backgroundWidth, backgroundHeight);

            // // Compute the offset for X position
            // float offsetX = (textWidth - backgroundWidth) / 2f;

            backgroundSpriteRenderer.transform.localPosition = new Vector3(
                backgroundSpriteRenderer.transform.localPosition.x ,
                backgroundSpriteRenderer.transform.localPosition.y,
                backgroundSpriteRenderer.transform.localPosition.z
            );
            textMeshPro.transform.localPosition = new Vector3(
                textMeshPro.transform.localPosition.x, 
                textMeshPro.transform.localPosition.y, 
                textMeshPro.transform.localPosition.z
            );
        }

        private void UpdateChatBubbleOrientation()
        {

            var cameraTransform = Camera.main.transform;
            var headTransform = transform.parent; // Since bubbleTransform == headTransform

            // Debug.Log("Camera: " + cameraTransform.eulerAngles);
            // Debug.Log("Head: " + headTransform.eulerAngles); // Bubble and Head are the same

            // Get direction from head to camera
            var headToCamera = (cameraTransform.position - headTransform.position).normalized;

            // Ensure the chat bubble faces the camera while keeping it upright
            transform.forward = Vector3.ProjectOnPlane(headToCamera, Vector3.up).normalized;
            // Debug.Log("Bubble: " + cameraTransform.eulerAngles);

        }

        private void Log(string message)
        {
            // Log to the console
            Debug.Log(message);

            // Log to the file
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}\n");
        }

    }
}