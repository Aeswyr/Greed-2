using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class InteractableData : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<PlayerController> action;
	[SerializeField] private bool isHoldInteract;
	[SerializeField] private UnityEvent<PlayerController> previewAction;
	[SerializeField] private GameObject interactWheel;
	[SerializeField] private Image interactWheelBar;

	void Start()
	{
		if (isHoldInteract)
		{
			ToggleInteractionWheel(false);
		}
	}

	public void FireInteraction(PlayerController target)
	{
		action?.Invoke(target);
	}

	public void FirePreview(PlayerController target)
	{
		previewAction?.Invoke(target);
	}

	public bool IsHoldInteract()
	{
		return isHoldInteract;
	}

	public void UpdateInteractionWheel(float fillRatio)
	{
		interactWheelBar.fillAmount = fillRatio;
	}

	public void ToggleInteractionWheel(bool enabled)
	{
		interactWheel.SetActive(enabled);
		UpdateInteractionWheel(0);
	}
}
