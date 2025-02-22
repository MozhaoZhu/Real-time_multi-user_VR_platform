using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Ubiq.Networking;
using Ubiq.Dictionaries;
using Ubiq.Messaging;
using Ubiq.Logging.Utf8Json;
using Ubiq.Rooms;
using System;
using System.Text;
using Ubiq.Avatars;
using Avatar = Ubiq.Avatars.Avatar;


public class MyChatDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text textMeshPro;
    [SerializeField] private Image backgroundImage;
    
    public float typingSpeed = 0.05f;

    public Avatar avatar; 
    private string myPeerId; 



    private void Start()
    {
        // Initialize the chat bubble with a default message
        context = NetworkScene.Register(this, networkId);
        backgroundImage = transform.Find("Background").GetComponent<Image>();
        textMeshPro = transform.Find("Background/text").GetComponent<TMP_Text>();
        HideChatBubble();

        avatar = GetComponentInParent<Avatar>();
        if (avatar != null && avatar.Peer != null)
        {
            myPeerId = avatar.Peer.uuid;
        }
        else
        {
            Debug.LogError("Avatar or Peer not initialized.");
        }
    }


    private NetworkId networkId = new NetworkId(99);
    private NetworkContext context;


    [Serializable]
    private struct Message
    {
        public string data;
        public string peer;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage data)
    {
        Message message = data.FromJson<Message>();
        
        // Check if the message is from your avatar
        if (message.peer == myPeerId)
        {
            ShowChatBubble();
            StartCoroutine(TypeText(message.data));
        }
    }

    private IEnumerator TypeText(string message)
    {
        textMeshPro.text = "";  // Clear the existing text

        // Loop through each character in the message and add it one by one
        foreach (char letter in message)
        {
            textMeshPro.text += letter;  // Add one character to the text
            textMeshPro.ForceMeshUpdate();  // Force the text to update after each letter

            yield return new WaitForSeconds(typingSpeed);
        }
        yield return new WaitForSeconds(2f); 
        HideChatBubble();
    }

    private void ShowChatBubble()
    {
        backgroundImage.enabled = true;
        textMeshPro.enabled = true; 
    }

    private void HideChatBubble()
    {
        backgroundImage.enabled = false;
        textMeshPro.enabled = false;
    }
}
