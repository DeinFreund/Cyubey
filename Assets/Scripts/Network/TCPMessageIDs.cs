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
    CHECK_HASH, //server sends client chunk hash to check sync
    UPDATE_BLOCK, //server sends single block update

    //client -> server
    LOGIN_REQUEST, //client asks for login
    READY, //client finished loading
    REQUEST_CHUNK, //client asks for chunk data
    REQUEST_HASH, //client asks for chunk hash
    SET_BLOCK, //client asks to set block
}