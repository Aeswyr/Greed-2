using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
	private InputHandler input;

	[SerializeField]
	private GameObject menuParent;

	private void Start()
	{
		input = FindAnyObjectByType<InputHandler>();
		menuParent.SetActive(value: false);
	}

	private void FixedUpdate()
	{
		if (input.menu.pressed)
		{
			menuParent.SetActive(!menuParent.activeSelf);
		}
	}

	public void ReturnToMenu()
	{
		GameManager.Instance.CleanupGame();
	}

	public void Quit()
	{
		Application.Quit();
	}
}
