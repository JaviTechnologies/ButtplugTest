using UnityEngine;
using UnityEngine.Events;

namespace Buttplug
{
    /// <summary>
    /// Helper class to manipulate device
    /// </summary>
    public class DeviceConnector : AndroidJavaProxy
    {
        public enum DeviceStatus
        {
            None,
            Disconnected,
            Connected,
            Error
        }

        public class DeviceStatusChangedEvent : UnityEvent<DeviceStatus> { }

        /// <summary>
        /// locker for the creation of the instance
        /// </summary>
        private static readonly object locker = new object();

        /// <summary>
        /// Internal unique instance
        /// </summary>
        private static DeviceConnector m_Instance = null;

        /// <summary>
        ///  Instance access
        /// </summary>
        public static DeviceConnector Instance
        {
            get
            {
                // create instance using double-check locking...
                if (m_Instance == null)
                {
                    lock (locker)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new DeviceConnector();
                        }
                    }
                }

                // return unique instance
                return m_Instance;
            }
        }

        /// <summary>
        /// Server name to use
        /// </summary>
        const string m_ServerName = "Websocket Server";

        // Server max ping time
        const long m_MaxPingTime = 0L;

        /// <summary>
        /// Java client connector
        /// </summary>
        private AndroidJavaObject buttplugClient;

        /// <summary>
        /// NAme of the device to connect
        /// </summary>
        const string DEVICE_NAME = "Fleshlight Launch";

        /// <summary>
        /// Assigned id of the connected device
        /// </summary>
        long m_ConnectedDevice = -1;

        /// <summary>
        /// Event to observe changes on the device status
        /// </summary>
        public DeviceStatusChangedEvent OnDeviceStatusChanged = new DeviceStatusChangedEvent();

        DeviceStatus m_Status;
        public DeviceStatus Status
        {
            get
            {
                return m_Status;
            }
            private set
            {
                m_Status = value;

                OnDeviceStatusChanged.Invoke(m_Status);
            }
        }

        /// <summary>
        /// Returns true if device is conected
        /// </summary>
        public bool IsConnected { get { return Status == DeviceStatus.Connected; } }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DeviceConnector() : base("org.metafetish.buttplug.client.IServerConnectionEventHandler")
        {
            Status = DeviceStatus.None;

            //Setup Buttplug Server and Client from Java function
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject serverFactory = new AndroidJavaObject("org.metafetish.buttplug.components.controls.UnityAppContextServerFactory", activity, m_ServerName, m_MaxPingTime);
            buttplugClient = new AndroidJavaObject("org.metafetish.buttplug.client.ButtplugUnityEmbededClient", "Unity App", serverFactory, this);
        }

        public void Start()
        {
            Status = DeviceStatus.Disconnected;

            buttplugClient.Call("disconnect");
            buttplugClient.Call("connect");
        }

        /// <summary>
        /// Sends a stroke command to the connected device
        /// </summary>
        /// <param name="duration">Duration of the stroke in milliseconds</param>
        /// <param name="position">Final position of the stroke (between 0 and 1)</param>
        public void IssueStroke(long duration, double position)
        {
            if (m_ConnectedDevice > 0)
            {
                buttplugClient.Call("SendLinearCmdToLaunch", m_ConnectedDevice, duration, position, 1L);
            }
        }

        /// <summary>
        /// Sends a stroke command to the connected device
        /// </summary>
        /// <param name="speed">Speed of the stroke (between 0 and 1)</param>
        /// <param name="position">Final position of the stroke (between 0 and 1)</param>
        public void IssueStroke(double speed, double position)
        {
            if (m_ConnectedDevice > 0)
            {
                int finalSpeed = Mathf.Clamp((int)(speed * 100), 2, 99);
                int finalPosition = Mathf.Clamp((int)(position * 100), 1, 99);

                buttplugClient.Call("SendFleshlightMessage", m_ConnectedDevice, finalSpeed, finalPosition, 1L);
            }
        }

        public void OnServerConnectionSuccess()
        {
            Debug.Log("OnServerConnectionSuccess");
            buttplugClient.Call("requestStartScanning");
        }

        public void OnServerConnectionError(string msg)
        {
            Debug.Log("OnServerConnectionError " + msg);
        }

        public void OnDeviceAdded(long id, string name)
        {
            Debug.LogFormat("OnDeviceAdded [{0}] => {1}", id, name);
            if (DEVICE_NAME.Equals(name))
            {
                m_ConnectedDevice = id;

                Status = DeviceStatus.Connected;
            }
        }

        public void OnDeviceRemoved(long id)
        {
            Debug.LogFormat("OnDeviceRemoved {0}", id);
            if (m_ConnectedDevice.Equals(id))
            {
                m_ConnectedDevice = -1;

                Status = DeviceStatus.Disconnected;
            }
        }
    }
}
