using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Features.UI;
using System;

namespace Features.UI
{
    public class GameMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject tipsPanel;
        [SerializeField] private Toggle tipsToggle;
		[SerializeField] private TMP_InputField joinCodeInputText;
        [SerializeField] private Button exitButton;

        private SystemActions input;
        private bool isVisible = false;

		private void Update()
		{
			tipsPanel.SetActive(tipsToggle.isOn);
		}

		private void Awake()
        {
            input = new SystemActions();
            input.UI.ToggleMenu.performed += ctx =>
            {
                Debug.Log("ToggleMenu action triggered!");
                ToggleMenu();
			};
			input.UI.Enable();
        }

        private void Start()
        {
            menuPanel.SetActive(false);
            joinCodeInputText.text = $"{Features.Networking.RelayManager.JoinCode}";
            exitButton.onClick.AddListener(ExitToMenu);
		}

        private void OnDestroy()
        {
            input.UI.ToggleMenu.performed -= ctx => ToggleMenu();
			input.UI.Disable();
        }

        private void ToggleMenu()
        {
            isVisible = !isVisible;
            menuPanel.SetActive(isVisible);

            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isVisible;

            UIStateEvents.ToggleGameMenu(isVisible);
        }


        private void ExitToMenu()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
                NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene("MainMenu");
        }
    }
}
