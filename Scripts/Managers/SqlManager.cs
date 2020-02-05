using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;



public class SqlManager : MonoBehaviour
{
    public static SqlManager Instance;

    string loginURL = "http://nutrichef.alwaysdata.net/login/?";
    string registerURL = "http://nutrichef.alwaysdata.net/register/?";
    string getPlayerNameURL = "http://nutrichef.alwaysdata.net/getplayernamefromid/?";
    string getPlayerIdURL = "http://nutrichef.alwaysdata.net/getplayeridfromname/?";

    /* To DELETE
    string ulr = "http://nutrichef.alwaysdata.net/login/?username=User0&password=C0888271869105317DA48AC72C75212E8B00AD3E24C1913B3FE3B9A23707F856";
    */

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
     
    }

    #region HashPassword
    public static byte[] GetHash(string inputString)
    {
        HashAlgorithm algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GenerateSHA256String(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    /*public static string GetStringFromHash(byte[] hash)
    {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            result.Append(hash[i].ToString("X2"));
        }
        return result.ToString();
    }

    public static string GenerateSHA256String(string inputString)
    {
        SHA256 sha256 = SHA256Managed.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(inputString);
        byte[] hash = sha256.ComputeHash(bytes);
        return GetStringFromHash(hash);
    }*/
    #endregion

    #region Login
    public void Login(string _userName, string _password, SqlLoginCallback callback)
    {
        StartCoroutine(SQLLogin(_userName, _password, callback));
    }

    IEnumerator SQLLogin(string username, string password, SqlLoginCallback callback)
    {
        WWWForm form = new WWWForm();
        //URL Command 
        string URL = loginURL + "username=" + username + "&password=" + GenerateSHA256String(password);

        UnityWebRequest download = UnityWebRequest.Post(URL, form);
        //Send Request
        download.SendWebRequest();

        //Waiting for response
        while (!download.isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }

        //Read response
        ReaderHandlerLogin(download.downloadHandler.text, username, callback);
    }

    void ReaderHandlerLogin(string text, string username, SqlLoginCallback callback)
    {
        SignInCommandLog command = SignInCommandLog.none;
        string[] lines = text.Split('|');

        LoginResponse loginReponse = new LoginResponse();

        if (SignInCommandLog.TryParse(lines[0].Substring(0, lines[0].IndexOf(':')), out command))
        {
            switch (command)
            {
                case SignInCommandLog.succes:
                    int userID = -1;
                    int.TryParse(lines[1].Split(':')[1], out userID);

                    if (userID != -1)
                    {
                        loginReponse.command = SignInCommandLog.succes;
                    }
                    else
                    {
                        loginReponse.command = SignInCommandLog.connectionFailed;
                    }
                    loginReponse.userID = userID.ToString();
                    loginReponse.userName = username;
                    break;
                default:
                    break;
            }
        }
        callback(loginReponse);
    }
    #endregion

    #region Register
    public void Register(string _userName, string _password, string _email, SqlRegisterCallback callback)
    {
        StartCoroutine(SQLRegister(_userName, _password, _email, callback));
    }

    void ReaderHandlerRegister(string text, string username, SqlRegisterCallback callback)
    {
        RegisterCommandLog command = RegisterCommandLog.none;
        string[] lines = text.Split('|');

        RegisterResponse registerReponse = new RegisterResponse();

        if (RegisterCommandLog.TryParse(lines[0].Substring(0, lines[0].IndexOf(':')), out command))
        {
            registerReponse.command = command;
            registerReponse.userName = username;
        }
        callback(registerReponse);
    }

    IEnumerator SQLRegister(string username, string password, string email, SqlRegisterCallback callback)
    {
        WWWForm form = new WWWForm();
        //URL Command 
        string URL = registerURL + "username=" + username + "&password=" + GenerateSHA256String(password) + "&email=" + email;

        UnityWebRequest download = UnityWebRequest.Post(URL, form);
        //Send Request
        download.SendWebRequest();

        //Waiting for response
        while (!download.isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }
        //Read response
        ReaderHandlerRegister(download.downloadHandler.text, username, callback);
    }
    #endregion

    #region GetPlayerName
    public void GetPlayerName(string _userId, SqlGetPlayerNameCallback callback)
    {
        StartCoroutine(SQLGetPlayerName(_userId, callback));
    }

    IEnumerator SQLGetPlayerName(string _userId, SqlGetPlayerNameCallback callback)
    {
        WWWForm form = new WWWForm();
        //URL Command 
        string URL = getPlayerNameURL + "userId=" + _userId;

        UnityWebRequest download = UnityWebRequest.Post(URL, form);
        //Send Request
        download.SendWebRequest();

        //Waiting for response
        while (!download.isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }

        //Read response
        ReaderHandlerGetPlayerName(download.downloadHandler.text, _userId, callback);
    }

    void ReaderHandlerGetPlayerName(string text, string _userId, SqlGetPlayerNameCallback callback)
    {
        GetPlayerNameCommandLog command = GetPlayerNameCommandLog.none;
        string[] lines = text.Split('|');

        GetPlayerNameResponse getPlayerNameResponse = new GetPlayerNameResponse();

        if (GetPlayerNameCommandLog.TryParse(lines[0].Substring(0, lines[0].IndexOf(':')), out command))
        {
            getPlayerNameResponse.command = command;
        }

        if (command == GetPlayerNameCommandLog.succes)
        {
            getPlayerNameResponse.userName = lines[1].Substring(lines[1].IndexOf(':'));
            getPlayerNameResponse.userId = _userId;
        }

        callback(getPlayerNameResponse);
    }
    #endregion

    #region GetPlayerName
    public void GetPlayerId(string _userName, SqlGetPlayerIdCallback callback)
    {
        StartCoroutine(SQLGetPlayerId(_userName, callback));
    }

    IEnumerator SQLGetPlayerId(string _userName, SqlGetPlayerIdCallback callback)
    {
        WWWForm form = new WWWForm();
        //URL Command 
        string URL = getPlayerIdURL + "username=" + _userName;

        UnityWebRequest download = UnityWebRequest.Post(URL, form);
        //Send Request
        download.SendWebRequest();

        //Waiting for response
        while (!download.isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }

        //Read response
        ReaderHandlerGetPlayerId(download.downloadHandler.text, _userName, callback);
    }

    void ReaderHandlerGetPlayerId(string text, string _userName, SqlGetPlayerIdCallback callback)
    {
        GetPlayerIdCommandLog command = GetPlayerIdCommandLog.none;
        string[] lines = text.Split('|');

        GetPlayerIdResponse GetPlayerIdResponse = new GetPlayerIdResponse();

        if (GetPlayerIdCommandLog.TryParse(lines[0].Substring(0, lines[0].IndexOf(':')), out command))
        {
            GetPlayerIdResponse.command = command;
        }

        if (command == GetPlayerIdCommandLog.succes)
        {
            GetPlayerIdResponse.userId = lines[1].Substring(lines[1].IndexOf(':') + 1);
            GetPlayerIdResponse.userName = _userName;
        }
        callback(GetPlayerIdResponse);
    }
    #endregion
}
