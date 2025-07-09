/*
 * Unity 我超你妈，为什么一个这么流行的游戏引擎
 * 结果对对应编程语言的支持一塌糊涂
 * 都他妈 2025 了我还不能用最新最热的C# 和 dotnet
 * 
 */



using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Diagnostics;


public static class UnitySupport
{
    public static async Task<AudioClip> LoadWithUnityAsync(string path, AudioType audioType)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        string url = new Uri(path).AbsoluteUri;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Delay(10);
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                LogManager.LogError($"UnityRequest Failed to load audio: {www.error}");
                return null;
            }

            var clip = DownloadHandlerAudioClip.GetContent(www);

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            LogManager.LogInfo($"Time used when loading with unity : {elapsedTime}");

            return clip;
        }
    }
}
