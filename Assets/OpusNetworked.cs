using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Text;

public class OpusNetworked : NetworkBehaviour {
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