using Buttplug;
using UniRx;
using UnityEngine;

public class ExampleController : MonoBehaviour
{
    private void OnEnable()
    {
        // add listener for device status change events
        DeviceConnector.Instance.OnDeviceStatusChanged.AddListener(HandleDeviceStatusChanged);
    }

    private void OnDisable()
    {
        // remove listener
        DeviceConnector.Instance.OnDeviceStatusChanged.RemoveListener(HandleDeviceStatusChanged);
    }

    /// <summary>
    /// Handle changes on the device connectivity
    /// </summary>
    /// <param name="status"></param>
    void HandleDeviceStatusChanged(DeviceConnector.DeviceStatus status)
    {
        // Since this function gets called from a library, it is outside the main thread...
        // ... We need to execute any Unity API related code on the main thread (or the app will crash)
        Scheduler.MainThread.Schedule(_ =>
        {
            // do something...
        });
    }

    /// <summary>
    /// Opens a connection to a Launch device
    /// </summary>
    public void StartConnetion()
    {
        // open a connection
        DeviceConnector.Instance.Start();
    }

    /// <summary>
    /// Moves the device UP
    /// </summary>
    public void MoveUp()
    {
        var duration = 200L;
        var position = .6f;

        //issue cmd to get Up
        DeviceConnector.Instance.IssueStroke(duration, position);
    }

    /// <summary>
    /// Moves the device DOWN
    /// </summary>
    public void MoveDown()
    {
        var duration = 230L;
        var position = .1f;

        //issue cmd to get Down
        DeviceConnector.Instance.IssueStroke(duration, position);
    }
}
