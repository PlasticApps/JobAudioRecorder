using System;
using System.Collections;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Android;


namespace PlasticApps.Record
{
    [RequireComponent(typeof(AudioListener))]
    [DefaultExecutionOrder(10)]
    public class AudioRecorder : AbstractRecorder
    {
        public DataPath MOutputDir;

        private MemoryStream _outputStream;
        private BinaryWriter _outputWriter;
        private string _outPath;

        public override bool BeginRecording()
        {
            if (m_recording) return false;

            #if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            #endif
            
            MOutputDir.CreateDirectory();
            {
                _outPath = MOutputDir.GetFullPath() + "/" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav";
                _outputStream = new MemoryStream();
                _outputWriter = new BinaryWriter(_outputStream);
                m_recording = true;
            }
            return true;
        }

        public void OnAudioFilterRead(float[] data, int channels)
        {
            if (isRecording)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    _outputWriter.Write((short) (data[i] * 32767f));
                }
            }
        }

        public override void EndRecording()
        {
            if (!m_recording) return;
            m_recording = false;
            StartCoroutine(Release(_outPath));
        }

        /// <summary>
        /// Start saving stream in job
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        IEnumerator Release(string file)
        {
            NativeArray<int> data = new NativeArray<int>(1, Allocator.Persistent);
            NativeArray<byte> stream = new NativeArray<byte>(_outputStream.ToArray(), Allocator.Persistent);
            NativeArray<byte> path = new NativeArray<byte>(Encoding.ASCII.GetBytes(file), Allocator.Persistent);

            JobHandle handle = new JobWaveEncode
            {
                Data = data,
                Stream = stream,
                Path = path
            }.Schedule();

            while (!handle.IsCompleted)
                yield return null;

            handle.Complete();

            _outputStream = null;
            _outputWriter = null;

            if (data[0] == 1) OnReplay(file);

            data.Dispose();
        }

        void OnReplay(string path)
        {
            // TO SHARE FILE PASTE CODE HERE (;
            Debug.Log(path);
        }

        // JOB write Stream to File
        struct JobWaveEncode : IJob
        {
            public NativeArray<int> Data;

            [DeallocateOnJobCompletion] public NativeArray<byte> Stream;
            [DeallocateOnJobCompletion] public NativeArray<byte> Path;

            public void Execute()
            {
                try
                {
                    // WAVE Audio Settings
                    short BITS_PER_SAMPLE = 16;
                    float HEADER_SIZE = 44;
                    int SAMPLE_RATE = 48000;
                    int channels = 2;
                    
                    long numberOfSamples = Stream.Length / (BITS_PER_SAMPLE / 8);
                    MemoryStream outputHeaderStream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(outputHeaderStream);
                    string filename = Encoding.ASCII.GetString(Path.ToArray());
                    byte[] buffer = Stream.ToArray();

                    writer.Write(0x46464952);
                    writer.Write((int) (HEADER_SIZE + numberOfSamples * BITS_PER_SAMPLE * channels / 8) - 8);
                    writer.Write(0x45564157);
                    writer.Write(0x20746d66);
                    writer.Write(16);
                    writer.Write((short) 1);
                    writer.Write((short) channels);
                    writer.Write(SAMPLE_RATE);
                    writer.Write(SAMPLE_RATE * channels * (BITS_PER_SAMPLE / 8));
                    writer.Write((short) (channels * (BITS_PER_SAMPLE / 8)));
                    writer.Write(BITS_PER_SAMPLE);
                    writer.Write(0x61746164);
                    writer.Write((int) (numberOfSamples * BITS_PER_SAMPLE * channels / 8));

                    outputHeaderStream.Write(buffer, 0, Stream.Length);
                    outputHeaderStream.Position = 0;

                    FileStream fs = File.OpenWrite(filename);
                    outputHeaderStream.WriteTo(fs);
                    fs.Close();

                    Data[0] = 1;
                }
                catch (Exception e)
                {
                    Data[0] = 0;
                    Debug.LogException(e);
                }
            }
        }
    }
}