using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Buttplug
{
    public class DeviceConnectionIndicator : MonoBehaviour
    {
        [SerializeField]
        Image m_ConnectedIndicator;

        [SerializeField]
        Image m_DisconnectedIndicator;

        void OnEnable()
        {
            DeviceConnector.Instance.OnDeviceStatusChanged.AddListener(HandleDeviceStatusChanged);
        }

        void OnDisable()
        {
            DeviceConnector.Instance.OnDeviceStatusChanged.RemoveListener(HandleDeviceStatusChanged);
        }

        void HandleDeviceStatusChanged(DeviceConnector.DeviceStatus status)
        {
            // Since this function gets called from a library, it is outside the main thread...
            // ... We need to execute any Unity API related code on the main thread (or the app will crash)
            Scheduler.MainThread.Schedule(_ =>
            {
                var connected = status == DeviceConnector.DeviceStatus.Connected;

                m_DisconnectedIndicator.enabled = !connected;
                m_ConnectedIndicator.enabled = connected;
            });
        }
    }
}

