using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OpusNetworked : NetworkBehaviour {
    public string name = "something";

    public void Start () {
        if (isLocalPlayer) {
            name = System.Environment.MachineName;
            Debug.Log ("OpusNetworked.Start: local name " + name);
        } else {
            Debug.Log ("OpusNetworked.Start: non local name " + name);
        }

    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        base.OnSerialize(writer, initialState);

        writer.Write(name);

        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        base.OnDeserialize(reader, initialState);

        if (!isLocalPlayer) {
            name = reader.ReadString() + "-nonlocal";
        }
    }
}
