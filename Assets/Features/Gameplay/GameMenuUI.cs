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
        [SerializeField] private TMP_Text joinCodeText;
        [SerializeField] private Button exitButton;

        private bool isVisible = false;

        private void Start()
        {
            menuPanel.SetActive(false);

            // Присваиваем Join Code
            joinCodeText.text = $"Join Code: {Features.Networking.RelayManager.JoinCode}";

            // Назначаем действие кнопке выхода
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

            // Вернуться в главное меню
            SceneManager.LoadScene("MainMenu");
        }
    }
}
