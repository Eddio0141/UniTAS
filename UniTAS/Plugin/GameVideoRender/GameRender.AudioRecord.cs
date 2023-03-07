using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

public partial class GameRender
{
    private const string OutputFile = "audio.wav";

    private const int BufferSize = 1024;
    private const int NumBuffers = 2;
    private readonly int _outputRate = AudioSettings.outputSampleRate;
    private const int HeaderSize = 44;
    private List<byte> _recData;

    private void StartAudioCapture()
    {
        _recData = new();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_isRecording) return;

        foreach (var item in data)
        {
            var dataCopy = item;
            if (dataCopy < -1.0f)
            {
                dataCopy = -1.0f;
            }
            else if (dataCopy > 1.0f)
            {
                dataCopy = 1.0f;
            }

            var shortData = (short)(dataCopy * 32767.0f); // convert float to short
            var bytesData = System.BitConverter.GetBytes(shortData); // convert short to bytes

            _recData.Add(bytesData[0]); // add bytes to list
            _recData.Add(bytesData[1]);
        }
    }

    private void SaveWavFile()
    {
        var bytesFile = new byte[HeaderSize + _recData.Count]; // create a byte array for the whole file

        WriteHeader(bytesFile); // write header data

        for (var i = 0; i < _recData.Count; i++) // write audio data
        {
            bytesFile[HeaderSize + i] = _recData[i];
        }

        var fileStream = new FileStream(OutputFile, FileMode.Create); // create a file stream
        var binaryWriter = new BinaryWriter(fileStream);

        binaryWriter.Write(bytesFile); // write the byte array to the file

        binaryWriter.Close();
        fileStream.Close();

        Debug.Log($"File saved: {OutputFile}");

        _recData.Clear(); // clear recorded data list		
    }

    private void WriteHeader(byte[] bytesFile)
    {
        const int bitsPerSample = 16;
        const int channels = 2;

        const int subChunk1Size = 16;
        var subChunk2Size = _recData.Count;
        var chunkSize = 4 + 8 + subChunk1Size + 8 + subChunk2Size;

        var byteRate = _outputRate * channels * bitsPerSample / 8;
        const int blockAlign = channels * bitsPerSample / 8;

        bytesFile[0] = (byte)'R'; // RIFF/WAVE header
        bytesFile[1] = (byte)'I';
        bytesFile[2] = (byte)'F';
        bytesFile[3] = (byte)'F';
        bytesFile[4] = (byte)(chunkSize & 0xff);
        bytesFile[5] = (byte)((chunkSize >> 8) & 0xff);
        bytesFile[6] = (byte)((chunkSize >> 16) & 0xff);
        bytesFile[7] = (byte)((chunkSize >> 24) & 0xff);
        bytesFile[8] = (byte)'W';
        bytesFile[9] = (byte)'A';
        bytesFile[10] = (byte)'V';
        bytesFile[11] = (byte)'E';
        bytesFile[12] = (byte)'f'; // 'fmt ' chunk
        bytesFile[13] = (byte)'m';
        bytesFile[14] = (byte)'t';
        bytesFile[15] = (byte)' ';
        bytesFile[16] = subChunk1Size & 0xff;
        // bytesFile[17] = subChunk1Size >> 8 & 0xff;
        bytesFile[17] = 0;
        // bytesFile[18] = subChunk1Size >> 16 & 0xff;
        bytesFile[18] = 0;
        // bytesFile[19] = subChunk1Size >> 24 & 0xff;
        bytesFile[19] = 0;
        bytesFile[20] = 1; // format = 1
        bytesFile[21] = 0;
        bytesFile[22] = channels;
        bytesFile[23] = 0;
        bytesFile[24] = (byte)(_outputRate & 0xff);
        bytesFile[25] = (byte)((_outputRate >> 8) & 0xff);
        bytesFile[26] = (byte)((_outputRate >> 16) & 0xff);
        bytesFile[27] = (byte)((_outputRate >> 24) & 0xff);
        bytesFile[28] = (byte)(byteRate & 0xff);
        bytesFile[29] = (byte)((byteRate >> 8) & 0xff);
        bytesFile[30] = (byte)((byteRate >> 16) & 0xff);
        bytesFile[31] = (byte)((byteRate >> 24) & 0xff);
        bytesFile[32] = blockAlign;
        bytesFile[33] = 0;
        bytesFile[34] = bitsPerSample;
        bytesFile[35] = 0;
        bytesFile[36] = (byte)'d';
        bytesFile[37] = (byte)'a';
        bytesFile[38] = (byte)'t';
        bytesFile[39] = (byte)'a';
        bytesFile[40] = (byte)(subChunk2Size & 0xff);
        bytesFile[41] = (byte)((subChunk2Size >> 8) & 0xff);
        bytesFile[42] = (byte)((subChunk2Size >> 16) & 0xff);
        bytesFile[43] = (byte)((subChunk2Size >> 24) & 0xff);
    }
}