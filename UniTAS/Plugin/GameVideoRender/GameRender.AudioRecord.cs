using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    private bool _recordingAudio;
    private bool _firstAudioProcess;

    private float[] _audioData;

    // for 1 channel
    private int _audioDataGrabLength;

    private double _audioTimer;
    private double _audioBufferDuration;

    private List<byte> _audioDataBuffer;

    private void StartAudioCapture()
    {
        Trace.Write($"Starting audio capture, channels: {_channels}, sample rate: {AudioSettings.outputSampleRate}");

        _firstAudioProcess = true;
        _recordingAudio = true;
        _audioFileStream = new(OutputFile, FileMode.Create);
        _audioProcessingThread = new(AudioProcessingThread);
        _audioProcessingThread.Start();

        _audioProcessingQueue = new();
        _audioDataBuffer = new();

        _audioTimer = 0;
    }

    public void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_recordingAudio) return;

        // to be safe, check every time
        var dataLen = data.Length / channels;
        if (_audioDataGrabLength != dataLen)
        {
            _audioDataGrabLength = dataLen;
            _audioBufferDuration = 1.0 / AudioSettings.outputSampleRate * dataLen;
            _audioData = new float[dataLen];
            Trace.Write($"New audio data length: {dataLen}, buffer duration: {_audioBufferDuration}");
        }

        _audioTimer += _audioBufferDuration;

        if (_firstAudioProcess)
        {
            _firstAudioProcess = false;
            return;
        }

        for (var i = 0; i < _channels; i++)
        {
            AudioListener.GetOutputData(_audioData, i);
            _audioProcessingQueue.Enqueue(_audioData);
        }
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
            var allChannels = new List<float[]>();
            for (var i = 0; i < _channels; i++)
            {
                allChannels.Add(_audioProcessingQueue.Dequeue());
            }

            var dataLen = allChannels[0].Length;

            for (var i = 0; i < dataLen; i++)
            {
                for (var j = 0; j < _channels; j++)
                {
                    var intData = (short)(allChannels[j][i] * 32767);
                    _audioDataBuffer.Add((byte)intData);
                    _audioDataBuffer.Add((byte)(intData >> 8));
                }
            }
        }

        // trim if too much data compared to video
        if (_audioTimer > _videoTimer)
        {
            var trim = (int)((_audioTimer - _videoTimer) * AudioSettings.outputSampleRate * _channels * 2);
            Trace.Write($"Trimming audio data by {_audioTimer - _videoTimer} seconds ({trim} bytes)");
            _audioDataBuffer.RemoveRange(_audioDataBuffer.Count - trim, trim);
        }

        Trace.Write($"Writing {_audioDataBuffer.Count} bytes of audio data to file {OutputFile}");
        _audioFileStream.Write(_audioDataBuffer.ToArray(), 0, _audioDataBuffer.Count);
        _audioFileStream.Flush();

        Trace.Write("Done writing audio data");
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