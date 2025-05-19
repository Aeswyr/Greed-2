using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
	[SerializeField]
	private GameObject interactIcon;

	[SerializeField]
	private CanvasGroup moneyGroup;

	[SerializeField]
	private TextMeshProUGUI moneyText;

	[SerializeField]
	private CanvasGroup staminaGroup;

	[SerializeField]
	private Image staminaBar;

	[SerializeField]
	private CanvasGroup healthGroup;

	[SerializeField]
	private Image healthBar;

	[SerializeField]
	private CanvasGroup skillGroup;

	[SerializeField]
	private Image skillBar;

	[SerializeField]
	private CanvasGroup crownGroup;

	[SerializeField]
	private TextMeshProUGUI crownCount;
	[SerializeField]
	private TextMeshProUGUI nameplate;

	private bool moneyLock;

	private bool healthFull;

	private float moneyFade;
	private float nameplateFade;

	private float staminaFade;

	private float skillFade;

	private float healthFade;

	private int currentMoney;

	private int targetMoney;

	private void Start()
	{
		moneyGroup.alpha = 0f;
		staminaGroup.alpha = 0f;
		healthGroup.alpha = 0f;
		skillGroup.alpha = 0f;
		crownGroup.alpha = 0f;
		nameplate.alpha = 0f;
		interactIcon.SetActive(value: false);
	}

	public void UpdateMoney(int target)
	{
		if (targetMoney != target)
		{
			moneyGroup.alpha = 1f;
			targetMoney = target;
			moneyFade = Time.time + 1f;
		}
	}

	public void UpdateStamina(float ratio)
	{
		if (ratio < 1f)
		{
			staminaGroup.alpha = 1f;
			staminaBar.fillAmount = ratio;
			staminaFade = Time.time + 0.2f;
		}
	}

	public void UpdateSkill(float ratio)
	{
		if (ratio < 1f)
		{
			skillGroup.alpha = 1f;
			skillBar.fillAmount = ratio;
			skillFade = Time.time + 0.2f;
		}
	}

	public void SetMoneyLock(bool val)
	{
		moneyLock = val;
		if (val)
		{
			moneyGroup.alpha = 1f;
		}
		else
		{
			moneyGroup.alpha = 0f;
		}
	}

	public void UpdateCrowns(int count)
	{
		if (count > 0)
		{
			crownGroup.alpha = 1f;
			crownCount.text = count.ToString();
		}
		else
		{
			crownGroup.alpha = 0f;
		}
	}

	public void UpdateHealth(int health, int max)
	{
		healthGroup.alpha = 1f;
		healthBar.fillAmount = (float)health / (float)max;
		healthFull = health >= max;
		healthFade = Time.time + 0.2f;
	}

	public void SetInteractActive(bool val)
	{
		interactIcon.SetActive(val);
		if (val)
		{
			Animator component = interactIcon.GetComponent<Animator>();
			switch (FindAnyObjectByType<InputHandler>().activeDevice)
			{
			case DeviceType.KEYBOARD:
				component.Play("keyboard");
				break;
			case DeviceType.GAMEPAD:
				component.Play("controller");
				break;
			}
		}
	}

	public void SetNameplate(string name) {
		nameplate.text = name;
	}

	public void UpdateNameplate() {
		nameplate.alpha = 1;
		nameplateFade = Time.time + 2f;
	}

	private void FixedUpdate()
	{
		if (currentMoney != targetMoney)
		{
			int num = Mathf.Max(1, Mathf.Abs(targetMoney - currentMoney) / 10) * ((currentMoney < targetMoney) ? 1 : (-1));
			currentMoney += num;
			moneyText.text = currentMoney.ToString();
			moneyFade = Time.time + 1f;
		}
		else if (!moneyLock && Time.time > moneyFade && currentMoney == targetMoney && moneyGroup.alpha > 0f)
		{
			moneyGroup.alpha -= 0.02f;
		}
		if (Time.time > skillFade && skillGroup.alpha > 0f)
		{
			skillGroup.alpha -= 0.04f;
		}
		if (Time.time > staminaFade && staminaGroup.alpha > 0f)
		{
			staminaGroup.alpha -= 0.04f;
		}
		if (healthFull && Time.time > healthFade && healthGroup.alpha > 0f)
		{
			healthGroup.alpha -= 0.04f;
		}
		if (Time.time > nameplateFade && nameplate.alpha > 0) {
			nameplate.alpha -= 0.02f;
		}
	}
}
