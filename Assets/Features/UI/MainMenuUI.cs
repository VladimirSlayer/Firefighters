using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Networking;

public class MenuUI : MonoBehaviour
{
	public Button exitButton;

	private void Start()
	{
		exitButton.onClick.AddListener(async () =>
		{
		#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false; // ��������� � ���������
		#else
							Application.Quit(); // ����� �� ��������� ������
		#endif
		});
	}
}
