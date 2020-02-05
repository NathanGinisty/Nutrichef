using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#region Login
public enum SignInCommandLog
{
    none = -1,
    succes,
    connectionFailed,
    incorrectName,
    incorrectPassword,
    usernamecheckfailed,
    wrongUsername,
    wrongPassword
}

public delegate void SqlLoginCallback(LoginResponse loginResponse);

public class LoginResponse
{
    public SignInCommandLog command;
    public string userID;
    public string userName;

    public LoginResponse()
    {
        command = SignInCommandLog.none;
        userID = "-1";
        userName = "";
    }
}
#endregion

#region Register
public enum RegisterCommandLog
{
    none = -1,
    succes,
    connectionFailed,
    incorrectName,
    incorrectPassword,
    eMailNotExist,
    eMailNotValid,
    usernameCheckFailed,
    usernameAlreadyTaken,
    eMailCheckFailed,
    eMailAlreadyTaken,
    insertQueryFailed
}

public delegate void SqlRegisterCallback(RegisterResponse registerResponse);

public class RegisterResponse
{
    public RegisterCommandLog command;
    public string userName;

    public RegisterResponse()
    {
        command = RegisterCommandLog.none;
        userName = "";
    }
}
#endregion

#region GetPlayerName
public enum GetPlayerNameCommandLog
{
    none = -1,
    succes,
    connectionFailed,
    incorrectId,
    querryCheckFailed,
    userNameNotExist,
}

public delegate void SqlGetPlayerNameCallback(GetPlayerNameResponse getPlayerNameResponse);

public class GetPlayerNameResponse
{
    public GetPlayerNameCommandLog command;
    public string userName;
    public string userId;

    public GetPlayerNameResponse()
    {
        command = GetPlayerNameCommandLog.none;
        userName = "";
        userId = "-1";
    }
}
#endregion

#region GetPlayerId
public enum GetPlayerIdCommandLog
{
    none = -1,
    succes,
    connectionFailed,
    incorrectName,
    querryCheckFailed,
    userNameNotExist,
}

public delegate void SqlGetPlayerIdCallback(GetPlayerIdResponse GetPlayerIdResponse);

public class GetPlayerIdResponse
{
    public GetPlayerIdCommandLog command;
    public string userName;
    public string userId;

    public GetPlayerIdResponse()
    {
        command = GetPlayerIdCommandLog.none;
        userName = "";
        userId = "-1";
    }
}
#endregion
