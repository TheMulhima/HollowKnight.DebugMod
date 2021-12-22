using System;
using System.Security.Cryptography.X509Certificates;
using Modding;
using UnityEngine;

namespace DebugMod.MonoBehaviours
{
    public class FrameByFrameAdvance : MonoBehaviour
    {
        public bool frameAdvance = false;

        private void Awake()
        {
            if (GameManager.instance.GetComponent<TimeScale>() != null)
            {
                Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
            }
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (!frameAdvance)
            {
                Time.timeScale = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (frameAdvance)
            {
                frameAdvance = false;
                Time.timeScale = 0f;
            }
        }

        public void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
    
