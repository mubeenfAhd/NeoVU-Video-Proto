using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using UnityEngine;

public class VideoSceneManager : MonoBehaviourPunCallbacks
{
    //public GameObject videoPlayer_Prefab;

    public void Start()
    {
        Hashtable props = new Hashtable
            {
                {VideoDetail.USER_LOADED_LEVEL, true}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void BackToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    #region PUN CALLBACKS

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }
    #endregion
}
