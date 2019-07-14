using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatControl : MonoBehaviour
{
    [SerializeField]
    float fadeOutTime = 5f;

    [SerializeField]
    GameObject otherPlayerPrefab = null;
	
	[SerializeField]
	GameObject ownPlayer = null;

    GameObject UIChatText = null;

    GameObject UIChatInput = null;

    UnityEngine.UI.Text chatText = null;

    UnityEngine.UI.InputField inputField = null;

    System.Text.StringBuilder b = new System.Text.StringBuilder();

    System.Net.Sockets.TcpClient chatClient = null;

    private float TimeLeftUntilFade = 0f;

    private List<OtherPlayer> otherPlayerList = new List<OtherPlayer>();

    private string ownName;
	
	private BlockPlacement blockPlacementInstance = null;
	
	private Vector3 lastPosition;
	
	private int pingTimer;


    // Start is called before the first frame update
    void Start()
    {
        this.UIChatText = GameObject.Find("UIChatText");
        this.UIChatInput = GameObject.Find("UIChatInput");
        this.chatText = GameObject.Find("ChatText").GetComponent<UnityEngine.UI.Text>();
        this.inputField = GameObject.Find("InputField").GetComponent<UnityEngine.UI.InputField>();
		this.blockPlacementInstance = ownPlayer.GetComponent<BlockPlacement>();
		this.lastPosition = ownPlayer.transform.position;
		this.pingTimer = System.Environment.TickCount;

        b.AppendLine(chatText.text);
    }


    // Update is called once per frame
    void Update()
    {
        checkForNewServerMessages();
		sendPositionUpdateToServer();
    }


    void checkForNewServerMessages()
    {
        if (chatClient != null)
        {
            if (chatClient.Available > 0)
            {
                System.Text.StringBuilder response = new System.Text.StringBuilder();

                while (chatClient.Available > 0)
                {
                    byte[] data = new byte[chatClient.Available];
                    System.Int32 bytes = chatClient.GetStream().Read(data, 0, data.Length);
                    response.Append(System.Text.Encoding.UTF8.GetString(data, 0, bytes));
                }

                handleServerResponse(response.ToString());
            }
        }
        if (TimeLeftUntilFade > 0f)
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                TimeLeftUntilFade = 0f;
                return;
            }
            UIChatText.GetComponent<CanvasGroup>().alpha = 1f;
            TimeLeftUntilFade -= Time.deltaTime;
            if (TimeLeftUntilFade <= 0f)
            {
                UIChatText.GetComponent<CanvasGroup>().alpha = 0f;
            }
        }
    }


    void handleServerResponse(string response)
    {

        string[] singleJSONStrings = response.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < singleJSONStrings.Length; i++)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"end\""))
            {
                b.AppendLine("Server terminated connection");
                chatClient.Close();
                chatClient = null;
                continue;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"welcome\""))
            {
                b.AppendLine("Received welcome from server!\nFollowing coordinates have been assigned to you:");
                WelcomeMessage message = UnityEngine.JsonUtility.FromJson<WelcomeMessage>(singleJSONStrings[i]);
                b.AppendLine(message.position);
				this.teleportAvatar(message.position);
                continue;
            }
			
			if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"newcomer\""))
            {
                b.Append("New player connected! Name: ");
                ServerNewcomerMessage message = UnityEngine.JsonUtility.FromJson<ServerNewcomerMessage>(singleJSONStrings[i]);
                b.Append(message.name);
				b.Append(" Position: ");
				b.Append(message.position);
				b.Append("\n");
				
				try {
					string[] coords = message.position.Split(',');
					float x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
					float y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
					float z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture);
					otherPlayerList.Add(new OtherPlayer(message.name, x, y, z, otherPlayerPrefab));
				} catch (System.Exception e) {
					b.AppendLine(e.ToString());
				}
                continue;
            }
			
			if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"disconnect\""))
            {
                ServerDisconnectMessage message = UnityEngine.JsonUtility.FromJson<ServerDisconnectMessage>(singleJSONStrings[i]);
                b.Append("Player ");
				b.Append(message.name);
				b.Append(" disconnected!\n");
				OtherPlayer disconnected = otherPlayerList.Find(x => x.getName().Equals(message.name));
				otherPlayerList.Remove(disconnected);
				disconnected.destroy();
                continue;
            }
			
			if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"blockAction\""))
            {
				try {
					BlockActionMessage message = UnityEngine.JsonUtility.FromJson<BlockActionMessage>(singleJSONStrings[i]);
					
					string[] positionRaw = message.position.Split(',');
					float x = float.Parse(positionRaw[0], System.Globalization.CultureInfo.InvariantCulture);
					float y = float.Parse(positionRaw[1], System.Globalization.CultureInfo.InvariantCulture);
					float z = float.Parse(positionRaw[2], System.Globalization.CultureInfo.InvariantCulture);
					Vector3 position = new Vector3(x, y, z);
					
					GameObject result = this.blockPlacementInstance.blockPlacementList.Find(block => block.transform.position == position);
					
					if (result == null) {
						int blockID = int.Parse(message.blockID);
						GameObject newBlock = GameObject.Instantiate(this.blockPlacementInstance.blockSelectionList[blockID], position, Quaternion.identity);
						this.blockPlacementInstance.blockPlacementList.Add(newBlock);
					} else {
						this.blockPlacementInstance.blockPlacementList.Remove(result);
						GameObject.Destroy(result);
					}
					
				} catch (System.Exception e) {
					b.AppendLine(e.ToString());
				}
                continue;
            }
			
			if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"positionUpdate\"")) {
				try {
					PositionUpdateMessage message = UnityEngine.JsonUtility.FromJson<PositionUpdateMessage>(singleJSONStrings[i]);
					
					string[] positionRaw = message.position.Split(',');
					float x = float.Parse(positionRaw[0], System.Globalization.CultureInfo.InvariantCulture);
					float y = float.Parse(positionRaw[1], System.Globalization.CultureInfo.InvariantCulture);
					float z = float.Parse(positionRaw[2], System.Globalization.CultureInfo.InvariantCulture);
					Vector3 position = new Vector3(x, y, z);
					
					OtherPlayer moved = otherPlayerList.Find(player => player.getName().Equals(message.name));
					moved.move(position);
					
				} catch (System.Exception e) {
				}
			}

            if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"chat\""))
            {
                ServerMessage message = UnityEngine.JsonUtility.FromJson<ServerMessage>(singleJSONStrings[i]);
                b.Append(message.sender);
                b.Append(message.world ? " (world): " : " (you): ");
                b.Append(message.content);
                b.Append('\n');
                continue;
            }
			
			if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"ping\""))
            {
                int latency = System.Environment.TickCount - this.pingTimer;
				b.Append("Ping: ");
				b.Append(latency);
				b.Append(" ms\n");
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(singleJSONStrings[i], "\"type\"\\s*:\\s*\"state\""))
            {
                foreach (OtherPlayer player in otherPlayerList)
                {
                    player.destroy();
                }
                otherPlayerList.Clear();


                ServerStateMessage message = UnityEngine.JsonUtility.FromJson<ServerStateMessage>(singleJSONStrings[i]);
                string parsedPositions = message.positions;
                parsedPositions = parsedPositions.Replace("{", "");
                parsedPositions = parsedPositions.Replace("}", "");
                parsedPositions = parsedPositions.Replace("\\", "");
                parsedPositions = parsedPositions.Replace("\"", "");
                parsedPositions = parsedPositions.Replace(":", ",");

                string[] positionData = parsedPositions.Split(new char[] { ',' });

                b.AppendLine("Generating list of players and their global positions:");
                for (int j = 0; j < positionData.Length; j += 4)
                {
                    b.AppendLine("Name: " + positionData[j] + ", X:" + positionData[j + 1] + ", Y:" + positionData[j + 2] + ", Z:" + positionData[j + 3]);
                    if (positionData[j].Equals(ownName))
                    {
                        continue;
                    }
					float x = float.Parse(positionData[j + 1], System.Globalization.CultureInfo.InvariantCulture);
					float y = float.Parse(positionData[j + 2], System.Globalization.CultureInfo.InvariantCulture);
					float z = float.Parse(positionData[j + 3], System.Globalization.CultureInfo.InvariantCulture);
                    otherPlayerList.Add(new OtherPlayer(positionData[j], x, y, z, otherPlayerPrefab));
                }

                continue;
            }
        }



        
		if (!this.chatText.text.Equals(b.ToString())) {
			this.chatText.text = b.ToString();
			this.TimeLeftUntilFade = fadeOutTime;
		}
        
    }


    void sendStringToServer(string message)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message + '\n');
        chatClient.GetStream().Write(data, 0, data.Length);

    }


    void submitInput()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            if(this.inputField.text != "")
            {
                this.handleInputString(this.inputField.text);
                this.inputField.text = "";
                this.inputField.ActivateInputField();
            }
        }
    }


    void handleInputString(string input)
    {
        if (input.StartsWith("clear"))
        {
            b.Clear();
            this.chatText.text = b.ToString();
            return;
        }

        if (input.StartsWith("help") || input.StartsWith("?"))
        {
            b.AppendLine("Available Commands:\nclear - Clears chat log\nconnect <address> <port> <username> - Connect to the chat server\ngetall - Refreshes the connected client list\nmsg <receiver> - Message the specified receiver (receiver 'world' results in a global message)\ntp <x> <y> <z> - move to the specified coordinates\nttp <playername> - move to a specific player\nping - measure server latency");
            this.chatText.text = b.ToString();
            return;
        }

        if (input.StartsWith("connect"))
        {
            connectToChatServer(input);
            this.chatText.text = b.ToString();
            return;
        }

        if (input.StartsWith("msg"))
        {
            sendMessageToChatServer(input);
            this.chatText.text = b.ToString();
            return;
        }

        if (input.StartsWith("getall")) {
            getAllClientsFromChatServer();
            this.chatText.text = b.ToString();
            return;
        }

        if (input.StartsWith("tp"))
        {
            this.teleportAvatar(input.Replace("tp", "").Trim());
            this.chatText.text = b.ToString();
            return;
        }
		
		if (input.StartsWith("ttp"))
        {
            this.teleportToPlayer(input.Replace("ttp", "").Trim());
            this.chatText.text = b.ToString();
            return;
        }
		
		if (input.StartsWith("ping")) {
			this.sendPingRequestToServer();
			return;
		}

        b.Append(input);
        b.Append(": command not recognized\n");
        this.chatText.text = b.ToString();
    }


    void connectToChatServer(string input)
    {
        try
        {
            if (this.chatClient != null)
            {
                b.AppendLine("You are already connected to a server");
                return;
            }

            string[] substrings = input.Split(new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);
            if (substrings.Length < 4)
            {
                b.AppendLine("usage: connect <address> <port> <username>");
                return;
            }

            string address = substrings[1];
            System.Int32 port = System.Int32.Parse(substrings[2]);
            string username = substrings[3];

            b.AppendLine("Attempting to connect...");
            this.chatClient = new System.Net.Sockets.TcpClient(address, port);
            b.AppendLine("Connection successfully established! Sending welcome message...");

            HelloMessage hello = new HelloMessage();
            hello.type = "hello";
            hello.name = username;

            this.ownName = username;

            string json = UnityEngine.JsonUtility.ToJson(hello);
            sendStringToServer(json);

        } catch (System.Exception e)
        {
            b.AppendLine(e.ToString());
        }
    }


    void sendMessageToChatServer(string input)
    {
        if (chatClient != null)
        {
            try
            {
                string[] substrings = input.Split(new char[] { ' ' }, 3, System.StringSplitOptions.RemoveEmptyEntries);

                ClientMessage message = new ClientMessage();
                message.type = "chat";
                message.receiver = substrings[1];
                message.content = substrings[2];

                string json = UnityEngine.JsonUtility.ToJson(message);
                sendStringToServer(json);

                if (!message.receiver.Equals("world"))
                {
                    b.Append(ownName);
                    b.Append(" (");
                    b.Append(message.receiver);
                    b.Append("): ");
                    b.Append(message.content);
                    b.Append('\n');
                }

            } catch (System.Exception e)
            {
                b.AppendLine(e.ToString());
            }
        } else
        {
            b.AppendLine("Error: You are currently not connected to a server");
        }
    }
	
	void sendPingRequestToServer() {
		if (chatClient != null) {
			GenericMessage message = new GenericMessage();
			message.type = "ping";
			
			string json = UnityEngine.JsonUtility.ToJson(message);
			this.pingTimer = System.Environment.TickCount;
			sendStringToServer(json);
		}
	}


    void getAllClientsFromChatServer()
    {
        if (chatClient != null)
        {
            try
            {
                GenericMessage message = new GenericMessage();
                message.type = "getstate";

                string json = UnityEngine.JsonUtility.ToJson(message);
                sendStringToServer(json);

            }
            catch (System.Exception e)
            {
                b.AppendLine(e.ToString());
            }
        }
        else
        {
            b.AppendLine("Error: You are currently not connected to a server");
        }
    }
	
	public void sendBlockInformationToServer(string materialName, Vector3 position) {
		if (chatClient != null) {
			try {
				int blockID = 0;
				
				switch(materialName) {
					case "BlockSurfaceWood (Instance)":
						blockID = 0;
						break;
					case "BlockSurfaceTree (Instance)":
						blockID = 1;
						break;
					case "BlockSurfaceRTX (Instance)":
						blockID = 2;
						break;
					case "BlockSurfaceBurger (Instance)":
						blockID = 3;
						break;
					case "BlockSurface1337 (Instance)":
						blockID = 4;
						break;
					default:
						blockID = 0;
						break;
				}
				
				BlockActionMessage message = new BlockActionMessage();
				message.type = "blockAction";
				message.blockID = blockID.ToString();
				message.position = position.ToString().Replace("(", "").Replace(")", "");
				
				string json = UnityEngine.JsonUtility.ToJson(message);
				sendStringToServer(json);
				
			} catch (System.Exception e) {
				b.AppendLine(e.ToString());
				this.chatText.text = b.ToString();
				this.TimeLeftUntilFade = fadeOutTime;
			}
		}
	}
	
	void sendPositionUpdateToServer() {
		if (chatClient != null) {
			try {
				if (this.ownPlayer.transform.position != this.lastPosition) {
					this.lastPosition = this.ownPlayer.transform.position;
					PositionUpdateMessage message = new PositionUpdateMessage();
					message.type = "positionUpdate";
					message.name = this.ownName;
					message.position = this.ownPlayer.transform.position.ToString().Replace("(", "").Replace(")", "");
					
					string json = UnityEngine.JsonUtility.ToJson(message);
					sendStringToServer(json);
				}
			} catch (System.Exception e) {
				b.AppendLine(e.ToString());
				this.chatText.text = b.ToString();
				this.TimeLeftUntilFade = fadeOutTime;
			}
		}
	}

    void teleportAvatar(string input)
    {
        try
        {
            string[] coords = input.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (coords.Length < 3)
            {
                b.AppendLine("usage: tp <x>,<y>,<z>");
                return;
            }

            float x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
            float y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
            float z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture);

            ownPlayer.transform.position = new Vector3(x, y, z);

            b.Append("Moved player to ");
            b.Append(x.ToString());
            b.Append(" ");
            b.Append(y.ToString());
            b.Append(" ");
            b.Append(z.ToString());
            b.Append('\n');

        }
        catch (System.Exception e)
        {
            b.AppendLine(e.ToString());
        }
    }
	
	void teleportToPlayer(string input) {
		OtherPlayer otherPlayer = this.otherPlayerList.Find(x => x.getName().Equals(input));
		if (otherPlayer != null) {
			this.ownPlayer.transform.position = otherPlayer.getPosition();
		}
	}
}
