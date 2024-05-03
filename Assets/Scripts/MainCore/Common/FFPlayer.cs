#if false
using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Example;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace MainCore.Common
{
    /// <summary>
    /// FFPlayer
    /// </summary>
    public sealed unsafe class FFPlayer : IDisposable
    {
        private string _url;
        int error;
        AVHWDeviceType deviceType;
        private Size _frameSize;
        private AVPixelFormat _pixelFormat;
        private AVSampleFormat _sample_fmt;

        private readonly AVCodecContext* _pVideoContext;
        private readonly AVCodecContext* _pAudioContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly int _videoStreamIndex;
        private readonly int _audioStreamIndex;

        private readonly AVFrame* _audioFrame;
        private readonly AVFrame* _videoFrame;
        private AVFrame* _g2cFrame;
        private AVFrame _tempFrame;
        private readonly AVPacket* _packet;

        SwrContext* _audioSwrContext;
        int outChannelCount;
        AVSampleFormat outFormat;
        AVPixelFormat destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;

        private Action<byte[]> _onVideoData;
        private Action<byte[]> _onAudioData;
        private Action<int, int> _onVideoSize;

        Thread thread = null;

        public FFPlayer(string url, Action<int, int> onVideoSize, Action<byte[]> onVideoData, Action<byte[]> onAudioData)
        {
            _url = url;
            _onVideoSize += onVideoSize;
            _onVideoData += onVideoData;
            _onAudioData += onAudioData;
            Init();
            _pFormatContext = ffmpeg.avformat_alloc_context();
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();
            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();
            AVCodec* videoCodec = null;
            _videoStreamIndex = ffmpeg.av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &videoCodec, 0).ThrowExceptionIfError();
            AVCodec* audioCodec = null;
            _audioStreamIndex = ffmpeg.av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &audioCodec, 0).ThrowExceptionIfError();
            Debug.LogWarning("_videoStreamIndex:" + _videoStreamIndex);
            Debug.LogWarning("_audioStreamIndex:" + _audioStreamIndex);

            _pVideoContext = ffmpeg.avcodec_alloc_context3(videoCodec);
            Debug.LogWarning("deviceType:" + deviceType);
            if (deviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                ffmpeg.av_hwdevice_ctx_create(&_pVideoContext->hw_device_ctx, deviceType, null, null, 0).ThrowExceptionIfError();
            }
            AVCodecParameters* vavcp = _pFormatContext->streams[_videoStreamIndex]->codecpar;
            ffmpeg.avcodec_parameters_to_context(_pVideoContext, vavcp).ThrowExceptionIfError();

            _pAudioContext = ffmpeg.avcodec_alloc_context3(audioCodec);
            AVCodecParameters* aavcp = _pFormatContext->streams[_audioStreamIndex]->codecpar;
            ffmpeg.avcodec_parameters_to_context(_pAudioContext, aavcp).ThrowExceptionIfError();

            ffmpeg.avcodec_open2(_pVideoContext, videoCodec, null).ThrowExceptionIfError();
            ffmpeg.avcodec_open2(_pAudioContext, audioCodec, null).ThrowExceptionIfError();

            _frameSize = new Size(_pVideoContext->width, _pVideoContext->height);
            if (_onVideoSize != null)
            {
                _onVideoSize(_frameSize.Width, _frameSize.Height);
            }
            _pixelFormat = _pVideoContext->pix_fmt;
            _sample_fmt = _pVideoContext->sample_fmt;

            //frame->16bit 44100 PCM 统一音频采样格式与采样率
            //创建swrcontext上下文件
            _audioSwrContext = ffmpeg.swr_alloc();
            //音频格式  输入的采样设置参数
            AVSampleFormat inFormat = _pAudioContext->sample_fmt;
            // 出入的采样格式
            outFormat = AVSampleFormat.AV_SAMPLE_FMT_S16;
            // 输入采样率
            int inSampleRate = _pAudioContext->sample_rate;
            // 输出采样率
            int outSampleRate = 44100;
            // 输入声道布局
            ulong in_ch_layout = _pAudioContext->channel_layout;
            //输出声道布局
            int out_ch_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
            //给Swrcontext 分配空间，设置公共参数
            ffmpeg.swr_alloc_set_opts(_audioSwrContext, out_ch_layout, outFormat, outSampleRate,
                (long)in_ch_layout, inFormat, inSampleRate, 0, null
            );
            // 初始化
            ffmpeg.swr_init(_audioSwrContext);
            // 获取声道数量
            outChannelCount = ffmpeg.av_get_channel_layout_nb_channels((ulong)out_ch_layout);

            _packet = ffmpeg.av_packet_alloc();
            _audioFrame = ffmpeg.av_frame_alloc();
            _videoFrame = ffmpeg.av_frame_alloc();
            _g2cFrame = ffmpeg.av_frame_alloc();

            thread = new Thread(new ThreadStart(DecodeTest));
            thread.IsBackground = true;
            thread.Start();
        }

        private void Init()
        {
            ffmpeg.RootPath = Application.streamingAssetsPath + "/FFmpeg/x86_64";
            Debug.LogWarning($"FFmpeg version info: {ffmpeg.av_version_info()}");
            SetupLogging();
            ConfigureHWDecoder();
        }

        bool isVideo;

        private void DecodeTest()
        {
            while (true)
            {
                ffmpeg.av_frame_unref(_audioFrame);
                ffmpeg.av_frame_unref(_videoFrame);

                do
                {
                    //开始读取源文件，进行解码
                    error = ffmpeg.av_read_frame(_pFormatContext, _packet);
                    if (error == ffmpeg.AVERROR_EOF)
                    {
                        Debug.LogWarning("over");
                        break;
                    }
                    if (_packet->stream_index == _audioStreamIndex)
                    {
                        error = ffmpeg.avcodec_send_packet(_pAudioContext, _packet);
                        //解码
                        error = ffmpeg.avcodec_receive_frame(_pAudioContext, _audioFrame);

                        isVideo = false;
                    }
                    else if (_packet->stream_index == _videoStreamIndex)
                    {
                        error = ffmpeg.avcodec_send_packet(_pVideoContext, _packet);
                        //解码
                        error = ffmpeg.avcodec_receive_frame(_pVideoContext, _videoFrame);

                        isVideo = true;
                    }
                }
                while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
                if (!isVideo)
                {
                    byte* out_buffer = (byte*)Marshal.AllocHGlobal(2 * 44100);
                    //将每一帧数据转换成pcm
                    ffmpeg.swr_convert(_audioSwrContext, &out_buffer, 2 * 44100, (byte**)&_audioFrame->data, _audioFrame->nb_samples);
                    //获取实际的缓存大小
                    int out_buffer_size = ffmpeg.av_samples_get_buffer_size(null, outChannelCount, _audioFrame->nb_samples, outFormat, 1);
                    if (out_buffer_size > 0)
                    {
                        byte[] data = new byte[out_buffer_size];
                        Marshal.Copy((IntPtr)out_buffer, data, 0, out_buffer_size);
                        if (_onAudioData != null)
                        {
                            _onAudioData(data);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("out_buffer_size:" + out_buffer_size);
                    }
                    Marshal.FreeCoTaskMem((IntPtr)out_buffer);
                }
                if (isVideo)
                {
                    if (_pVideoContext->hw_device_ctx != null)
                    {
                        ffmpeg.av_hwframe_transfer_data(_g2cFrame, _videoFrame, 0).ThrowExceptionIfError();
                        _tempFrame = *_g2cFrame;
                    }
                    else
                    {
                        _tempFrame = *_videoFrame;
                    }
                    var sourcePixelFormat = GetHWPixelFormat(deviceType);
                    using (var vfc = new VideoFrameConverter(_frameSize, sourcePixelFormat, _frameSize, destinationPixelFormat))
                    {
                        AVFrame convertedFrame = vfc.Convert(_tempFrame);
                        IntPtr imgPtr = (IntPtr)convertedFrame.data[0];
                        int dataLen = _frameSize.Width * _frameSize.Height * 3;
                        byte[] data = new byte[dataLen];
                        Marshal.Copy((IntPtr)convertedFrame.data[0], data, 0, data.Length);
                        if (_onVideoData != null)
                        {
                            _onVideoData(data);
                        }
                        Marshal.FreeCoTaskMem(imgPtr);
                    }
                }
                Thread.Sleep(12);
            }
        }

        public void Play()
        {

        }

        public void Pause()
        {

        }

        public void Stop()
        {

        }

        private unsafe void SetupLogging()
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);
            // do not convert to local function
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level())
                {
                    return;
                }
                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                Debug.LogWarning(line);
            };
            ffmpeg.av_log_set_callback(logCallback);
        }

        private void ConfigureHWDecoder()
        {
            deviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var availableHWDecoders = new Dictionary<int, AVHWDeviceType>();

            var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var number = 0;
            while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                Debug.Log($"{++number}. {type}");
                availableHWDecoders.Add(number, type);
            }
            if (availableHWDecoders.Count == 0)
            {
                Debug.Log("Your system have no hardware decoders.");
                deviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
                return;
            }
            int decoderNumber = availableHWDecoders.SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
            if (decoderNumber == 0)
            {
                decoderNumber = availableHWDecoders.First().Key;
            }
            Debug.LogWarning($"Selected [{decoderNumber}]");
            int.TryParse(Console.ReadLine(), out var inputDecoderNumber);
            availableHWDecoders.TryGetValue(inputDecoderNumber == 0 ? decoderNumber : inputDecoderNumber, out deviceType);
        }

        private AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
        {
            switch (hWDevice)
            {
                case AVHWDeviceType.AV_HWDEVICE_TYPE_NONE:
                    return AVPixelFormat.AV_PIX_FMT_NONE;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU:
                    return AVPixelFormat.AV_PIX_FMT_VDPAU;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA:
                    return AVPixelFormat.AV_PIX_FMT_CUDA;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI:
                    return AVPixelFormat.AV_PIX_FMT_VAAPI;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2:
                    return AVPixelFormat.AV_PIX_FMT_NV12;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_QSV:
                    return AVPixelFormat.AV_PIX_FMT_QSV;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX:
                    return AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA:
                    return AVPixelFormat.AV_PIX_FMT_NV12;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_DRM:
                    return AVPixelFormat.AV_PIX_FMT_DRM_PRIME;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL:
                    return AVPixelFormat.AV_PIX_FMT_OPENCL;
                case AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC:
                    return AVPixelFormat.AV_PIX_FMT_MEDIACODEC;
                default:
                    return AVPixelFormat.AV_PIX_FMT_NONE;
            }
        }

        public void Dispose()
        {
            if (thread != null)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            ffmpeg.av_frame_unref(_audioFrame);
            ffmpeg.av_free(_audioFrame);

            ffmpeg.av_frame_unref(_videoFrame);
            ffmpeg.av_free(_videoFrame);

            ffmpeg.av_frame_unref(_g2cFrame);
            ffmpeg.av_free(_g2cFrame);

            ffmpeg.av_packet_unref(_packet);
            ffmpeg.av_free(_packet);

            ffmpeg.avcodec_close(_pVideoContext);
            ffmpeg.avcodec_close(_pAudioContext);

            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }
    }
}
#endif