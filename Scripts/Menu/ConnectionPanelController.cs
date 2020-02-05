using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Login
{
    [Space]
    public InputField userNameText;
    public InputField userPasswordText;
    public Text InfoText;
}

[System.Serializable]
public class RegisterData
{
    [Space]
    public Text userNameText;
    public Text userEmail;
    public InputField userPasswordText;
    public InputField userPasswordVerifText;
    public Text InfoText;
}

public class ConnectionPanelController : MonoBehaviourPunCallbacks
{
    [Space]
    [SerializeField] Login loginData;
    [SerializeField] RegisterData registerData;
    [Space]
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject RegisterPanel;
    [Space]
    [SerializeField] Button ButtonConnection;
    [SerializeField] Button ButtonOfflineMode;
    [SerializeField] Button ButtonCreateAccount;

    SqlManager sqlManager;

    private void Start()
    {
        sqlManager = SqlManager.Instance;
        PhotonNetwork.OfflineMode = false;
    }

    private void ButtonsInteractable(bool state)
    {
        ButtonConnection.interactable = state;
        ButtonOfflineMode.interactable = state;
        ButtonCreateAccount.interactable = state;
    }

    #region OnButtons
    public void OnButtonLoginClick()
    {
        ButtonsInteractable(false);
        Coroutine waiting = StartCoroutine(WaitingForSQLSignIn());
        sqlManager.Login(loginData.userNameText.text, loginData.userPasswordText.text, LoginCallback);
        StopCoroutine(waiting);
    }

    public void OnButtonRegisterClick()
    {
        if (registerData.userPasswordText.text.Length < 6)
        {
            Debug.Log("Password Need 6 Character min");
            return;
        }

        if (registerData.userPasswordText.text != registerData.userPasswordVerifText.text)
        {
            Debug.Log("Password Confirmation is diferent");
            return;
        }

        Coroutine waiting = StartCoroutine(WaitingForSQLRegister());
        sqlManager.Register(registerData.userNameText.text, registerData.userPasswordText.text, registerData.userEmail.text, RegisterCallback);
        StopCoroutine(waiting);
    }

    public void OnButtonCreateAccount()
    {

    }
    
    public void OnButtonOfflineMode()
    {
        //PhotonNetwork.OfflineMode = true;
        SceneManager.LoadScene("Lobby");
    }

    #endregion

    #region Coroutines
    IEnumerator WaitingForSQLSignIn()
    {
        int nbPoint = 0;
        while (true)
        {
            loginData.InfoText.text = "Checking UserName And Password";
            for (int i = 0; i < nbPoint; i++)
            {
                loginData.InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator WaitingForSQLRegister()
    {
        registerData.InfoText.gameObject.SetActive(true);
        int nbPoint = 0;
        while (true)
        {
            registerData.InfoText.text = "Checking register data";
            for (int i = 0; i < nbPoint; i++)
            {
                registerData.InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator WaitingForConnectMaster()
    {
        loginData.InfoText.gameObject.SetActive(true);
        int nbPoint = 0;
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            loginData.InfoText.text = "Connecting To Server";
            for (int i = 0; i < nbPoint; i++)
            {
                loginData.InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
        loginData.InfoText.gameObject.SetActive(false);
    }
    #endregion

    public void Connect(string userName, string userID)
    {
        StartCoroutine(WaitingForConnectMaster());
        AuthenticationValues authentication = new AuthenticationValues();
        authentication.UserId = userID;
        PhotonNetwork.AuthValues = authentication;

        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.GameVersion = "alpha";
        PhotonNetwork.NickName = userName;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }

    #region Callbacks
    void LoginCallback(LoginResponse loginResponse)
    {
        switch (loginResponse.command)
        {
            case SignInCommandLog.none:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            case SignInCommandLog.succes:
                GameManager.Instance.Audio.PlaySound("SweetDoubleHit", AudioManager.Canal.SoundEffect);
                Connect(loginResponse.userName, loginResponse.userID);
                break;
            case SignInCommandLog.connectionFailed:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            case SignInCommandLog.incorrectName:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            case SignInCommandLog.incorrectPassword:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            case SignInCommandLog.usernamecheckfailed:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            case SignInCommandLog.wrongUsername:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            case SignInCommandLog.wrongPassword:
                GameManager.Instance.Audio.PlaySound("ErrorDoubleHit", AudioManager.Canal.SoundEffect);
                ButtonsInteractable(true);
                break;
            default:
                break;
        }
        Debug.Log("responce : " + loginResponse.command);

    }

    void RegisterCallback(RegisterResponse registerResponse)
    {
        Debug.Log("register :" + registerResponse.command);

        if (registerResponse.command == RegisterCommandLog.succes)
        {
            loginPanel.SetActive(true);
            RegisterPanel.SetActive(false);
            loginData.userNameText.text = registerResponse.userName;
            loginData.userPasswordText.text = "";
        }

    }
    #endregion


}
