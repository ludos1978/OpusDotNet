using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkTransmitterExample : NetworkBehaviour {
     /*//inside a class, derived from network behaviour, which has the NetworkTransmitter component attached...
     ...
     //on client and server
     NetworkTransmitter networkTransmitter = GetComponent<NetworkTransmitter>();
     ...
     //on client: listen for and handle received data
     networkTransmitter.OnDataCompletelyReceived += MyCompletelyReceivedHandler;
     networkTransmitter.OnDataFragmentReceived += MyFragmentReceivedHandler;
 
     ...

     //on server: transmit data. myDataToSend is an object serialized to byte array.
     StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(0, myDataToSend))
     ...
     //on client this will be called once the complete data array has been received
     [Client]
    private void MyCompletelyReceivedHandler(int transmissionId, byte[] data) {
        //deserialize data to object and do further things with it...
    }
    //on clients this will be called every time a chunk (fragment of complete data) has been received
    [Client]
    private void MyFragmentReceivedHandler(int transmissionId, byte[] data) {
        //update a progress bar or do something else with the information
    }*/

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
