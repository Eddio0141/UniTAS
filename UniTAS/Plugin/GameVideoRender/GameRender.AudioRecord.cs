using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using UniTAS.Plugin.UnityAudioGrabber;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class GameRender : IOnAudioFilterRead
{
    private const string OutputFile = "audio.wav";

    private FileStream _audioFileStream;

    private Thread _audioProcessingThread;
    private Queue<float[]> _audioProcessingQueue;
    private bool _recordingAudio;

    private readonly int _channels = AudioSettings.speakerMode switch
    {
        AudioSpeakerMode.Raw => 1,
        AudioSpeakerMode.Mono => 1,
        AudioSpeakerMode.Stereo => 2,
        AudioSpeakerMode.Quad => 4,
        AudioSpeakerMode.Surround => 4,
        AudioSpeakerMode.Mode5point1 => 6,
        AudioSpeakerMode.Mode7point1 => 8,
        AudioSpeakerMode.Prologic => 2,
        _ => 1
    };

    private static float AudioFrequency => 1f / AudioSettings.outputSampleRate;
    private float _audioTime;

    private void StartAudioCapture()
    {
        _audioFileStream = new(OutputFile, FileMode.Create);
        _audioProcessingThread = new(AudioProcessingThread);
        _audioProcessingThread.Start();

        _audioProcessingQueue = new();
        _recordingAudio = true;
    }

    public void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_recordingAudio) return;
        _audioProcessingQueue.Enqueue(data);
    }

    private void SaveWavFile()
    {
        // wait for all audio data to be written
        _audioProcessingThread.Join();
    }

    private void AudioProcessingThread()
    {
        while (true)
        {
            if (_audioProcessingQueue.Count == 0)
            {
                Thread.Sleep(1);
                continue;
            }

            if (!_recording && _audioTime >= _renderedTime)
            {
                _recordingAudio = false;

                // finish up
                WriteHeader();
                _audioFileStream.Close();

                return;
            }

            var data = _audioProcessingQueue.Dequeue();
            var bytes = new byte[data.Length * 2];
            const int rescaleFactor = 32767; //to convert float to Int16

            for (var i = 0; i < data.Length; i++)
            {
                var intData = (short)(data[i] * rescaleFactor);
                var byteArr = new[] { (byte)intData, (byte)(intData >> 8) };
                byteArr.CopyTo(bytes, i * 2);
            }

            _audioFileStream.Write(bytes, 0, bytes.Length);

            _audioTime += AudioFrequency;
        }
    }

    private void WriteHeader()
    {
        var header = new List<byte>();

        // RIFF header
        header.AddRange(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
        header.AddRange(BitConverter.GetBytes((int)_audioFileStream.Length - 8));
        header.AddRange(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });

        // fmt chunk
        header.AddRange(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
        header.AddRange(BitConverter.GetBytes(16));
        header.AddRange(BitConverter.GetBytes((short)1));
        header.AddRange(BitConverter.GetBytes((short)_channels));
        header.AddRange(BitConverter.GetBytes(AudioSettings.outputSampleRate));
        header.AddRange(BitConverter.GetBytes(AudioSettings.outputSampleRate * _channels * 2));
        header.AddRange(BitConverter.GetBytes((short)(_channels * 2)));
        header.AddRange(BitConverter.GetBytes((short)16));

        // data chunk
        header.AddRange(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
        header.AddRange(BitConverter.GetBytes((int)_audioFileStream.Length - 44));

        _audioFileStream.Position = 0;
        _audioFileStream.Write(header.ToArray(), 0, header.Count);
    }
}