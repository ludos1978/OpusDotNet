using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using POpusCodec.Enums;

namespace POpusCodec
{
    internal class Wrapper
    {
        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_encoder_get_size(Channels channels);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern OpusStatusCode opus_encoder_init(IntPtr st, SamplingRate Fs, Channels channels, OpusApplicationType application);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern string opus_get_version_string();

        public static IntPtr opus_encoder_create(SamplingRate Fs, Channels channels, OpusApplicationType application)
        {
            int size = Wrapper.opus_encoder_get_size(channels);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            OpusStatusCode statusCode = Wrapper.opus_encoder_init(ptr, Fs, channels, application);

            try
            {
                HandleStatusCode(statusCode);
            }
            catch (Exception ex)
            {
                if (ptr != IntPtr.Zero)
                {
                    Wrapper.opus_encoder_destroy(ptr);
                    ptr = IntPtr.Zero;
                }

                throw ex;
            }

            return ptr;
        }

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_encode(IntPtr st, short[] pcm, int frame_size, byte[] data, int max_data_bytes);

        public static int opus_encode(IntPtr st, short[] pcm, int frame_size, byte[] data)
        {
            if (st == IntPtr.Zero)
                throw new ObjectDisposedException("OpusEncoder");

            int payloadLength = opus_encode(st, pcm, frame_size, data, data.Length);

            if (payloadLength <= 0)
            {
                HandleStatusCode((OpusStatusCode)payloadLength);
            }

            return payloadLength;
        }

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_encode_float(IntPtr st, float[] pcm, int frame_size, byte[] data, int max_data_bytes);

        public static int opus_encode(IntPtr st, float[] pcm, int frame_size, byte[] data)
        {
            if (st == IntPtr.Zero)
                throw new ObjectDisposedException("OpusEncoder");

            int payloadLength = opus_encode_float(st, pcm, frame_size, data, data.Length);

            if (payloadLength <= 0)
            {
                HandleStatusCode((OpusStatusCode)payloadLength);
            }

            return payloadLength;
        }

        public static void opus_encoder_destroy(IntPtr st)
        {
            Marshal.FreeHGlobal(st);
        }

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_encoder_ctl(IntPtr st, OpusCtlSetRequest request, int value);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_encoder_ctl(IntPtr st, OpusCtlGetRequest request, ref int value);

        public static int get_opus_encoder_ctl(IntPtr st, OpusCtlGetRequest request)
        {
            if (st == IntPtr.Zero)
                throw new ObjectDisposedException("OpusEncoder");

            int value = 0;
            OpusStatusCode statusCode = (OpusStatusCode)opus_encoder_ctl(st, request, ref value);

            HandleStatusCode(statusCode);

            return value;
        }

        public static void set_opus_encoder_ctl(IntPtr st, OpusCtlSetRequest request, int value)
        {
            if (st == IntPtr.Zero)
                throw new ObjectDisposedException("OpusEncoder");

            OpusStatusCode statusCode = (OpusStatusCode)opus_encoder_ctl(st, request, value);

            HandleStatusCode(statusCode);
        }

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_decoder_get_size(Channels channels);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern OpusStatusCode opus_decoder_init(IntPtr st, SamplingRate Fs, Channels channels);

        public static IntPtr opus_decoder_create(SamplingRate Fs, Channels channels)
        {
            int size = Wrapper.opus_decoder_get_size(channels);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            OpusStatusCode statusCode = Wrapper.opus_decoder_init(ptr, Fs, channels);

            try
            {
                HandleStatusCode(statusCode);
            }
            catch (Exception ex)
            {
                if (ptr != IntPtr.Zero)
                {
                    Wrapper.opus_decoder_destroy(ptr);
                    ptr = IntPtr.Zero;
                }

                throw ex;
            }

            return ptr;
        }

        public static void opus_decoder_destroy(IntPtr st)
        {
            Marshal.FreeHGlobal(st);
        }


        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_decode(IntPtr st, byte[] data, int len, short[] pcm, int frame_size, int decode_fec);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_decode_float(IntPtr st, byte[] data, int len, float[] pcm, int frame_size, int decode_fec);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_decode(IntPtr st, IntPtr data, int len, short[] pcm, int frame_size, int decode_fec);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int opus_decode_float(IntPtr st, IntPtr data, int len, float[] pcm, int frame_size, int decode_fec);


        public static int opus_decode(IntPtr st, byte[] data, short[] pcm, int decode_fec, int channels)
        {
            if (st == IntPtr.Zero)
                throw new ObjectDisposedException("OpusDecoder");

            int numSamplesDecoded = 0;

            if (data != null)
            {
                numSamplesDecoded = opus_decode(st, data, data.Length, pcm, pcm.Length / channels, decode_fec);
            }
            else
            {
                numSamplesDecoded = opus_decode(st, IntPtr.Zero, 0, pcm, pcm.Length / channels, decode_fec);
            }

            if (numSamplesDecoded == (int)OpusStatusCode.InvalidPacket)
                return 0;

            if (numSamplesDecoded <= 0)
            {
                HandleStatusCode((OpusStatusCode)numSamplesDecoded);
            }

            return numSamplesDecoded;
        }

        public static int opus_decode(IntPtr st, byte[] data, float[] pcm, int decode_fec, int channels)
        {
            if (st == IntPtr.Zero)
                throw new ObjectDisposedException("OpusDecoder");

            int numSamplesDecoded = 0;

            if (data != null)
            {
                numSamplesDecoded = opus_decode_float(st, data, data.Length, pcm, pcm.Length / channels, decode_fec);
            }
            else
            {
                numSamplesDecoded = opus_decode_float(st, IntPtr.Zero, 0, pcm, pcm.Length / channels, decode_fec);
            }

            if (numSamplesDecoded == (int)OpusStatusCode.InvalidPacket)
                return 0;

            if (numSamplesDecoded <= 0)
            {
                HandleStatusCode((OpusStatusCode)numSamplesDecoded);
            }

            return numSamplesDecoded;
        }


        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int opus_packet_get_bandwidth(byte[] data);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int opus_packet_get_nb_channels(byte[] data);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern string opus_strerror(OpusStatusCode error);


        private static void HandleStatusCode(OpusStatusCode statusCode)
        {
            if (statusCode != OpusStatusCode.OK)
            {
                throw new OpusException(statusCode, opus_strerror(statusCode));
            }
        }
    }
}
