
/*
 * stopwatch + ffmpeg + BepInEx logging = 崩溃
 * 如果没有其中一个都不会崩
 *
 * 已解决，利用.NET Core 的 code analyze 发现了 ProcessAudioFrame()
 * 里面的 while 循环会频繁调用 byte** convertedData = stackalloc byte*[1]
 * 导致 StackOverFlowException / 栈溢出
 *
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using UnityEngine;


public static class FFmpegSupport
{
    public static unsafe Task<AudioClip> DecodeAndLoad(string filePath)
    {
        return Task.Run(() =>
        {

            AVFormatContext*    formatContext    = null;
            AVCodecContext*     codecCtx         = null;
            SwrContext*         swrCtx           = null;
            AVPacket*           packet           = null;
            AVFrame*            frame            = null;

            int                 audioStreamIndex;

            ffmpeg.RootPath = PathManager.GetCurrentPluginPath(Path.Combine("lib", "ffmpeg")); // 或者你手动指定 dll 路径
            
            LogManager.LogDebug($"ffmpeg version: {ffmpeg.av_version_info()}");

            ffmpeg.avformat_network_init();

            formatContext = InitializeFormatContext(filePath);
            // 查找音频流
            audioStreamIndex = FindAudioStreamIndex(formatContext);

            // 设置解码器
            codecCtx = InitializeCodecContext(formatContext, audioStreamIndex);

            // swrCtx 初始化方式
            swrCtx = InitializeResampler(codecCtx);

            packet = ffmpeg.av_packet_alloc();

            frame = ffmpeg.av_frame_alloc();

            var streamedData = DecodeAudioFrames(formatContext, codecCtx, swrCtx, packet, frame, audioStreamIndex);


            // 创建 Unity AudioClip

            float[] sampleArray = streamedData.GetAllSamples();
            int channelCount    = codecCtx->ch_layout.nb_channels;
            int sampleRate      = codecCtx->sample_rate;
            int sampleCount     = sampleArray.Length / channelCount;



            AudioClip clip = AudioClip.Create("decoded_clip", sampleCount, channelCount, sampleRate, false);
            clip.SetData(sampleArray, 0);

            CleanupResources(frame, packet, codecCtx, swrCtx, formatContext);

            return clip;
        });
    }

    private static unsafe AVFormatContext* InitializeFormatContext(string filePath)
    {
        var formatContext = ffmpeg.avformat_alloc_context();
        if (ffmpeg.avformat_open_input(&formatContext, filePath, null, null) != 0)
            throw new Exception("Could not open input file.");

        if (ffmpeg.avformat_find_stream_info(formatContext, null) != 0)
            throw new Exception("Could not find stream info.");

        return formatContext;
    }

    private static unsafe int FindAudioStreamIndex(AVFormatContext* formatContext)
    {
        for (int i = 0; i < formatContext->nb_streams; i++)
        {
            if (formatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                return i;
            }
        }
        throw new Exception("No audio stream found.");
    }

    private static unsafe AVCodecContext* InitializeCodecContext(AVFormatContext* formatContext, int audioStreamIndex)
    {
        AVCodecParameters* codecpar = formatContext->streams[audioStreamIndex]->codecpar;
        AVCodec* codec = ffmpeg.avcodec_find_decoder(codecpar->codec_id);

        if (codec == null)
            throw new Exception("Unsupported codec");

        AVCodecContext* codecCtx = ffmpeg.avcodec_alloc_context3(codec);
        ffmpeg.avcodec_parameters_to_context(codecCtx, codecpar);

        if (ffmpeg.avcodec_open2(codecCtx, codec, null) < 0)
        {
            ffmpeg.avcodec_free_context(&codecCtx);
            throw new Exception("Could not open codec");
        }

        return codecCtx;
    }

    private static unsafe SwrContext* InitializeResampler(AVCodecContext* codecCtx)
    {
        int channels = codecCtx->ch_layout.nb_channels;
        AVChannelLayout inLayout = codecCtx->ch_layout;
        AVChannelLayout outLayout;
        ffmpeg.av_channel_layout_default(&outLayout, channels);

        var swrCtx = ffmpeg.swr_alloc();
        ffmpeg.av_opt_set_chlayout(swrCtx, "in_chlayout", &inLayout, 0);
        ffmpeg.av_opt_set_chlayout(swrCtx, "out_chlayout", &outLayout, 0);
        ffmpeg.av_opt_set_int(swrCtx, "in_sample_rate", codecCtx->sample_rate, 0);
        ffmpeg.av_opt_set_int(swrCtx, "out_sample_rate", codecCtx->sample_rate, 0);
        ffmpeg.av_opt_set_sample_fmt(swrCtx, "in_sample_fmt", codecCtx->sample_fmt, 0);
        ffmpeg.av_opt_set_sample_fmt(swrCtx, "out_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_FLT, 0);
        ffmpeg.swr_init(swrCtx);
        return swrCtx;
    }

    private static unsafe StreamedAudioData DecodeAudioFrames(AVFormatContext* formatContext,
                                                              AVCodecContext* codecCtx,
                                                              SwrContext* swrCtx,
                                                              AVPacket* packet,
                                                              AVFrame* frame,
                                                              int audioStreamIndex)
    {
        var streamedData = new StreamedAudioData
        {
            Channels = codecCtx->ch_layout.nb_channels,
            SampleRate = codecCtx->sample_rate
        };

        while (ffmpeg.av_read_frame(formatContext, packet) >= 0)
        {
            if (packet->stream_index != audioStreamIndex)
            {
                ffmpeg.av_packet_unref(packet);
                continue;
            }

            ProcessAudioFrame(codecCtx, swrCtx, packet, frame, streamedData);
            ffmpeg.av_packet_unref(packet);
        }
        return streamedData;
    }


    private static unsafe void ProcessAudioFrame(AVCodecContext* codecCtx,
                                                 SwrContext* swrCtx,
                                                 AVPacket* packet,
                                                 AVFrame* frame,
                                                 StreamedAudioData streamedData)
    {
        byte** convertedData = stackalloc byte*[1];
        int outLinesize;
        ffmpeg.avcodec_send_packet(codecCtx, packet);
        while (ffmpeg.avcodec_receive_frame(codecCtx, frame) == 0)
        {

            int outSamples = ffmpeg.av_samples_alloc_array_and_samples(
                &convertedData, &outLinesize,
                codecCtx->ch_layout.nb_channels,
                frame->nb_samples,
                AVSampleFormat.AV_SAMPLE_FMT_FLT,
                0);

            int samplesConverted = ffmpeg.swr_convert(swrCtx,
                convertedData,
                frame->nb_samples,
                frame->extended_data,
                frame->nb_samples);

            int bufferSize = ffmpeg.av_samples_get_buffer_size(
                null,
                codecCtx->ch_layout.nb_channels,
                samplesConverted,
                AVSampleFormat.AV_SAMPLE_FMT_FLT,
                1);

            float[] buffer = new float[bufferSize / sizeof(float)];
            Marshal.Copy((IntPtr)convertedData[0], buffer, 0, buffer.Length);
            streamedData.AddSamples(buffer);

            ffmpeg.av_freep(&convertedData[0]);
        }
    }

    private static unsafe void CleanupResources(AVFrame* frame,
                                                AVPacket* packet,
                                                AVCodecContext* codecCtx,
                                                SwrContext* swrCtx,
                                                AVFormatContext* formatContext)
    {
        if (frame != null) ffmpeg.av_frame_free(&frame);
        if (packet != null) ffmpeg.av_packet_free(&packet);
        if (codecCtx != null) ffmpeg.avcodec_free_context(&codecCtx);
        if (swrCtx != null)
        {
            ffmpeg.swr_close(swrCtx);
            ffmpeg.swr_free(&swrCtx);
        }
        if (formatContext != null)
        {
            ffmpeg.avformat_close_input(&formatContext);
            ffmpeg.avformat_free_context(formatContext);
        }
        ffmpeg.avformat_network_deinit();
    }
}
