using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// The UI-Manager uses functions as listeners in the Unity UI Eventsystem to execute code with buttonpresses and such.

public class UImanager : MonoBehaviour 
{
    // Variables
    public NetworkManager netmanager;
    public Text serverText;
    public InputField serverInputField;
    public InputField nameInputField;
    public Camera maincamera;
    private NetworkView network;
    public string playerName;
    public static string currentMap = "Map";
    public Text currentMapText;

    void Start()
    {
        network = maincamera.GetComponent<NetworkView>();
    }

    // Sets the server name to the input in the menu's textfield
    public void ChangeServerName()
    {
        serverText.text = serverInputField.text;
        netmanager.roomName = serverText.text;
    }
    //Changes the player name
    public void ChangePlayerName()
    {
        playerName = nameInputField.text;
    }

    // Exits the game
    public void ExitApplication()
    {
        Debug.Log("Application closed");
        Application.Quit();
    }
    //Loads the Menu scene
    public void LoadMenu()
    {
        MasterServer.UnregisterHost();
        Application.LoadLevel("Menu");
        Network.Disconnect();
    }
    //Restarts the game
    public void RestartGame()
    {
        netmanager.StartMap();
    }
    //Changes the current map to "Map"
    public void ChangeMap1()
    {
        currentMap = "Map";
        currentMapText.text = currentMap;
    }
    //Changes the current map to "Map2"
    public void ChangeMap2()
    {
        currentMap = "Map2";
        currentMapText.text = currentMap;
    }
}
