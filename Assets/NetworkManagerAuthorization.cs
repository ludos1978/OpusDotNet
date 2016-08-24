using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManagerAuthorization : NetworkManager {

    public GameObject[] authorizeObjects;

    public override void OnServerConnect(NetworkConnection conn) {
        //Debug.Log("NetworkManagerAuthorization.OnServerConnect: give permission " + conn.address);
        base.OnServerConnect(conn);

        //NetworkServer.(gameObject, c.connection);
        foreach (GameObject go in authorizeObjects) {
            go.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        base.OnServerDisconnect(conn);
    }
}
