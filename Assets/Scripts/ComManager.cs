using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _raquetaPrefab;
    [SerializeField] private NetworkPrefabRef _bolaPrefab;
    [SerializeField] private NetworkPrefabRef _panelPrefab;
    [SerializeField] private NetworkPrefabRef _scorePrefab;
    [SerializeField] private NetworkPrefabRef _livesPrefab;
    [SerializeField] private List<NetworkPrefabRef> _ladrillosPrefab;

    float ladrillosOffsetX = -7.5f;
    float ladrillosOffsetY = 20f;

    public KeyValuePair<PlayerRef, NetworkObject>? host;
    public KeyValuePair<PlayerRef, NetworkObject>? client;

    private NetworkRunner _runner; //Atributo interno privado donde descansa el NetworkRunner

    async void StartGame(GameMode mode)
    {
        //    // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>(); _runner.ProvideInput = true;
        //    // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs() {
            GameMode = mode,
            SessionName = null,
            DisableClientSessionCreation = true,
            PlayerCount = 2,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }


    private void OnGUI()
    {
        if (_runner == null) {
            if (GUI.Button(new Rect(350, 200, 190, 50), "Host")) StartGame(GameMode.Host);
            if (GUI.Button(new Rect(350, 260, 190, 50), "Join")) StartGame(GameMode.Client);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer) {
            if (host == null) {
                runner.Spawn(_scorePrefab,
                    new Vector3(-4, 31, -1.5f));

                runner.Spawn(_livesPrefab,
                     new Vector3(4, 31, -1.5f));

                runner.Spawn(_bolaPrefab,
                    new Vector3(0, 4, -1.5f), null, player);

                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 7; j++) {
                        runner.Spawn(_ladrillosPrefab[i],
                            new Vector3(ladrillosOffsetX + 2.5f * j,
                            ladrillosOffsetY - 1.5f * i, -1.5f));
                    }
                }

                var raqueta = runner.Spawn(_raquetaPrefab,
                    new Vector3(0, 3, -1.5f), null, player);
                host = new KeyValuePair<PlayerRef, NetworkObject>(player, raqueta);
            }
            else {
                var raqueta = runner.Spawn(_raquetaPrefab,
                    new Vector3(0, 27, -1.5f), null, host.Value.Key);
                client = new KeyValuePair<PlayerRef, NetworkObject>(player, raqueta);
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Find and remove the players avatar
        if (client != null && client.Value.Key == player) {
            runner.Despawn(client.Value.Value);
            client = null;
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();
        data.direction = Vector3.positiveInfinity;

        if (Input.GetKey(KeyCode.A)) data.direction = Vector3.left;
        else if (Input.GetKey(KeyCode.D)) data.direction = Vector3.right;
        else if (Input.GetKey(KeyCode.S)) data.direction = Vector3.zero;
        else if (Input.GetKey(KeyCode.Space)) data.direction = Vector3.negativeInfinity;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
