using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Text;

public class NetworkTransmitterExample : NetworkTransmitter {
    string name = "hello world";
    //inside a class, derived from network behaviour, which has the NetworkTransmitter component attached...

    public override void OnStartClient () {
        base.OnStartClient();

        Debug.Log("NetworkTransmitterExample.OnStartClient");
        if (isLocalPlayer) {
            name = System.Environment.MachineName;
            Debug.Log("NetworkTransmitterExample.OnStartClient: local name " + name);
        }
        else {
            Debug.Log("NetworkTransmitterExample.OnStartClient: non local name " + name);
        }

        //on client: listen for and handle received data
        OnDataCompletelyReceived += MyCompletelyReceivedHandler;
        OnDataFragmentReceived += MyFragmentReceivedHandler;
    }

    public override void OnStartServer () {
        base.OnStartServer();

        Debug.Log("NetworkTransmitterExample.OnStartServer");
        NetworkTransmitter networkTransmitter = GetComponent<NetworkTransmitter>();

        if (isLocalPlayer) {
            name = System.Environment.MachineName;
            Debug.Log("NetworkTransmitterExample.OnStartServer: local name " + name);
        }
        else {
            Debug.Log("NetworkTransmitterExample.OnStartServer: non local name " + name);
        }

        byte[] toBytes = Encoding.ASCII.GetBytes(name);

        //on server: transmit data. myDataToSend is an object serialized to byte array.
        StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(0, toBytes));
    }

    //on client this will be called once the complete data array has been received
    [Client]
    private void MyCompletelyReceivedHandler(int transmissionId, byte[] data) {
        //deserialize data to object and do further things with it...
        string something = Encoding.ASCII.GetString(data);
        Debug.Log("NetworkTransmitterExample.MyCompletelyReceivedHandler " + something);
    }

    //on clients this will be called every time a chunk (fragment of complete data) has been received
    [Client]
    private void MyFragmentReceivedHandler(int transmissionId, byte[] data) {
        //update a progress bar or do something else with the information
        string something = Encoding.ASCII.GetString(data);
        Debug.Log("NetworkTransmitterExample.MyFragmentReceivedHandler " + something);
    }
}
