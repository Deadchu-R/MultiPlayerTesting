using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    private const string CURRENT_ROOM_PLAYERS_PATTERN = "{0}/{1}";
    public static MultiplayerManager Instance { get; private set; }

    [Header("PlayerAttributes")] [SerializeField]
    private TMP_InputField nickNameInputField;

    [Header("Canvas")] [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject roomCanvas;
    [SerializeField] private GameObject lobbyCanvas;

    [Header("Rooms Controls")] [SerializeField]
    private Button createRoomButton;
    [SerializeField] private Button connectToServersButton;
    [SerializeField] private TMP_InputField roomNameToCreate;
    [SerializeField] private TextMeshProUGUI currentRoomsText;
    [SerializeField] private TextMeshProUGUI currentPlayerCount;
    [SerializeField] private GameObject roomListContent;
    [SerializeField] private GameObject JoinGameButtonPrefab;
    [SerializeField] private GameObject leaveRoomButton;
    [SerializeField] private GameObject startGameButton;
  

    [Header("Debug Texts")] [SerializeField]
    private TextMeshProUGUI nickNameDebugText;

    [SerializeField] private TextMeshProUGUI playersListText;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        leaveRoomButton.SetActive(false);
        createRoomButton.interactable = false;
        startGameButton.SetActive(false);
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(false);
        createRoomButton.interactable = false;
        currentPlayerCount.text = string.Format(CURRENT_ROOM_PLAYERS_PATTERN, 0, 0);
        
        
    }

    #region nickNameMethods

    private bool IsPlayerChoseNickName()
    {
        if (!string.IsNullOrEmpty(nickNameInputField.text) && nickNameInputField.text != "EnterNameHere")
        {
            ChangeNickName();
            return true;
        }
        else
        {
       
            nickNameDebugText.text = "Nickname is empty or invalid";
            return false;
        }
    }

    private void ChangeNickName()
    {
        PhotonNetwork.NickName = nickNameInputField.text;

    }

    private bool IsNickNameAvailable()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (nickNameInputField.text == player.NickName)
            {
   
                nickNameDebugText.text = "Nickname already taken";
                return false;
            }
        }

        return true;
    }

    #endregion


    #region ConnectingToServersMethods

    public void ConnectToServers()
    {
        if (IsPlayerChoseNickName() == true)
        {
            PhotonNetwork.ConnectUsingSettings();
            if (IsNickNameAvailable() == false)
            {
                DisconnectFromServers();
            }
            else
            {
                mainMenuCanvas.SetActive(false);
                roomCanvas.SetActive(true);
            }
        }
    }

    private void DisconnectFromServers()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("<color=#00ff00>We are connected!</color>");
        createRoomButton.interactable = true;
        PhotonNetwork.JoinLobby();
        lobbyCanvas.SetActive(true);
    }

    #endregion

    #region RoomMethods

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        leaveRoomButton.SetActive(true);
        lobbyCanvas.SetActive(true);
        RefreshRoomList();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        leaveRoomButton.SetActive(false);
    }



    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RefreshRoomList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RefreshRoomList();
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                Debug.Log("cannot start game");
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError("Create failed..." + Environment.NewLine + message);
        createRoomButton.interactable = true;
    }

    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        RefreshRoomList();
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && PhotonNetwork.IsMasterClient)
            {
                Debug.Log("can start game");
                startGameButton.SetActive(true);
            }
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby");
        
        RefreshRoomList();
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameToCreate.text) && roomNameToCreate.text != "EnterDesiredRoomName" && PhotonNetwork.IsConnected)
        {
            PhotonNetwork.CreateRoom(roomNameToCreate.text, new RoomOptions { MaxPlayers = 5 });
            Debug.Log("created room");
            Debug.Log($"player: {PhotonNetwork.NickName} is in room: {PhotonNetwork.InRoom}");
            createRoomButton.interactable = false;
        }
        else
        {
            Debug.Log("Room name is empty or invalid");
        }
        RefreshRoomList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log("Room list updated!");
        foreach (var room in roomList)
        {

            if (!PhotonNetwork.InRoom && !room.RemovedFromList && PhotonNetwork.InLobby )
            {
                GameObject joinGameButton;

                joinGameButton = Instantiate(JoinGameButtonPrefab, roomListContent.transform);
                joinGameButton.GetComponentInChildren<TextMeshProUGUI>().text = room.Name + ' ' + string.Format(CURRENT_ROOM_PLAYERS_PATTERN, room.PlayerCount, room.MaxPlayers);
                if (room.PlayerCount >= room.MaxPlayers)
                {
                    joinGameButton.GetComponent<Button>().interactable = false;
                }
                else
                {
                    joinGameButton.GetComponent<Button>().interactable = true;
                }
               

                Debug.Log("instantiated join game button");
            }
         

     
            
        }
        
        RefreshRoomList();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void RefreshRoomList()
    {
        playersListText.text = string.Empty;
 
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.InRoom)
        {
            currentPlayerCount.text = string.Format(CURRENT_ROOM_PLAYERS_PATTERN,
            PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);
            foreach (var player in PhotonNetwork.PlayerList)
            {
                playersListText.text += player.NickName + Environment.NewLine;
        
            }
        }
        else
        {
            currentPlayerCount.text = string.Format(CURRENT_ROOM_PLAYERS_PATTERN, 0, 0);
        }
        
    }

    #endregion
}