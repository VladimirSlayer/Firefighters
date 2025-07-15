using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Features.Networking
{
    public class NetworkUI : MonoBehaviour
    {
        public Button hostButton;
        public Button joinButton;
        public TMP_InputField joinCodeInput;
        public TMP_Text joinCodeDisplay;

        private RelayManager relayManager;

        private void Start()
        {
            relayManager = FindObjectOfType<RelayManager>();

            hostButton.onClick.AddListener(async () =>
            {
                await relayManager.StartHostAsync();
                if (joinCodeDisplay != null)
                    joinCodeDisplay.text = $"Join Code: {RelayManager.JoinCode}";
            });

            joinButton.onClick.AddListener(async () =>
            {
                await relayManager.JoinAsync(joinCodeInput.text);
            });
        }
    }
}
