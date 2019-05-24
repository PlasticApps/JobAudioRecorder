using PlasticApps.Record;
using UnityEngine;
using UnityEngine.UI;

namespace PlasticApps
{
    public class RecordController : MonoBehaviour
    {
        public Text textObject;
        public AudioRecorder recorder;

        private Button button;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (recorder == null) enabled = false;
            else button?.onClick.AddListener(Toggle);
        }

        private void OnDisable()
        {
            button?.onClick.RemoveListener(Toggle);
        }

        public void Toggle()
        {
            recorder.isRecording = !recorder.isRecording;
            textObject.text = recorder.isRecording ? "Stop" : "Start";
        }
    }
}