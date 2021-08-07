using Photon.Pun;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class SeekBarControl : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{
    public VideoManager vManager;

    bool slide = false;

    public VideoPlayer videoPlayer;
    public Slider videoSeek;

    public PhotonView view;

    long videoFrame = 0;
    float videoSeekTime = 0.0f;

    private void Update()
    {
        if (!slide)
        {
            float val = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
            if (PhotonNetwork.IsMasterClient)
            {
                videoSeek.value = val;
            }   
            
            vManager.seekControl(val);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        slide = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float frame = (float)videoSeek.value * (float)videoPlayer.frameCount;
        if (PhotonNetwork.IsMasterClient)
        {
            videoPlayer.frame = (long)frame;
        }
        
        vManager.VideoFrameChanged(frame);
        slide = false;
    }
}
