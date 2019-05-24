using System.Collections;
using System.Collections.Generic;
using PlasticApps.Record;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlasticApps
{
    public class SimpleTap : MonoBehaviour
    {
        public AudioSource _source;

        private void OnMouseDown()
        {
            _source?.PlayOneShot(_source.clip);
        }
    }
}