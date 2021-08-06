using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VideoManager : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips;

    public Button play_Button;
    public Button pause_Button;
    public Slider videoSeek;

    float videoSeekTime = 0.0f;
    long videoFrame = 0;

    bool slide = false;

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

        //PhotonNetwork.Instantiate("VideoPlayerPrefab", new Vector3(0,0,0), Quaternion.identity);

        //videoPlayer = GameObject.FindObjectOfType<VideoPlayer>();
        //view = GameObject.FindObjectOfType<PhotonView>();
    }
    void Start()
    {
        videoPlayer.clip = videoClips[PlayerPrefs.GetInt("PlayVideoIndex")];

        if (PhotonNetwork.IsMasterClient)
        {
            play_Button.interactable = true;
            pause_Button.interactable = true;
            videoSeek.interactable = true;
        }

        Hashtable props = new Hashtable
            {
                {VideoDetail.USER_LOADED_LEVEL, true}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void Update()
    {
        if (!slide)
        {
            if (videoPlayer != null)
            {
                videoSeek.value = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
                videoSeekTime = videoSeek.value;

                if (PhotonNetwork.IsMasterClient)
                {
                    sseekValu[VideoDetail.VIDEO_SEEK] = videoSeekTime;
                }
            }
        }
    }

    //static float SyncVideoSeek()
    //{
    //    Player p;
    //    object seek;
    //    if (p.CustomProperties.TryGetValue(VideoDetail.VIDEO_SEEK, out seek))
    //    {
    //        return (float)seek;
    //    }

    //    return 0;
    //    //if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(VideoDetail.VIDEO_SEEK, out seek))
    //    //{
    //    //    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { VideoDetail.VIDEO_SEEK, ((float)seek = videoSeekTime) } });
    //    //}
    //}

    public void VideoControls (string value){
        if (value.Contains("Play"))
        {
            videoPlayer.Play();
            Debug.Log("h");
            //if (!PhotonNetwork.IsMasterClient)
            if (!view.IsMine)
            {
                Debug.Log("hhh");
                videoPlayer.Play();
            }

        }
        else
        {
            videoPlayer.Pause();

            //if (!PhotonNetwork.IsMasterClient)
            if (!view.IsMine)
            {
                Debug.Log("jjj");
                videoPlayer.Pause();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        slide = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float frame = (float)videoSeek.value * (float)videoPlayer.frameCount;
        videoPlayer.frame = (long)frame;
        videoFrame = videoPlayer.frame;

        slide = false;
    }


   

    #region PUN CALLBACKS
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                play_Button.interactable = true;
                pause_Button.interactable = true;
                videoSeek.interactable = true;
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == targetPlayer.ActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("ddd");
                videoSeek.value = videoSeekTime;
                videoPlayer.frame = videoFrame;
            }
        }
    }

    #endregion
}
