using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/* The NetworkManager handles all the networking at it's core to get users connected and playing with eachother.
 * By regestering a host at Unity's master-server and a server on your computer and by looking after the certain roomname you refreshed,
 * you can connect with other players and play over the internet. The NetworkManager also keeps you up to date with the latest available servers being hosted.
 * When you refresh in the menu and there is a server available, a button with the roomname will show up on the screen, and you can connect to the server.
 */

public class NetworkManager : MonoBehaviour
{
    //Sets a unique type and game name for the Unity master-server to recognize and not mix up with other games. Also sets a server to "open" or "close" so players can't join started games.
    private const string gameName = "Hexlock";
    public string roomName = "RoomName";
    private const string serverOpen = "Open";
    private const string serverClosed = "Closed";

    //Max connections and host port.
    private const int maxPlayers = 4;
    private const int listenPort = 25000;

    //If the client is looking for a new host in the menu or not.
    private bool isRefreshingHostList = false;
    //Stores all the available hots.
    [HideInInspector]
    public HostData[] hostList;
    NetworkView networkView;

    public GameObject serverButton; //Stores a serverbutton prefab.
    private const int serverButtonOffset = 30; //Set a current offset from other buttons so overlaping won't be a problem.
    public GameObject[] serverButtonList; //Store all the available serverbuttons.
    public GameObject ClientLobby; //Store the ClientLobby from the Menu Scene.
    public GameObject Hostlobby; //Store the HostLobby from the Menu Scene.

    public GameObject hostButton; //Stores the hostButton from the Menu Scene.
    public GameObject[] mapButtons; //Store all the map buttons from the Menu Scene.
    public GameObject choseMapText; // Stores the choose map text from the Menu Scene
    public GameObject currentMapText; //Stored the currentMapText from the Menu Scene.

    void Start()
    {
        networkView = GetComponent<NetworkView>(); //Get the network view component.
    }

    //Get and Set the RoomName.
    public string RoomName
    { 
        get { return roomName; }
        set { roomName = value; }
    }

    public void FindServer()
    {
        UpdateServerInfo();

        //Refrehes the list of servers being hosted.
        RefreshHostList();
           
        //Finds new hosts and draws additional buttons for every server found.
        if (hostList != null)
        {
            for (int i = 0; i < hostList.Length; i++)
            {
                if (hostList[i].comment == serverOpen)
                {
                    serverButtonList = new GameObject[hostList.Length];
                    serverButtonList[i] = Instantiate(serverButton, new Vector3(85, 400, 0), serverButton.transform.rotation) as GameObject;
                    serverButtonList[i].transform.SetParent(ClientLobby.transform);
                    serverButtonList[i].transform.FindChild("Text").GetComponent<Text>().text = hostList[i].gameName;
                    serverButtonList[i].GetComponent<Button>().onClick.AddListener(JoinServer);
                    print(hostList[i].gameName);
                }
            }
            MasterServer.ClearHostList();
        }
    }

    //Initalizes a new server with a max amount of connections(players), on a certain port with the public ip.
    //Then sends the unique typeName and gameName to the Unity master-server.
    public void StartServer()
    {
        Network.InitializeSecurity();
        Network.InitializeServer(maxPlayers, listenPort, !Network.HavePublicAddress());
        MasterServer.RegisterHost(gameName, roomName, serverOpen);
    }
    //Disconnect and unregister the hosted server.
    public void CloseServer()
    {
        Network.Disconnect();
        MasterServer.UnregisterHost();
        Debug.Log("Server closed");
    }

    //When the server has been loaded on either the Server-side or the Client-side.
    //The Hostlobby will be shown.
    void OnServerInitialized()
    {
        Hostlobby.SetActive(true);
    }

    void Update()
    {
        //Update the list of servers after the client has found all available hosts.
        if (isRefreshingHostList && MasterServer.PollHostList().Length > 0)
        {
            isRefreshingHostList = false;
            hostList = MasterServer.PollHostList();
        }
    }

    public void RefreshHostList()
    {
        //Request the list from the Unity master-server of available hosts.
        if (!isRefreshingHostList)
        {
            MasterServer.ClearHostList();

            isRefreshingHostList = true;
            MasterServer.RequestHostList(gameName);
        }
    }

    //Connect to the chosen server.
    public void JoinServer()
    {
        for (int i = 0; i < hostList.Length; i++)
        {
            if (hostList[i].gameName == serverButtonList[i].transform.FindChild("Text").GetComponent<Text>().text)
            {
                Network.Connect(hostList[i]);
            }
        }
        ClientLobby.SetActive(false);
    }
    //Loads the game map for all players.
    [RPC]
    public void LoadLevel(String sceneName)
    {
        Application.LoadLevel(sceneName);
    }

    //When a client enters the server, spawn the player.
    void OnConnectedToServer()
    {
        Debug.Log("Is connected");
        ActivateLobby();
    }

    //Activates and de-activates certain objects depending on if you're a client or a server-host.
    private void ActivateLobby()
    {
        Hostlobby.SetActive(true);
        hostButton.SetActive(false);
        mapButtons[0].SetActive(false);
        mapButtons[1].SetActive(false);
        choseMapText.SetActive(false);
        currentMapText.SetActive(false);
    }
    //Calls on the "LoadLevel" RPC-function to the current map selected for all clients and the host.
    //Also re-registers the server as a closed servers to prevent new clients from joining mid-game.
    public void StartMap()
    {
        networkView.RPC("LoadLevel", RPCMode.AllBuffered, UImanager.currentMap);
        MasterServer.UnregisterHost();
        MasterServer.RegisterHost(gameName, roomName, serverClosed);
    }
    //Destorys old serverbuttons upon refresh.
    private void UpdateServerInfo()
    {
        Debug.Log(serverButtonList.Length);

        foreach (GameObject button in serverButtonList)
        {
            Destroy(button.gameObject);
        }
    }
}
