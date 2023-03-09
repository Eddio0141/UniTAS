using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using UniTAS.Plugin.Extensions;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class GameRender
{
    private const string OutputFile = "audio.wav";

    private FileStream _audioFileStream;

    private Thread _audioProcessingThread;
    private Queue<float[]> _audioProcessingQueue;
    private Queue<int> _audioDataLengthQueue;
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

    private bool _firstAudioProcess;

    private float[] _audioData;
    private int _actualAudioDataLength;
    private float _lastDeltaTime;

    private void StartAudioCapture()
    {
        _recordingAudio = true;
        _audioFileStream = new(OutputFile, FileMode.Create);
        _audioProcessingThread = new(AudioProcessingThread);
        _audioProcessingThread.Start();

        _audioProcessingQueue = new();
        _audioDataLengthQueue = new();
        _firstAudioProcess = true;
        _lastDeltaTime = 0f;
    }

    private void StopRecordingWav()
    {
        Trace.Write("Writing header and closing wav file");
        _recordingAudio = false;
        // wait for all audio data to be written
        _audioProcessingThread.Join();

        // finish up
        WriteHeader();
        _audioFileStream.Close();
    }

    private void SendAudioData()
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (Time.deltaTime != _lastDeltaTime)
        {
            _lastDeltaTime = Time.deltaTime;
            _actualAudioDataLength = (int)Math.Ceiling(_lastDeltaTime * AudioSettings.outputSampleRate);
            var audioDataLength = _actualAudioDataLength.RoundUpToNearestPowerOfTwo();
            Trace.Write(
                $"Frame time changed to {_lastDeltaTime}, new audio data length: {audioDataLength}, actual: {_actualAudioDataLength}");
            _audioData = new float[audioDataLength];
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        if (_firstAudioProcess)
        {
            _firstAudioProcess = false;
            return;
        }

        _audioDataLengthQueue.Enqueue(_actualAudioDataLength);
        for (var i = 0; i < _channels; i++)
        {
            AudioListener.GetOutputData(_audioData, i);
            _audioProcessingQueue.Enqueue(_audioData);
        }
    }

    private void AudioProcessingThread()
    {
        while (_recordingAudio)
        {
            if (_audioProcessingQueue.Count < _channels)
            {
                Thread.Sleep(1);
                continue;
            }

            // audio data is interleaved, e.g. [L, R, L, R, L, R, L, R]
            var dataLen = _audioDataLengthQueue.Dequeue();
            var allChannels = new List<float[]>();
            for (var i = 0; i < _channels; i++)
            {
                allChannels.Add(_audioProcessingQueue.Dequeue());
            }

            // if odd number of data points, remove last one
            if (dataLen % 2 != 0)
            {
                dataLen--;
            }

            var bytes = new byte[2];
            for (var i = 0; i < dataLen; i++)
            {
                for (var j = 0; j < _channels; j++)
                {
                    var intData = (short)(allChannels[j][i] * 32767);
                    bytes[0] = (byte)intData;
                    bytes[1] = (byte)(intData >> 8);

                    _audioFileStream.Write(bytes, 0, bytes.Length);
                }
            }
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