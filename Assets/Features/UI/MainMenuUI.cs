using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
	public Button exitButton;

	private void Start()
	{
		exitButton.onClick.AddListener(() =>
		{
		#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false; 
		#else
							Application.Quit();
		#endif
		});
	}
}
