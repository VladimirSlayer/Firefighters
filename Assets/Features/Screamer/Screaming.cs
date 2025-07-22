using UnityEngine;

public class Screaming : MonoBehaviour
{
	[Header("Sounds")]
	public AudioSource scream;

	//[Header("Scream area")]
	//public Collider ScreamArea;

	private void OnTriggerEnter(Collider other)
    {
		if (other.CompareTag("Player")) //���� ������������ ��������� � �������� ������� ��� "player"
		{
			scream.Play();
		}
	}
}
