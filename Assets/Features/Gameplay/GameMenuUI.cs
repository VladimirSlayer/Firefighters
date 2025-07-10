using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Features.UI
{
    public class GameMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private TMP_InputField joinCodeInputText;
        [SerializeField] private Button exitButton;

        private bool isVisible = false;

        private void Start()
        {
            menuPanel.SetActive(false);


            joinCodeInputText.text = $"{Features.Networking.RelayManager.JoinCode}";


            exitButton.onClick.AddListener(ExitToMenu);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }

        private void ToggleMenu()
        {
            isVisible = !isVisible;
            menuPanel.SetActive(isVisible);

            if (isVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }


        private void ExitToMenu()
        {
            if (NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.Shutdown();
            else if (NetworkManager.Singleton.IsClient)
                NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene("MainMenu");
        }
    }
}
