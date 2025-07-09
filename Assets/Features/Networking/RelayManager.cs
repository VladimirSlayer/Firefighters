using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;

namespace Features.Networking
{
    public class RelayManager : MonoBehaviour
    {
        public static string JoinCode { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        }

        private async void Start()
        {
            int guard = 100;
            while (NetworkManager.Singleton == null && guard-- > 0)
                await Task.Yield();

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton is still null!");
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }


        private void OnDestroy()
        {
            if (NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        public async Task StartHostAsync()
        {
            await InitServicesAndRelayAsync(true);
            NetworkManager.Singleton.StartHost();
        }

        public async Task JoinAsync(string joinCode)
        {
            await InitServicesAndRelayAsync(false, joinCode);
            NetworkManager.Singleton.StartClient();
        }

        private async Task InitServicesAndRelayAsync(bool host, string joinCode = "")
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (host)
            {
                Allocation alloc = await Relay.Instance.CreateAllocationAsync(4);
                JoinCode = await Relay.Instance.GetJoinCodeAsync(alloc.AllocationId);
                Debug.Log($"[Relay] Join Code: {JoinCode}");

                transport.SetRelayServerData(new RelayServerData(alloc, "udp"));
            }
            else
            {
                JoinAllocation alloc = await Relay.Instance.JoinAllocationAsync(joinCode);
                transport.SetRelayServerData(new RelayServerData(alloc, "udp"));
            }
        }

        private void OnServerStarted()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) &&
                client.PlayerObject != null)
            {
                client.PlayerObject.transform.position = GetSpawnPosition();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
                return;

            if (clientId != NetworkManager.Singleton.LocalClientId)
                return;

            Debug.Log("[Client] Disconnected from host. Returning to main menu.");

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu");
        }



        private void OnSceneLoaded(ulong _, string sceneName, LoadSceneMode __)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (sceneName != "GameScene") return;

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                var po = kvp.Value.PlayerObject;
                if (po != null) po.transform.position = GetSpawnPosition();
            }
        }

        private Vector3 GetSpawnPosition() =>
            new Vector3(Random.Range(-4f, 4f), 1.2f, Random.Range(-4f, 4f));
    }
}
