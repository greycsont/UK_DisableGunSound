using System;
using System.Collections.Generic;
using System.Xml.Linq;
using JetBrains.Annotations;

public class StreamedAudioData
{
    private const int _CHUNK_SIZE = 8192; // 8KB chunks
    private readonly List<float[]> _chunks = new List<float[]>();
    private float[] _currentChunk = new float[_CHUNK_SIZE];
    private int _currentPos;

    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int TotalSamples { get; private set; }

    public void AddSamples(float[] samples)
    {
        foreach (var sample in samples)
        {
            _currentChunk[_currentPos++] = sample;
            TotalSamples++;

            if (_currentPos >= _CHUNK_SIZE)
            {
                _chunks.Add(_currentChunk);
                _currentChunk = new float[_CHUNK_SIZE];
                _currentPos = 0;
            }
        }
    }

    public float[] GetAllSamples()
    {
        if (_currentPos > 0)
        {
            Array.Resize(ref _currentChunk, _currentPos);
            _chunks.Add(_currentChunk);
            _currentChunk = Array.Empty<float>();
        }

        var result = new float[TotalSamples];
        int pos = 0;

        foreach (var chunk in _chunks)
        {
            Array.Copy(chunk, 0, result, pos, chunk.Length);
            pos += chunk.Length;
        }

        return result;
    }

}