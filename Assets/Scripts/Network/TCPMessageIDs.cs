using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum TCPMessageID
{
    //server -> client
    HELLO, //welcomes client
    LOGIN_RESPONSE, //login result
    USER_LOGIN, //a user logged into the game
    USER_LOGOUT, //a user logged out

    //client -> server
    LOGIN_REQUEST, //client asks for login
    READY, //client finished loading
}