using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Chat;
using ExitGames.Client.Photon;


public enum ChatActionEnum
{
    Nothing,
    InvitationFriend,
    ResponseInvitationFriend,
    InvitationGame,
    Connection,
}

public class ChatListner : MonoBehaviour, IChatClientListener
{
    LobbyPhotonController lobbyPhotonController;
    ChatClient chatClient;
    System.DateTime connectionDate;

    private void Start()
    {
        lobbyPhotonController = GetComponent<LobbyPhotonController>();

        if (!lobbyPhotonController.offlineMode)
        {
            chatClient = new ChatClient(this);
            chatClient.ChatRegion = "EU";

            chatClient.Connect("ebc68e28-3a2d-4e12-89e4-bdcffe637fa5", "alpha", new AuthenticationValues(lobbyPhotonController.GetPlayerUserId()));
        }
        else
        {
            Destroy(this);
        }
    }

    public void Update()
    {
        chatClient.Service();
    }

    public void SendMessageCommand(string user, string message)
    {
        chatClient.SendPrivateMessage(user, message);
    }

    public void DebugReturn(DebugLevel level, string message)
    {
#if UNITY_EDITOR
        Debug.Log("Debug Message level" + level + " message:" + message);
#endif
    }

    public void OnChatStateChange(ChatState state)
    {

    }

    public void OnConnected()
    {
#if UNITY_EDITOR
        Debug.Log("Connect to chat");
#endif
        connectionDate = System.DateTime.UtcNow;
        string message = "</From " + PhotonNetwork.LocalPlayer.UserId + " /Date " + connectionDate + " /Act " + (int)ChatActionEnum.Connection + " />";
        chatClient.SendPrivateMessage(lobbyPhotonController.GetPlayerUserId(), message);
        //Debug.Log("Send -> " + message);
    }

    public void OnDisconnected()
    {

    }


    int borneLeft(string text, string balise)
    {
        return text.IndexOf(balise) + balise.Length;
    }

    int borneRight(string text, int beginPos = 0)
    {
        return text.IndexOf(" /", beginPos);
    }

    string GetStringFromMessageString(string msgString, string balise)
    {
        int tempPos = borneLeft(msgString, balise);
        return msgString.Substring(tempPos, borneRight(msgString, tempPos) - tempPos);
    }


    #region Used Ichat Methods

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        Debug.Log("message chat");
    }

    private void OnDrawGizmos()
    {

    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        ChatActionEnum currentAction = ChatActionEnum.Nothing;
        string msgString = message.ToString();

        int posBegin = borneLeft(msgString, "/Act ");
        int posEnd = msgString.IndexOf(" />", posBegin);

        string actionType = GetStringFromMessageString(msgString, "/Act "); // msgString.Substring(posBegin, posEnd - posBegin);
        ChatActionEnum.TryParse(actionType, out currentAction);

        string tempString = GetStringFromMessageString(msgString, "/From ");
        //si c est soi meme
        if (tempString == PhotonNetwork.LocalPlayer.UserId && currentAction != ChatActionEnum.Connection)
        {
            return;
        }
        //Debug.Log(currentAction);
        switch (currentAction)
        {

            case ChatActionEnum.Nothing:
                break;

            case ChatActionEnum.InvitationFriend:
                //id
                GetStringFromMessageString(msgString, "/From ");

                //username
                GetStringFromMessageString(msgString, "/Name ");
                break;

            case ChatActionEnum.ResponseInvitationFriend:
                //username
                GetStringFromMessageString(msgString, "/Name ");
                break;

            case ChatActionEnum.InvitationGame:
                //id
                string id = GetStringFromMessageString(msgString, "/From ");

                //username
                string userName = GetStringFromMessageString(msgString, "/Name ");

                //roomName
                string roomName = GetStringFromMessageString(msgString, "/Into ");

                lobbyPhotonController.OnReceiveGameInvitation(userName, id, roomName);

                break;
            case ChatActionEnum.Connection:
                //username
                System.DateTime _connectionDate = Convert.ToDateTime(GetStringFromMessageString(msgString, "/Date "));

                if (connectionDate < _connectionDate)
                {
                    Debug.Log("Deconnect cause of other user is using this account");
                    PhotonNetwork.Disconnect();
                }

                break;

            default:
                Debug.Log("no action in your message");

                break;
        }

    }

    #endregion

    #region new Ichat methods 

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        throw new System.NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
