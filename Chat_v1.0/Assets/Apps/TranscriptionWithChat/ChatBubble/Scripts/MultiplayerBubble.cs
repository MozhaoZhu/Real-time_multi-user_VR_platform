using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Ubiq.Avatars;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Messaging;



public class UnityToArduino : MonoBehaviour
{
    
    // Initializing Avatars
    public AvatarManager avatarManager;
    // public NetworkId id1 = new NetworkId();
    // public NetworkId id2 = new NetworkId();
    //public Transform ava;


    void Start()
    {
        avatarManager = GetComponent<AvatarManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (avatarManager != null){            
            foreach (Avatar avatar in avatarManager.Avatars){
                Debug.Log("name: " + avatar.name);

            }

        }
        else {
            Debug.LogError("AvatarManager component not found on the GameObject.");
        }
    }


}