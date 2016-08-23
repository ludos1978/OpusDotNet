using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Text;

public class OpusNetworked : NetworkBehaviour {
    /*[SyncVar]
    private string MySyncString; // This string will sync across the network. It's value on the server will overide the value on all clients. Therefore this variable can only be changed only by the server, but it'll be the same on all clients

    [SyncVar]
    private int MySyncInt; // This int will sync across the network. It's value on the server will overide the value on all clients. Therefore this variable can only be changed only by the server, but it'll be the same on all clients

    [SyncVar]
    private float MySyncFloat;// This float will sync across the network. It's value on the server will overide the value on all clients. Therefore this variable can only be changed only by the server, but it'll be the same on all clients

    [SyncVar]
    private bool MySyncBool;// This bool will sync across the network. It's value on the server will overide the value on all clients. Therefore this variable can only be changed only by the server, but it'll be the same on all clients

    [SyncVar(hook = "MyHookFunction")]
    private string MySyncStringWithHook; // This is a hook, and the function "MyHookFunction" will be called whenever the variable changes.


    //This function will be called when "MySyncStringWithHook" changes.
    void MyHookFunction(string hook) {
        Debug.Log("MySyncStringWithHook changed");
    }*/

    public override void OnStartClient() {
        Debug.Log("OpusNetworked.OnStartClient: " + System.Environment.MachineName);
        base.OnStartClient();
            
        foreach (NetworkClient c in NetworkClient.allClients) {
            NetworkServer.SpawnWithClientAuthority(gameObject, c.connection);
        }
    }
    public override void OnStartServer() {
        Debug.Log("OpusNetworked.OnStartServer: " + System.Environment.MachineName);
        base.OnStartServer();
    }
    public override void OnStartLocalPlayer() {
        Debug.Log("OpusNetworked.OnStartLocalPlayer: " + System.Environment.MachineName);
        base.OnStartLocalPlayer();

    }
    public override void OnStartAuthority() {
        Debug.Log("OpusNetworked.OnStartAuthority: " + System.Environment.MachineName);
        base.OnStartAuthority();
    }
    public override void OnStopAuthority() {
        Debug.Log("OpusNetworked.OnStopAuthority: " + System.Environment.MachineName);
        base.OnStopAuthority();
    }
    public override void PreStartClient() {
        Debug.Log("OpusNetworked.PreStartClient: " + System.Environment.MachineName);
        base.PreStartClient();
    }

    [ClientCallback]
    void Update () {
        //if ((Time.frameCount % 30) == 0) {
        if (Input.GetKeyDown(KeyCode.S)) {
            string value = System.Environment.MachineName + " - " + (Time.frameCount / 30).ToString();
            Debug.Log("Update: " + System.Environment.MachineName + " send: " + value);
            Cmd_MyCommmand(System.Environment.MachineName + " - " + value);
        }
    }

    // how to set up a RPC
    [ClientRpc]
    void Rpc_MyRPC(string value) {
        Debug.Log("Rpc_MyRPC: " + System.Environment.MachineName + " get: " + value);
    }

    // How to set up a Command
    [Command]
    void Cmd_MyCommmand(string value)  {
        Debug.Log("Cmd_MyCommmand: " + System.Environment.MachineName + " get: " + value);
        Rpc_MyRPC(value);
    }


    /*const short chatMsg = 1001;
    NetworkClient _client;

    [SerializeField]
    private Text chatline;
    [SerializeField]
    private SyncListStruct<byte> opusData = new SyncListStruct<byte>();
    [SerializeField]
    private InputField input;

    public void Awake () {
        string name = "";
        if (isLocalPlayer) {
            name = System.Environment.MachineName;
            Debug.Log("OpusNetworked.Awake: local name " + name);
        }
        else {
            Debug.Log("OpusNetworked.Awake: non local name " + name);
        }
    }

    public override void OnStartClient() {
        opusData.Callback = OnOpusUpdated;
    }

    public void Start() {
        _client = NetworkManager.singleton.client;
        NetworkServer.RegisterHandler(opusData, OnServerPostOpusMessage);
        input.onEndEdit.AddListener(delegate { PostChatMessage(input.text); });
    }

    [Client]
    public void PostChatMessage(string message) {
        if (message.Length == 0) return;
        var msg = new StringMessage(message);
        _client.Send(chatMsg, msg);

        input.text = "";
        input.ActivateInputField();
        input.Select();
    }

    [Server]
    void OnServerPostOpusMessage(NetworkMessage netMsg) {
        //string message = netMsg.ReadMessage<StringMessage>().value;
        netMessage = netMsg.ReadMessage<byte>()
        byte[] toBytes = Encoding.ASCII.GetBytes();
        foreach (byte b in toBytes) { 
            opusData.Add(b);
        }
    }

    private void OnOpusUpdated(SyncList<byte>.Operation op, int index) {
        //chatline.text += chatLog[chatLog.Count - 1] + "\n";
        byte b = opusData[opusData.Count - 1];
    }*/
}