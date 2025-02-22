using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


using Ubiq.Networking;
using Ubiq.Dictionaries;
using Ubiq.Messaging;
using Ubiq.Logging.Utf8Json;
using Ubiq.Rooms;
using System;
using System.Text;

public class MyBubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
    [SerializeField] private TMP_Text textMeshPro;
    public float typingSpeed = 0.05f;


    private void Start()
    {


        // Initialize the chat bubble with a default message
        Setup("Waiting for messages...");
        context = NetworkScene.Register(this, networkId);
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Canvas/text").GetComponent<TMP_Text>();
    }


    private NetworkId networkId = new NetworkId(99);
    private NetworkContext context;


    [Serializable]
    private struct Message
    {
        public string data;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage data)
    {
        Message message = data.FromJson<Message>();
        Debug.Log("Message received: " + message.data);
        StartCoroutine(TypeText(message.data)); 
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

        // Loop through each character in the message and add it one by one
        foreach (char letter in message)
        {
            textMeshPro.text += letter;  // Add one character to the text
            textMeshPro.ForceMeshUpdate();  // Force the text to update after each letter

            // Adjust background size dynamically as the text is updated
            UpdateBackgroundSize();

            // Wait for the specified typing speed before adding the next character
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void UpdateBackgroundSize(){
        Bounds textBounds = textMeshPro.textBounds;
        Vector2 textSize = new Vector2(textBounds.size.x / 4f, textBounds.size.y );
        Vector3 textPosition = textMeshPro.transform.localPosition;
            
        // Add padding to the background size
        Vector2 padding = new Vector2(0f, 0f); // Adjust padding as needed
        int textLength = textMeshPro.text.Length;
        if (textLength < 10){
            padding.x = 3f;
        }
        backgroundSpriteRenderer.size = 0.25f * textSize + padding;
        
        Vector3 textTopLeftLocal = new Vector3(textBounds.center.x, textBounds.center.y, 0f);
        Vector3 textTopLeftWorld = textMeshPro.transform.TransformPoint(textTopLeftLocal);

        // Align background position
        backgroundSpriteRenderer.transform.position = textTopLeftWorld;        
    }
}
