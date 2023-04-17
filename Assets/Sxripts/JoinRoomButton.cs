using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinRoomButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    
    
    public void JoinRoom()
    {
        Debug.Log(roomNameText.text.Split(' ')[0]);
        MultiplayerManager.Instance.JoinRoom(roomNameText.text.Split(' ')[0]);
        Destroy(gameObject);
    }
}
