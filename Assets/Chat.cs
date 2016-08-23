using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;


public class Chat : NetworkBehaviour {
    const short chatMsg = 1000;
    NetworkClient _client;

    [SerializeField]
    private Text chatline;
    [SerializeField]
    private SyncListString chatLog = new SyncListString();
    [SerializeField]
    private InputField input;


    public override void OnStartClient() {
        chatLog.Callback = OnChatUpdated;
    }


    private void OnChatUpdated(SyncListString.Operation op, int index) {
        chatline.text += chatLog[chatLog.Count - 1] + "\n";
    }

    public void Start() {
        _client = NetworkManager.singleton.client;
        NetworkServer.RegisterHandler(chatMsg, OnServerPostChatMessage);
        input.onEndEdit.AddListener(
                delegate { PostChatMessage(input.text); }
            );
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
    void OnServerPostChatMessage(NetworkMessage netMsg) {
        string message = netMsg.ReadMessage<StringMessage>().value;
        chatLog.Add(message);
    }

}