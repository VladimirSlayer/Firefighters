
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay; 

namespace Features.Networking
{
    public class RelayManager : MonoBehaviour
    {
        public static string JoinCode { get; private set; }

        private const string GameScene = "GameScene";
        private const string MainMenu  = "MainMenu";

        private bool _sessionActive;
        private bool _disconnectHandled;
        private bool _intentionalShutdown;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!_sessionActive || _disconnectHandled) return;

            var nm = NetworkManager.Singleton;
            if (nm == null || nm.IsServer || !nm.IsClient) return;

            if (!nm.IsConnectedClient)
                HandleDisconnect();
        }

        public async Task StartHostAsync()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(4);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log($"[Relay] Host join code: {JoinCode}");

            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayData = AllocationUtils.ToRelayServerData(alloc, "udp");
            utp.SetRelayServerData(relayData);

            var nm = NetworkManager.Singleton;
            nm.StartHost();

            await WaitForSceneManager();
            nm.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);

            Subscribe();
            _sessionActive = true;
            nm.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
        }

        public async Task JoinAsync(string joinCode)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            JoinAllocation alloc;
            try
            {
                alloc = await RelayService.Instance.JoinAllocationAsync(joinCode.Trim());
            }
            catch (RelayServiceException e) when (e.Message.Contains("404"))
            {
                Debug.LogError($"[Relay] Некорректный или истёкший код: «{joinCode}»");
                return;
            }

            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayData = AllocationUtils.ToRelayServerData(alloc, "udp");
            utp.SetRelayServerData(relayData);

            var nm = NetworkManager.Singleton;
            var tcs = new TaskCompletionSource<bool>();
            void Handler(ulong clientId)
            {
                if (clientId == nm.LocalClientId)
                {
                    nm.OnClientConnectedCallback -= Handler;
                    tcs.TrySetResult(true);
                }
            }
            nm.OnClientConnectedCallback += Handler;

            nm.StartClient();

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            if (completed != tcs.Task)
            {
                Debug.LogError("[Relay] Не удалось подключиться к хосту за 5 секунд.");
                return;
            }

            await WaitForSceneManager();
            Subscribe();
            _sessionActive = true;
        }

        private async Task WaitForSceneManager()
        {
            int guard = 100;
            while ((NetworkManager.Singleton == null ||
                   NetworkManager.Singleton.SceneManager == null) && guard-- > 0)
            {
                await Task.Delay(50);
            }
            if (NetworkManager.Singleton?.SceneManager == null)
                Debug.LogError("NetworkManager.SceneManager так и не инициализировался");
        }

        private void Subscribe()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;

            nm.OnServerStarted            += OnServerStarted;
            nm.OnClientConnectedCallback  += OnClientConnected;
            nm.OnClientDisconnectCallback += OnClientDisconnected;
            nm.OnClientStopped            += OnClientStopped;
        }

        private void Unsubscribe()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;

            nm.OnServerStarted            -= OnServerStarted;
            nm.OnClientConnectedCallback  -= OnClientConnected;
            nm.OnClientDisconnectCallback -= OnClientDisconnected;
            nm.OnClientStopped            -= OnClientStopped;
        }

        private void OnServerStarted()
        {
            if (NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[Relay] Client connected: {clientId}");

            if (NetworkManager.Singleton.IsServer)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
                {
                    var po = clientData.PlayerObject;
                    if (po != null)
                    {
                        po.transform.position = new Vector3(
                            UnityEngine.Random.Range(-4f, 4f),
                            1.2f,
                            UnityEngine.Random.Range(-4f, 4f)
                        );
                        Debug.Log($"[Relay] Set start position for client {clientId}");
                    }
                }
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[Relay] Client disconnected: {clientId}");

            if (NetworkManager.Singleton.IsServer)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
                {
                    var netObj = clientData.PlayerObject;
                    if (netObj != null && netObj.IsSpawned)
                    {
                        netObj.Despawn(destroy: true);
                        Debug.Log($"[Relay] Despawned player object for client {clientId}");
                    }
                }
            }

            if (clientId == NetworkManager.Singleton.LocalClientId)
                HandleDisconnect();
        }

        private void OnClientStopped(bool _)
        {
            if (_intentionalShutdown)
            {
                Debug.Log("[Relay] OnClientStopped (expected)");
                return;
            }
            Debug.Log("[Relay] OnClientStopped (unexpected)");
            HandleDisconnect();
        }

        private void HandleDisconnect()
        {
            if (_disconnectHandled) return;
            _disconnectHandled = true;
            _intentionalShutdown = true;
            StartCoroutine(DisconnectRoutine());
        }

        private IEnumerator DisconnectRoutine()
        {
            NetworkManager.Singleton?.Shutdown();
            yield return null;
            SceneManager.LoadScene(MainMenu);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }
}
