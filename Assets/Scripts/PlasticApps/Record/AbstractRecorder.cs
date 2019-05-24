using UnityEngine;
using System;
using System.IO;
using UnityEngine.Android;
#if UNITY_EDITOR
using UnityEditor;

#endif

public abstract class AbstractRecorder : MonoBehaviour
{
    protected bool m_recording = false;

    public bool isRecording
    {
        get { return m_recording; }
        set
        {
            if (value)
                BeginRecording();
            else
                EndRecording();
        }
    }

    public abstract bool BeginRecording();

    public abstract void EndRecording();

    protected virtual void OnDisable()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
#endif
        {
            EndRecording();
        }
    }
}