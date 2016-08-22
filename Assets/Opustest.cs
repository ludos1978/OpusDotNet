using UnityEngine;
using System.Collections;
using POpusCodec;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Opustest : MonoBehaviour {

    public int micDeviceId = 0;

    // the audio source for the mic, used for recording sound
    AudioSource src;
    // the audio source for playback, must not be the same as the audio recording audiosource
    [Header("add a different audioSource for the playback (not the one on this gameobject)")]
    public AudioSource player;

    int playWritePosition = 0;

    float[] playData;
    
    List<float> micDataQueue;
    float[] dataSendBuffer;
    byte[] encodedData;
    float[] decodedData;
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

    // Use this for initialization
    void Start () {
        micDataQueue = new List<float>();
        //AudioSettings.SetDSPBufferSize(960, 2);

        //data = new float[samples];
        playData = new float[playDataMaxLength * audioSamplingRate];
        encoder = new OpusEncoder(opusSamplingRate, opusChannels);
        //encoder.InputChannels = POpusCodec.Enums.Channels.Stereo;
        //encoder.InputSamplingRate = POpusCodec.Enums.SamplingRate.Sampling48000;
        encoder.EncoderDelay = POpusCodec.Enums.Delay.Delay20ms;
        Debug.Log("Opustest.Start: framesize: " + encoder.FrameSizePerChannel + " " + encoder.InputChannels);

        // the encoder delay has some influence on the amout of data we need to send, but it's not a multiplication of it
        dataSendBuffer = new float[encoder.FrameSizePerChannel * audioChannels];

        //encoder.ForceChannels = POpusCodec.Enums.ForceChannels.NoForce;
        //encoder.SignalHint = POpusCodec.Enums.SignalHint.Auto;
        //encoder.Bitrate = samplerate;
        //encoder.Complexity = POpusCodec.Enums.Complexity.Complexity0;
        //encoder.DtxEnabled = true;
        //encoder.MaxBandwidth = POpusCodec.Enums.Bandwidth.Fullband;
        //encoder.ExpectedPacketLossPercentage = 0;
        //encoder.UseInbandFEC = true;
        //encoder.UseUnconstrainedVBR = true;

        decoder = new OpusDecoder(opusSamplingRate, opusChannels);


        // setup a playback audio clip
        AudioClip myClip = AudioClip.Create("MyPlayback", playDataMaxLength * audioSamplingRate, audioChannels, audioSamplingRate, true, OnAudioRead, OnAudioSetPosition);
        player.clip = myClip;
        player.Play();


        // setup a microphone audio recording
        Debug.Log("Opustest.Start: setup mic with " + Microphone.devices[micDeviceId] + " " + AudioSettings.outputSampleRate);
        src = GetComponent<AudioSource>();
        src.loop = true;
        src.clip = Microphone.Start (
            Microphone.devices[micDeviceId],
            true,
            1,
            AudioSettings.outputSampleRate);
        src.volume = 1;
        src.Play();
    }

    // this handles the microphone data, it sends the data and deletes any further audio output
    void OnAudioFilterRead(float[] data, int channels) {
        // add mic data to buffer
        micDataQueue.AddRange(data);

        // take pieces of buffer and send data
        while (micDataQueue.Count > dataSendBuffer.Length) { 
            dataSendBuffer = micDataQueue.GetRange(0, dataSendBuffer.Length).ToArray();
            SendData(dataSendBuffer);
            micDataQueue.RemoveRange(0, dataSendBuffer.Length);
        }

        // clear array so we dont output any sound
        for (int i=0; i<data.Length; i++) {
            data[i] = 0;
        }
    }

    // function which "sends" data (from micData to playData)
    void SendData (float[] micData) {

        encodedData = encoder.Encode(micData);

        // the data would need to be sent over the network, we just decode it now to test the result

        decodedData = decoder.DecodePacketFloat(encodedData);



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
        playWritePosition += decodedData.Length;

        Debug.Log("Opustest.SendData: transmission data " + encodedData.Length + " inData " + micData.Length + " outData " + decodedData.Length + " "); // + decoder.<);



        if (false) { 
            // input & ouput volume test
            float inA = 0;
            float outA = 0;
            foreach (float sample in micData) {
                inA += Mathf.Abs(sample);
            }
            foreach (float sample in decodedData) {
                outA += Mathf.Abs(sample);
            }
            float inAv = inA / micData.Length;
            float outAv = outA / decodedData.Length;
            Debug.Log("Opustest.SendData: input volume " + inAv.ToString("0.000") + " output volume " + outAv.ToString("0.000"));
        }
    }


    // this is used by the second audio source, to read data from playData and play it back
    void OnAudioRead(float[] data) {
        //Debug.Log("Opustest.OnAudioRead: play data " + data.Length);
        // data overflow
        if ((playPosition + data.Length) > playData.Length) {
            int maxSize = playData.Length - playPosition;
            Array.Copy(playData, playPosition, data, 0, maxSize);
            Array.Copy(playData, 0, data, maxSize, data.Length - maxSize);
        }
        // we can just copy it over
        else {
            Array.Copy(playData, playPosition, data, 0, data.Length);
        }
        playPosition = (playPosition + data.Length) % (playDataMaxLength * audioSamplingRate);
    }
    void OnAudioSetPosition(int newPosition) {
        playPosition = newPosition;
    }
}
