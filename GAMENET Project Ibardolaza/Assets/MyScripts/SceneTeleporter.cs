using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : NetworkBehaviour
{
    [Scene, SerializeField]
    private string sceneToTeleportTo;

    [SerializeField]
    private string spawnName;

    private void OnTriggerEnter(Collider collision)
    {
        if (!isServer) return;

        if (collision.GetComponent<PlayerMovement>())
        {
            Debug.Log("yes");
            StartCoroutine(SendPlayer(collision.gameObject));
        }
    }

    [ServerCallback]
    IEnumerator SendPlayer(GameObject player)
    {
        NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
        if (identity == null)
            yield break;

        NetworkConnectionToClient conn = identity.connectionToClient;
        if (conn == null)
            yield break;

        conn.Send(new SceneMessage { sceneName = this.gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });
        NetworkServer.RemovePlayerForConnection(conn, RemovePlayerOptions.KeepActive);

        conn.Send(new SceneMessage { sceneName = sceneToTeleportTo, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
        SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(sceneToTeleportTo));

        NetworkStartPosition[] positions = GameObject.FindObjectsOfType<NetworkStartPosition>();
        Vector3 position = Vector3.zero;
        foreach(NetworkStartPosition pos in positions)
        {
            if(pos.gameObject.scene.path == sceneToTeleportTo)
            {
                position = pos.gameObject.transform.position;
                break;
            }
        }    

        player.transform.position = position;

        yield return new WaitForEndOfFrame();
        NetworkServer.AddPlayerForConnection(conn, player);
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
    }
}
