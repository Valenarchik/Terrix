using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class BootstrapNetworkManager : NetworkBehaviour
{
    public static BootstrapNetworkManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private TextMeshProUGUI ramText;
    [SerializeField] private TextMeshProUGUI cpuText;
    [SerializeField] private TextMeshProUGUI connectionsCountText;
    private TimeManager timeManager;

    private void Awake() => Instance = this;

    private void Update()
    {
        var ping = NetworkManager.TimeManager.RoundTripTime / 2;
        var text = $"Ping: {ping}";
        pingText.text = text;
        // var usedRam = System.GC.GetTotalMemory(false) / 1024 / 1024;
        // ramText.text = $"Used RAM: {usedRam}";
        // Resources.UnloadUnusedAssets();
    }


    public override void OnStartClient()
    {
        // StartCoroutine(MeasurePing());
    }

    public override void OnStartServer()
    {
        StartCoroutine(MeasurePing_OnServer());
    }

    private System.Collections.IEnumerator MeasurePing()
    {
        while (true)
        {
            var time = Time.time;

            Ping_ToServer(NetworkManager.ClientManager.Connection, time);

            yield return new WaitForSeconds(1f);
        }
    }

    private System.Collections.IEnumerator MeasurePing_OnServer()
    {
        while (true)
        {
            // Resources.UnloadUnusedAssets();
            //
            // UpdateConnectionCount_ToObserver(NetworkManager.ServerManager.Clients.Count);
            // var usedRam = SystemInfo.systemMemorySize / 1024;
            var usedRam = System.GC.GetTotalMemory(false) / 1024 / 1024;
            var cpu = Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
            UpdateRAMInfo_ToObserver(usedRam, cpu);
            yield return new WaitForSeconds(1f);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void Ping_ToServer(NetworkConnection conn, float time)
    {
        Ping_ToClient(conn, time);
    }

    [TargetRpc]
    private void Ping_ToClient(NetworkConnection conn, float time)
    {
        // Debug.Log(NetworkManager.TimeManager.PingInterval);
        // var ping = (Time.time - time) * 1000;
        // var text = $"Ping: {ping}";
        // pingText.text = text;
        // Debug.Log(text);
    }

    [ObserversRpc]
    private void UpdateConnectionCount_ToObserver(int count)
    {
        connectionsCountText.text = $"Connections count: {count}";
    }

    [ObserversRpc]
    private void UpdateRAMInfo_ToObserver(long ram, float cpu)
    {
        ramText.text = $"Used RAM: {ram}";
        cpuText.text = $"Used CPU: {cpu}";
    }
}