using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OpusNetworked : NetworkBehaviour {
    public string name = "something";

    public override void OnStartLocalPlayer () {
        if (isLocalPlayer) {
            name = System.Environment.MachineName;
            Debug.Log ("OpusNetworked.Start: local name " + name);
        } else {
            Debug.Log ("OpusNetworked.Start: non local name " + name);
        }

    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        string _name = "";
        if (stream.isWriting) {
            //_name = name;
            int length = name.Length;
            stream.Serialize(ref length);
            char[] chars = name.ToCharArray ();
            for (int i = 0; i < length; i++) {
                stream.Serialize(ref chars [i]);
            }
        } else {
            int length = 0;
            stream.Serialize(ref length);
            char[] chars = new char[length];
            for (int i = 0; i < length; i++) {
                stream.Serialize (ref chars [i]);
            }
            name = chars.ToString ();
        }
    }

    public void Update () {
        //SetDirtyBit(
    }

    /*public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        base.OnSerialize(writer, initialState);

        Debug.Log ("OpusNetworked.OnSerialize: "+name);
        writer.Write(name);

        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        base.OnDeserialize(reader, initialState);

        name = reader.ReadString ();
        Debug.Log ("OpusNetworked.OnDeserialize: "+name);
        if (!isLocalPlayer) {
            name += "-nonlocal";
        }
    }*/
}
