using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
/* The GameManager handles the gamesession locally for every client, eventhough it processes data gathered over the network.
 * The main purpose is to handle the core gameplay data such as spawnpoints and when players are killed etc.
 */
    // Variables
    public GameObject player; //Get the local player object
    [HideInInspector]
    public static int killedPlayers; //How many players has been killed.
    private NetworkView network;
    public Material magma;

    float textureMoveX; // These will move the textures of the material
    float textureMoveY;

    [HideInInspector]
    public static int playersConnected; //How many players are connected to the server.
    public Vector3[] spawnPoints = new Vector3[4]; //Store down the available spawnpoints.
    public GameObject continueMenu; //Store the continue menu from the scene.

	// Use this for initialization
	void Start () 
    {
        killedPlayers = 0; //Null the value to avoid bugs if the scene changes.
        playersConnected = 0; //Null the value to avoid bugs if the scene changes.
        network = GetComponent<NetworkView>();
        Network.Instantiate(player, spawnPoints[Random.Range(0,4)], new Quaternion(), 1); // Spawns the player object when the scene is loaded.
        playersConnected++; //Everytime a GameManager script is instantiated, add a connected player.
        textureMoveX = 1f;
        textureMoveY = 1f;
	}

    // Update is called once per frame
    void Update() 
    {
        //Moves the lava texture.
        textureMoveY += 0.001f;
        textureMoveX += 0.001f;
        magma.SetTextureOffset("_MainTex", new Vector2(textureMoveX / 2, textureMoveY / 2));
        magma.SetTextureOffset("_DetailAlbedoMap", new Vector2(textureMoveX,textureMoveY));

        //If there is only one player alive on the server, show the continue menu and cursor so the host can decide to restart the game or go back to the menu.
        if (GameManager.killedPlayers == Network.connections.Length && Time.timeSinceLevelLoad >= 10f && Network.isServer)
        {
            continueMenu.SetActive(true);
            Cursor.visible = true;
        }
	}
}
