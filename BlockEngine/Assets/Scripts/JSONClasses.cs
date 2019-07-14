using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HelloMessage
{
    public string type;
    public string name;
}


[System.Serializable]
public class WelcomeMessage
{
    public string type;
    public string position;
}


[System.Serializable]
public class ClientMessage
{
    public string type;
    public string receiver;
    public string content;
}


[System.Serializable]
public class ServerMessage
{
    public string type;
    public string sender;
    public string content;
    public bool world;
}


[System.Serializable]
public class GenericMessage
{
    public string type;
}

[System.Serializable]
public class BlockActionMessage
{
    public string type;
	public string blockID;
	public string position;
}

[System.Serializable]
public class PositionUpdateMessage
{
    public string type;
	public string name;
	public string position;
}

[System.Serializable]
public class ServerStateMessage
{
    public string type;
    public string positions;
}

[System.Serializable]
public class ServerNewcomerMessage
{
    public string type;
	public string name;
    public string position;
}

[System.Serializable]
public class ServerDisconnectMessage
{
    public string type;
	public string name;
}