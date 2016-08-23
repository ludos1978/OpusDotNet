using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class NetworkTransmitterExample : NetworkBehaviour {
    string name = "";
    //inside a class, derived from network behaviour, which has the NetworkTransmitter component attached...
    void Awake () {
        if (isLocalPlayer) {
            name = System.Environment.MachineName;
            Debug.Log("OpusNetworked.Start: local name " + name);
        }
        else {
            Debug.Log("OpusNetworked.Start: non local name " + name);
        }

        //on client and server
        NetworkTransmitter networkTransmitter = GetComponent<NetworkTransmitter>();

        if (isClient) {
            //on client: listen for and handle received data
            networkTransmitter.OnDataCompletelyReceived += MyCompletelyReceivedHandler;
            networkTransmitter.OnDataFragmentReceived += MyFragmentReceivedHandler;
        } else {
            byte[] toBytes = Encoding.ASCII.GetBytes(name);
            //on server: transmit data. myDataToSend is an object serialized to byte array.
            StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(0, toBytes));
        }
    }

     
    //on client this will be called once the complete data array has been received
    [Client]
    private void MyCompletelyReceivedHandler(int transmissionId, byte[] data) {
        //deserialize data to object and do further things with it...
        string something = Encoding.ASCII.GetString(data);
        Debug.Log("MyCompletelyReceivedHandler " + something);
    }

    //on clients this will be called every time a chunk (fragment of complete data) has been received
    [Client]
    private void MyFragmentReceivedHandler(int transmissionId, byte[] data) {
        //update a progress bar or do something else with the information
        string something = Encoding.ASCII.GetString(data);
        Debug.Log("MyFragmentReceivedHandler " + something);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
