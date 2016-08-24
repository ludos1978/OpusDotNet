using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Text;
using POpusCodec;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
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

    //[ServerCallback]
    /*public override void OnStartClient() {
        Debug.Log("OpusNetworked.OnStartClient: " + System.Environment.MachineName);
        base.OnStartClient();

    }
    public override void OnStartServer() {
        Debug.Log("OpusNetworked.OnStartServer: " + System.Environment.MachineName);
        base.OnStartServer();
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
    }*/

    public int micDeviceId = 0;

    // the audio source for the mic, used for recording sound
    [Header("add a audioSource for the microphone recording")]
    public AudioSource src;
    // the audio source for playback, must not be the same as the audio recording audiosource
    [Header("add a different audioSource for the playback")]
    public AudioSource player;

    int playWritePosition = 0;

    float[] playData;

    List<float> micBuffer;
    int packageSize;
    List<float> receiveBuffer;
    OpusEncoder encoder;
    OpusDecoder decoder;

    //int samplerate = 48000;
    int playPosition = 0;

    POpusCodec.Enums.Channels opusChannels = POpusCodec.Enums.Channels.Stereo;
    int audioChannels = 2;
    POpusCodec.Enums.SamplingRate opusSamplingRate = POpusCodec.Enums.SamplingRate.Sampling48000;
    int audioSamplingRate = 48000;

    // time of the buffer which plays the sound
    int playDataMaxLength = 2; // seconds

    bool recording = false;
    [SyncVar]
    public bool playLocally = false;

    public override void OnStartLocalPlayer() {
        Debug.Log("OpusNetworked.OnStartLocalPlayer: " + System.Environment.MachineName);
        base.OnStartLocalPlayer();

        string value = System.Environment.MachineName; // + " - " + (Time.frameCount / 30).ToString();
        Debug.Log("Update: " + System.Environment.MachineName + " send: " + value);
    }

    [ClientCallback]
    void Update () {
        //if ((Time.frameCount % 30) == 0) {
        if (Input.GetKeyDown(KeyCode.S)) {
            Cmd_MyCommmand(System.Environment.MachineName);
        }

        // update if we send audio recording
        recording = Input.GetKey(KeyCode.R);
        SendData();
    }

    // How to set up a Command
    [Command]
    void Cmd_MyCommmand(string value)  {
        Debug.Log("Cmd_MyCommmand: " + System.Environment.MachineName + " get: " + value);
        Rpc_MyRPC(value);
    }

    // how to set up a RPC
    [ClientRpc]
    void Rpc_MyRPC(string value) {
        if (!isLocalPlayer) {
            Debug.Log("Rpc_MyRPC: " + System.Environment.MachineName + " get: " + value);
        }
    }


    // Use this for initialization
    void Start() {
        if (isLocalPlayer) {
            micBuffer = new List<float>();

            //data = new float[samples];
            playData = new float[playDataMaxLength * audioSamplingRate];
            encoder = new OpusEncoder(opusSamplingRate, opusChannels);
            //encoder.InputChannels = POpusCodec.Enums.Channels.Stereo;
            //encoder.InputSamplingRate = POpusCodec.Enums.SamplingRate.Sampling48000;
            encoder.EncoderDelay = POpusCodec.Enums.Delay.Delay20ms;
            Debug.Log("Opustest.Start: framesize: " + encoder.FrameSizePerChannel + " " + encoder.InputChannels);

            // the encoder delay has some influence on the amout of data we need to send, but it's not a multiplication of it
            packageSize = encoder.FrameSizePerChannel * audioChannels;
            //dataSendBuffer = new float[packageSize];

            //encoder.ForceChannels = POpusCodec.Enums.ForceChannels.NoForce;
            //encoder.SignalHint = POpusCodec.Enums.SignalHint.Auto;
            //encoder.Bitrate = samplerate;
            //encoder.Complexity = POpusCodec.Enums.Complexity.Complexity0;
            //encoder.DtxEnabled = true;
            //encoder.MaxBandwidth = POpusCodec.Enums.Bandwidth.Fullband;
            //encoder.ExpectedPacketLossPercentage = 0;
            //encoder.UseInbandFEC = true;
            //encoder.UseUnconstrainedVBR = true;

            // setup a microphone audio recording
            Debug.Log("Opustest.Start: setup mic with " + Microphone.devices[micDeviceId] + " " + AudioSettings.outputSampleRate);
            src = GetComponent<AudioSource>();
            src.loop = true;
            src.clip = Microphone.Start(
                Microphone.devices[micDeviceId],
                true,
                1,
                AudioSettings.outputSampleRate);
            src.volume = 1;
            src.Play();
        }

        // playback stuff
        decoder = new OpusDecoder(opusSamplingRate, opusChannels);

        receiveBuffer = new List<float>();

        // setup a playback audio clip
        AudioClip myClip = AudioClip.Create("MyPlayback", playDataMaxLength * audioSamplingRate, audioChannels, audioSamplingRate, true, OnAudioRead, OnAudioSetPosition);
        player.clip = myClip;
        player.Play();
    }

    [ClientCallback]
    // this handles the microphone data, it sends the data and deletes any further audio output
    void OnAudioFilterRead(float[] data, int channels) {
        if (recording) {
            // add mic data to buffer
            micBuffer.AddRange(data);
            Debug.Log("OpusNetworked.OnAudioFilterRead: " + data.Length);
        }

        // clear array so we dont output any sound
        for (int i = 0; i < data.Length; i++) {
            data[i] = 0;
        }
    }

    [ClientCallback]
    void SendData () {
        // take pieces of buffer and send data
        while (micBuffer.Count > packageSize) {
            byte[] encodedData = encoder.Encode(micBuffer.GetRange(0, packageSize).ToArray());
            Debug.Log("OpusNetworked.SendData: " + encodedData.Length);
            CmdDistributeData(encodedData);
            micBuffer.RemoveRange(0, packageSize);
        }
    }


    [Command]
    void CmdDistributeData(byte[] encodedData) {
        Debug.Log("OpusNetworked.CmdDistributeData: " + encodedData.Length);
        RpcReceiveData(encodedData);
    }

    [ClientRpc]
    void RpcReceiveData(byte[] encodedData) {
        if (isLocalPlayer && !playLocally) {
            Debug.Log("OpusNetworked.RpcReceiveData: discard! " + encodedData.Length);
            return;
        }

        Debug.Log("OpusNetworked.RpcReceiveData: add to buffer " + encodedData.Length);
        // the data would need to be sent over the network, we just decode it now to test the result
        receiveBuffer.AddRange(decoder.DecodePacketFloat(encodedData));
    }

    /*void AddToPlayBuffer() {
        float[] decodedData = receiveBuffer.GetRange(0, receiveBuffer.Count).ToArray();
        receiveBuffer.RemoveRange(0, receiveBuffer.Count);

        // write decodedData into playData
        int startPos = playWritePosition % (playDataMaxLength * audioSamplingRate);
        // more data than fits into the play buffer (overflow)
        if ((startPos + decodedData.Length) > playData.Length) {
            int maxLength = Mathf.Min(playData.Length, (playData.Length - startPos));
            int remainder = decodedData.Length - maxLength;
            //Debug.Log("split write: " + decodedData.Length + " " + startPos + " " + playData.Length + " " + "0" + " " + (maxLength - 1));
            Array.Copy(decodedData, 0, playData, startPos, maxLength);
            //Debug.Log("split write: " + decodedData.Length + " " + "0" + " " + playData.Length + " " + maxLength + " " + remainder);
            Array.Copy(decodedData, maxLength, playData, 0, remainder);
        }
        // write all data
        else {
            decodedData.CopyTo(playData, playWritePosition % (playDataMaxLength * audioSamplingRate));
        }

        // testing to set the play position
        playPosition = playWritePosition;

        playWritePosition = (playWritePosition + decodedData.Length) % (playDataMaxLength * audioSamplingRate);

        if (false) {
            // input & ouput volume test
            float outA = 0;
            foreach (float sample in decodedData) {
                outA += Mathf.Abs(sample);
            }
            float outAv = outA / decodedData.Length;
            Debug.Log("Opustest.SendData: output volume " + outAv.ToString("0.000"));
        }
    }*/


    // this is used by the second audio source, to read data from playData and play it back
    void OnAudioRead(float[] data) {
        Debug.LogWarning("Opustest.OnAudioRead: " + data.Length + " " + playPosition);

        int pullSize = Mathf.Min(data.Length, receiveBuffer.Count);
        float[] dataBuf = receiveBuffer.GetRange(0, pullSize).ToArray();
        dataBuf.CopyTo(data,0);
        receiveBuffer.RemoveRange(0, pullSize);

        // clear rest of data
        for (int i=pullSize; i<data.Length; i++) {
            data[i] = 0;
        }

        /*// data overflow
        if ((playPosition + data.Length) > playData.Length) {
            int maxSize = playData.Length - playPosition;
            Array.Copy(playData, playPosition, data, 0, maxSize);
            Array.Copy(playData, 0, data, maxSize, data.Length - maxSize);
        }
        // we can just copy it over
        else {
            Array.Copy(playData, playPosition, data, 0, data.Length);
        }
        playPosition = (playPosition + data.Length) % (playDataMaxLength * audioSamplingRate);*/
    }
    void OnAudioSetPosition(int newPosition) {
        playPosition = newPosition;
    }
}