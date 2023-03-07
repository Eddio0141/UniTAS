using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class GameRender
{
    private const string OutputFile = "audio.wav";

    private FileStream _audioFileStream;

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
        _ => throw new ArgumentOutOfRangeException()
    };

    private void StartAudioCapture()
    {
        _audioFileStream = new(OutputFile, FileMode.Create);
    }

    private void WriteAudioData()
    {
        // invoked every frame (approx 60 times per second)
        // use GetOutputData

        var numSamples = AudioSettings.outputSampleRate / Fps;
        var numBytes = numSamples * _channels * 2;
        var data = new float[numSamples];
        var buffer = new byte[numBytes];

        for (var i = 0; i < _channels; i++)
        {
            AudioListener.GetOutputData(data, i);
            for (var j = 0; j < numSamples; j++)
            {
                var floatToShort = (short)(data[j] * 32767);
                var bytes = BitConverter.GetBytes(floatToShort);
                buffer[j * 2] = bytes[0];
                buffer[j * 2 + 1] = bytes[1];
            }

            _audioFileStream.Write(buffer, 0, buffer.Length);
        }
    }

    private void SaveWavFile()
    {
        WriteHeader();
        _audioFileStream.Close();
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