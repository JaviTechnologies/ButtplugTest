using System.Threading.Tasks;
using Buttplug.Client;
using Buttplug.Core.Logging;
using Buttplug.Core.Messages;
using Buttplug.Server.Managers.UWPBluetoothManager;
//using Buttplug.Server.Bluetooth;
using UnityEngine;

public class LaunchControl : MonoBehaviour {

    private static async Task RunButtplugExample()
    {
        // Use an embedded connector to create server and client in the same run
        ButtplugEmbeddedConnector connector = new ButtplugEmbeddedConnector("MikoosServer");
        //var webConnector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12340"));

        // create client
        ButtplugClient client = new ButtplugClient("MikoosClient", connector);

        //setup event handlers
        client.DeviceAdded += (aObj, aDeviceEventArgs) =>
            Debug.Log($">>>>>>>>>>>>>>>> Device {aDeviceEventArgs.Device.Name} Connected!");

        client.DeviceRemoved += (aObj, aDeviceEventArgs) =>
            Debug.Log($">>>>>>>>>>>>>>>> Device {aDeviceEventArgs.Device.Name} Removed!");

        var server = connector.Server;

        server.AddDeviceSubtypeManager((IButtplugLogManager aLogManager) => new UWPBluetoothManager(aLogManager));

        //connect client to server
        try
        {
            Debug.Log(">>>>>>>>>>>>>>>>>>> Trying to CONNECT ");
            await client.ConnectAsync();
        }
        catch (ButtplugClientConnectorException ex)
        {
            Debug.Log($">>>>>>>>>>>>>>>>> Can't connect to Buttplug Server, exiting! Message: {ex.InnerException.Message}");
            return;
        }
        Debug.Log(">>>>>>>>>>>>>>>>>>>>> Connected to Server!");

        client.DeviceAdded += HandleDeviceAdded;
        client.DeviceRemoved += HandleDeviceRemoved;

        //setup scanning event handler
        client.ScanningFinished += (aObj, aScanningFinishedArgs) =>
            Debug.Log(">>>>>>>>>>>>>>>>>>> Device scanning is finished!");

        Debug.Log(">>>>>>>>>>>>>>>>>>> Scanning for devices...");
        await client.StartScanningAsync();

        // Stop scanning now, 'cause we don't want new devices popping up anymore.
        await client.StopScanningAsync();

        //list devices detected
        Debug.Log(">>>>>>>>>>>>>>>>>>> Client currently knows about these devices:");
        foreach (var device in client.Devices)
        {
            Debug.Log($"- {device.Name}");
        }

        Debug.Log(">>>>>>>>>>>>>>>>>>> END OF FUNCTION");

    }

    //Log Function
    static void HandleLogMessage(object aObj, LogEventArgs aArgs)
    {
        Debug.Log($"LOG: {aArgs.Message.LogMessage}");
    }


    static void HandleDeviceAdded(object aObj, DeviceAddedEventArgs aArgs)
    {
        Debug.Log($">>>>>>>>>>>>>>>>>>> Device connected: {aArgs.Device.Name}");
    }

    static void HandleDeviceRemoved(object aObj, DeviceRemovedEventArgs aArgs)
    {
        Debug.Log($">>>>>>>>>>>>>>>>>>>>Device connected: {aArgs.Device.Name}");
    }


    public void StartButtplug()
    {
        Debug.Log(">>>>>>>>>>>>>>>>>>> Button START Pressed");
        RunButtplugExample().Wait();
        Debug.Log(">>>>>>>>>>>>>>>>>>> STOP FUNCTION");

    }


}
