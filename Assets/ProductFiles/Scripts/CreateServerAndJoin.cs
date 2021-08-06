using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateServerAndJoin : MonoBehaviourPunCallbacks
{
    public GameObject createAndJoinLobby_Panel;
    public GameObject readyPlayer_Panel;
    public GameObject videoSelection_Panel;
    public GameObject videoSelection_Button;
    public GameObject watchVideo_Button;

    public GameObject roomListContent;
    public GameObject roomList_Prefab;

    public GameObject playerListContent;
    public GameObject PlayerListEntryPrefab;

    public InputField roomName_ITF;
    public Text errorMessage_Txt;

    [SerializeField]
    private byte maxPlayers = 20;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;

    void Awake()
    {
        // this make sure that the loaded scene is same for every connected player
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

    }


   public void OnCreateRoomButtonClicked()
    {
        if (!roomName_ITF.text.Equals(""))
        {
            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };
            PhotonNetwork.CreateRoom(roomName_ITF.text.ToString(), options, null);
            
        }
        else
        {
            errorMessage_Txt.text = "Room name cannot be null";
        }
    }

    public void ShowListOfRoomsAvailable()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    // back from video Selection Panel, after selecting a video
    public void OnBackButtonClicked()
    {
        this.SetActivePanel(readyPlayer_Panel.name);
    }

    public void OnVideoSelectionButtonClicked()
    {
        this.SetActivePanel(videoSelection_Panel.name);
    }

    public void OnStartTrainingButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("360Video");
    }

    public void OnLeaveRoomButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    void SetActivePanel(string activePanel)
    {
        createAndJoinLobby_Panel.SetActive(activePanel.Equals(createAndJoinLobby_Panel.name));
        readyPlayer_Panel.SetActive(activePanel.Equals(readyPlayer_Panel.name));    // UI should call OnRoomListButtonClicked() to activate this
        videoSelection_Panel.SetActive(activePanel.Equals(videoSelection_Panel.name));
    }

    #region MonoBehaviourPunCallbacks Callbacks
    public override void OnConnectedToMaster()
    {
        this.SetActivePanel(createAndJoinLobby_Panel.name);

        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnJoinedLobby()
    {
        // whenever this joins a new lobby, clear any previous room lists
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(createAndJoinLobby_Panel.name);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(createAndJoinLobby_Panel.name);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinedRoom()
    {
        cachedRoomList.Clear();

        SetActivePanel(readyPlayer_Panel.name);

        if(playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach(Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(playerListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<UserList>().Initialize(p.ActorNumber, p.NickName);

            object isPlayerReady;
            
            if(p.CustomProperties.TryGetValue(VideoDetail.USER_READY, out isPlayerReady))
            {
                entry.GetComponent<UserList>().SetPlayerReady((bool) isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);

            if (PhotonNetwork.IsMasterClient)
            {
                videoSelection_Button.SetActive(true);
            }

            watchVideo_Button.gameObject.SetActive(CheckPlayersReady());

            Hashtable props = new Hashtable
            {
                {VideoDetail.USER_LOADED_LEVEL, false}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }

    public override void OnLeftRoom()
    {
        SetActivePanel(createAndJoinLobby_Panel.name);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab);
        entry.transform.SetParent(readyPlayer_Panel.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<UserList>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        watchVideo_Button.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        watchVideo_Button.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            watchVideo_Button.gameObject.SetActive(CheckPlayersReady());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue(VideoDetail.USER_READY, out isPlayerReady))
            {
                entry.GetComponent<UserList>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        watchVideo_Button.gameObject.SetActive(CheckPlayersReady());
    }
    #endregion

    void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach(RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    void UpdateRoomListView()
    {
        foreach(RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(roomList_Prefab);
            entry.transform.SetParent(roomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomList>().Initialize(info.Name);

            roomListEntries.Add(info.Name, entry);
        }
    }

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(VideoDetail.USER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void LocalPlayerPropertiesUpdated()
    {
        watchVideo_Button.gameObject.SetActive(CheckPlayersReady());
    }
}
