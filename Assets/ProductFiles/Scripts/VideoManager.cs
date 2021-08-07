using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VideoManager : MonoBehaviourPunCallbacks
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips;

    public Button play_Button;
    //public Button pause_Button;
    public Slider videoSeek;

    //float videoSeekTime = 0.0f;
    //long videoFrame = 0;

    //bool slide = false;

    private List<int> userList;

    public PhotonView view;

    public static VideoManager Instance = null;

    Hashtable sseekValu = new Hashtable();

    public void Awake()
    {
        Instance = this;

        userList = new List<int>();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            userList.Add(p.ActorNumber);
        }
    }
    void Start()
    {
        videoPlayer.clip = videoClips[PlayerPrefs.GetInt("PlayVideoIndex")];

        if (PhotonNetwork.IsMasterClient)
        {
            play_Button.interactable = true;
            //pause_Button.interactable = true;
            videoSeek.interactable = true;
        }
    }

    public void VideoControls() {
        if (videoPlayer.isPlaying)
        {
            view.RPC("ChangeVideoState", RpcTarget.AllViaServer, "pause");
        }
        else if (videoPlayer.isPaused)
        {
            view.RPC("ChangeVideoState", RpcTarget.AllViaServer, "play");
        }
    }

    public void seekControl(float val)
    {
        view.RPC("SeekBarPositionChanged", RpcTarget.Others, val);
    }

    public void VideoFrameChanged(float frame)
    {
        view.RPC("FrameChanged", RpcTarget.Others, frame);
    }

    #region PUN CALLBACKS
    [PunRPC]
    public void SeekBarPositionChanged(float seekVal, PhotonMessageInfo info)
    {
        videoSeek.value = seekVal;
    }

    [PunRPC]
    public void FrameChanged(float frame, PhotonMessageInfo info)
    {
        videoPlayer.frame = (long)frame;
    }

    [PunRPC]
    public void ChangeVideoState(string value, PhotonMessageInfo info)
    {
        if (value.Contains("pause"))
        {
            videoPlayer.Pause();
            play_Button.GetComponentInChildren<Text>().text = "Play";
        }
        else if (value.Contains("play"))
        {
            videoPlayer.Play();
            play_Button.GetComponentInChildren<Text>().text = "Pause";
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                play_Button.interactable = true;
                //pause_Button.interactable = true;
                videoSeek.interactable = true;
            }
        }
    }

    #endregion
}
