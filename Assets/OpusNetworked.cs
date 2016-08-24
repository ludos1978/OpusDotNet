using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Text;
using POpusCodec;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NetworkIdentity))]
public class OpusNetworked : NetworkBehaviour {
    // device id of the microphone used to record sound
    public int micDeviceId = 0;

    // the audio source for the mic, used for recording sound
    AudioSource audiorecorder;
    // the audio source for playback, must not be the same as the audio recording audiosource
    [Header("add a different audioSource for the playback")]
    public AudioSource audioplayer;

    // buffers to receive and send audio data
    List<float> micBuffer;
    List<float> receiveBuffer;
    // size of a network package of opus, audio needs to be split in equal size packages and sent
    int packageSize;

    // decoder and encoder instances of opus
    OpusEncoder encoder;
    OpusDecoder decoder;

    // other values then stereo will not yet work
    POpusCodec.Enums.Channels opusChannels = POpusCodec.Enums.Channels.Stereo;
    // other values then 48000 do not work at the moment, it requires and additional conversion before sending and at receiving
    // also osx runs at 44100 i think, this causes also some hickups
    POpusCodec.Enums.SamplingRate opusSamplingRate = POpusCodec.Enums.SamplingRate.Sampling48000;

    bool recording = false;

    [Header("should the locally recorded sound be played locally as well?")]
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
        // update if we send audio recording
        recording = Input.GetKey(KeyCode.R);
        SendData();
    }
    

    // Use this for initialization
    void Start() {
        micBuffer = new List<float>();
        if (isLocalPlayer) {

            //data = new float[samples];
            encoder = new OpusEncoder(opusSamplingRate, opusChannels);
            encoder.EncoderDelay = POpusCodec.Enums.Delay.Delay20ms;
            Debug.Log("Opustest.Start: framesize: " + encoder.FrameSizePerChannel + " " + encoder.InputChannels);

            // the encoder delay has some influence on the amout of data we need to send, but it's not a multiplication of it
            packageSize = encoder.FrameSizePerChannel * (int)opusChannels;
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
            audiorecorder = GetComponent<AudioSource>();
            audiorecorder.loop = true;
            audiorecorder.clip = Microphone.Start(
                Microphone.devices[micDeviceId],
                true,
                1,
                AudioSettings.outputSampleRate);
            audiorecorder.Play();
        }

        // playback stuff
        decoder = new OpusDecoder(opusSamplingRate, opusChannels);

        receiveBuffer = new List<float>();

        // setup a playback audio clip, length is set to 1 sec (should not be used anyways)
        AudioClip myClip = AudioClip.Create("MyPlayback", (int)opusSamplingRate, (int)opusChannels, (int)opusSamplingRate, true, OnAudioRead, OnAudioSetPosition);
        audioplayer.loop = true;
        audioplayer.clip = myClip;
        audioplayer.Play();
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
        if (isLocalPlayer) {
            // take pieces of buffer and send data
            while (micBuffer.Count > packageSize) {
                byte[] encodedData = encoder.Encode (micBuffer.GetRange (0, packageSize).ToArray ());
                Debug.Log ("OpusNetworked.SendData: " + encodedData.Length);
                CmdDistributeData (encodedData);
                micBuffer.RemoveRange (0, packageSize);
            }
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

    // this is used by the second audio source, to read data from playData and play it back
    // OnAudioRead requires the AudioSource to be on the same GameObject as this script
    void OnAudioRead(float[] data) {
        Debug.LogWarning("Opustest.OnAudioRead: " + data.Length);

        int pullSize = Mathf.Min(data.Length, receiveBuffer.Count);
        float[] dataBuf = receiveBuffer.GetRange(0, pullSize).ToArray();
        dataBuf.CopyTo(data,0);
        receiveBuffer.RemoveRange(0, pullSize);

        // clear rest of data
        for (int i=pullSize; i<data.Length; i++) {
            data[i] = 0;
        }
    }
    void OnAudioSetPosition(int newPosition) {
        // we dont need the audio position at the moment
    }
}