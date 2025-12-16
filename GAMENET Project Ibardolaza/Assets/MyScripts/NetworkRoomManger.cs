using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class NetworkRoomManger : NetworkRoomManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.

    [Scene, SerializeField]
    string firstSceneToLoad;

    List<string> sceneNames = new List<string>();
    bool isSubsceneLoaded = false;
    bool isInTransition = false;
    public static new NetworkRoomManger singleton => (NetworkRoomManger)NetworkRoomManager.singleton;

    public Action<HashSet<MyNetworkRoomPlayer>> OnAddPlayer;
    #region Server Callbacks

    /// <summary>
    /// This is called on the server when the server is started - including when a host is started.
    /// </summary>
    public override void OnRoomStartServer() 
    {
        //SceneManager.LoadScene(0, LoadSceneMode.Additive)
        for (int i = 3; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            sceneNames.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if(sceneName == GameplayScene)
        {
            StartCoroutine(ServerLoadSubscenes());
        }
    }

    IEnumerator ServerLoadSubscenes()
    {
        foreach (string sceneName in sceneNames)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics2D
            });
        }

        isSubsceneLoaded = true;
    }

    public override void OnClientSceneChanged()
    {
        if(!isInTransition)
            base.OnClientSceneChanged();
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if(sceneOperation == SceneOperation.LoadAdditive)
        {
            StartCoroutine(LoadAddiveScene(newSceneName));
        }

        if(sceneOperation == SceneOperation.UnloadAdditive)
        {
            StartCoroutine(UnloadAdditiveScene(newSceneName));
        }
    }

    IEnumerator LoadAddiveScene(string sceneName)
    {
        isInTransition = true;

        if (mode == NetworkManagerMode.ClientOnly)
        {
            loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
            {
                yield return null;
            }

            NetworkClient.isLoadingScene = false;
            isInTransition = false;

            OnRoomClientSceneChanged();
        }
    }

    IEnumerator UnloadAdditiveScene(string sceneName)
    {
        isInTransition = true;

        if (mode == NetworkManagerMode.ClientOnly)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return Resources.UnloadUnusedAssets(); 
        }

        NetworkClient.isLoadingScene = false;
        isInTransition = false;
        OnRoomClientSceneChanged();
        //OnClientSceneChanged();
    }

    // JUST IN CASE: Deleted OnRoomStopServer, OnRoomStartHost, OnRoomStopHost, OnRoomServerConnect, OnRoomServerDisconnect, OnRoomServerSceneChanged as per the lesson.


    /// <summary>
    /// This allows customization of the creation of the room-player object on the server.
    /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <returns>The new room-player object.</returns>
    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        var obj = base.OnRoomServerCreateRoomPlayer(conn);

        var players = roomSlots
            .Cast<MyNetworkRoomPlayer>()
            .ToHashSet();

        OnAddPlayer?.Invoke(players);

        return obj;
        //return base.OnRoomServerCreateRoomPlayer(conn);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        MyNetworkRoomPlayer myRoomPlayer = roomPlayer.GetComponent<MyNetworkRoomPlayer>();
        myRoomPlayer.RpcDisableCanvas();
         
        return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
    /// <para>See OnRoomServerCreateGamePlayer to customize the player object for the initial GamePlay scene.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnRoomServerAddPlayer(conn);
    }

    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        //return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);

        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, ReplacePlayerOptions.KeepAuthority);
        StartCoroutine(AddPlayerToGameScene(conn));
        return false;
    }

    IEnumerator AddPlayerToGameScene(NetworkConnectionToClient conn)
    {
        while (!isSubsceneLoaded)
            yield return null;

        NetworkIdentity[] allObjectsWithNetworkIdentity = FindObjectsOfType<NetworkIdentity>();
        foreach(NetworkIdentity identity in allObjectsWithNetworkIdentity)
        {
            identity.enabled = true;
        }

        GameObject playerObject = conn.identity.gameObject;
        NetworkServer.RemovePlayerForConnection(conn, RemovePlayerOptions.KeepActive);

        conn.Send(new SceneMessage { sceneName = firstSceneToLoad, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
       
        SceneManager.MoveGameObjectToScene(playerObject, SceneManager.GetSceneByPath(firstSceneToLoad));
        NetworkStartPosition[] startingPositions = GameObject.FindObjectsOfType<NetworkStartPosition>();
        playerObject.transform.position = startingPositions.SingleOrDefault(pos => pos.gameObject.scene.path == firstSceneToLoad).transform.position;
        yield return new WaitForEndOfFrame();
        NetworkServer.AddPlayerForConnection(conn, playerObject);
        playerObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    /// <summary>
    /// This is called on server from NetworkRoomPlayer.CmdChangeReadyState when client indicates change in Ready status.
    /// </summary>
    public override void ReadyStatusChanged()
    {
        base.ReadyStatusChanged();
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        //base.OnRoomServerPlayersReady();
    }

    /// <summary>
    /// This is called on the server when CheckReadyToBegin finds that players are not ready
    /// <para>May be called multiple times while not ready players are joining</para>
    /// </summary>
    public override void OnRoomServerPlayersNotReady() { }

    #endregion

    #region Client Callbacks

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client enters the room.
    /// </summary>
    public override void OnRoomClientEnter() { }

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client exits the room.
    /// </summary>
    public override void OnRoomClientExit() { }

    /// <summary>
    /// This is called on the client when it connects to server.
    /// </summary>
    public override void OnRoomClientConnect() { }

    /// <summary>
    /// This is called on the client when disconnected from a server.
    /// </summary>
    public override void OnRoomClientDisconnect() { }

    /// <summary>
    /// This is called on the client when a client is started.
    /// </summary>
    public override void OnRoomStartClient() { }

    /// <summary>
    /// This is called on the client when the client stops.
    /// </summary>
    public override void OnRoomStopClient() { }

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    public override void OnRoomClientSceneChanged() { }

    #endregion

    #region Optional UI

    public override void OnGUI()
    {
        base.OnGUI();
    }

    #endregion
}
