using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignInManager : MonoBehaviour
{
    public GameObject signIn_Panel;
    public GameObject createJoinLobby_Panel;

    public InputField user_InputTextField;
    public InputField password_InputTextField;

    public void LogIn()
    {
        if(user_InputTextField.text.Length > 3 && password_InputTextField.text.Length > 7)
        {
            createJoinLobby_Panel.SetActive(true);
            signIn_Panel.SetActive(false);

            PhotonNetwork.LocalPlayer.NickName = user_InputTextField.text.ToString();
            // establish connection to photon server
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void LoadVideoScene(int index)
    {
        PlayerPrefs.SetInt("PlayVideoIndex", index);
    }
}
