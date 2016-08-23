using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OpusNetworked : NetworkBehaviour {
    public string name = "pc";

    public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        base.OnSerialize(writer, initialState);

        writer.Write(name);

        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        base.OnDeserialize(reader, initialState);

        if (!isLocalPlayer) {
            name = reader.ReadString();
        }
    }
}
