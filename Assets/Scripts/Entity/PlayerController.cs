using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
	private enum Stat
	{
		HEALTH,
		SKILL,
		POWER,
		SPEED,
		STAMINA
	}

	private enum InvulnState
	{
		NONE = 0,
		DODGE = 1,
		HITSTUN = 2,
		PARRY = 3,
		ARMOR = 4,
		GENERIC = 5
	}

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private InputHandler input;

	[SerializeField]
	private MovementHandler move;

	[SerializeField]
	private JumpHandler jump;

	[SerializeField]
	private GroundedCheck ground;

	[SerializeField]
	private UnitVFXController unitVFX;

	[SerializeField]
	private PlayerUIController unitUI;

	[SerializeField]
	private HurtboxController hurtbox;
	[SerializeField]
	private CircleCollider2D pickupBox;

	[SerializeField]
	private InteractboxController interactBox;
	[SerializeField]
	private Animator chargeGlintAnimator;

	[SerializeField]
	private MaterialLibrary colors;
	[SerializeField]
	private IntegerLibrary cooldowns;
	[Header("Action data")]
	[SerializeField]
	private float wallJumpDuration;

	[SerializeField]
	private float knockbackDuration;

	[SerializeField]
	private float knockbackSpeed;

	[SerializeField]
	private AnimationCurve knockbackCurve;

	[SerializeField]
	private float dodgeSpeed;

	[SerializeField]
	private AnimationCurve dodgeCurve;

	[SerializeField]
	private float pickAttackSpeed;

	[SerializeField]
	private AnimationCurve pickAttackCurve;
	[SerializeField]
	private float pickMineSpeed;

	[SerializeField]
	private AnimationCurve pickMineCurve;

	[SerializeField]
	private float swordAttackSpeed;

	[SerializeField]
	private AnimationCurve swordAttackCurve;

	[SerializeField]
	private float swordLungeSpeed;

	[SerializeField]
	private AnimationCurve swordLungeCurve;

	[SerializeField]
	private float unarmedAttackSpeed;

	[SerializeField]
	private AnimationCurve unarmedAttackCurve;

	[SerializeField]
	private float unarmedShortSpeed;

	[SerializeField]
	private AnimationCurve unarmedShortCurve;
	[SerializeField]
	private float unarmedStanceSpeed;

	[SerializeField]
	private AnimationCurve unarmedStanceCurve;
	[SerializeField]
	private float unarmedBlinkSpeed;
	[SerializeField]
	private AnimationCurve unarmedBlinkCurve;
	[SerializeField]
	private float unarmedUpperSpeed;
	[SerializeField]
	private AnimationCurve unarmedUpperCurve;

	[SerializeField]
	private float shieldAttackSpeed;

	[SerializeField]
	private AnimationCurve shieldAttackCurve;

	[SerializeField]
	private float shieldParrySpeed;

	[SerializeField]
	private AnimationCurve shieldParryCurve;

	[SerializeField]
	private float greatweaponAttackSpeed;

	[SerializeField]
	private AnimationCurve greatweaponAttackCurve;

	[SerializeField]
	private float greatweaponReleaseSpeed;

	[SerializeField]
	private AnimationCurve greatweaponReleaseCurve;

	[SerializeField]
	private float bowAttackSpeed;

	[SerializeField]
	private AnimationCurve bowAttackCurve;

	[SerializeField]
	private float bowReleaseSpeed;

	[SerializeField]
	private AnimationCurve bowReleaseCurve;
	[SerializeField]
	private float tomeAttackSpeed;

	[SerializeField]
	private AnimationCurve tomeAttackCurve;

	[SerializeField]
	private float tomeReleaseSpeed;

	[SerializeField]
	private AnimationCurve tomeReleaseCurve;
	[SerializeField]
	private float chainAttackSpeed;

	[SerializeField]
	private AnimationCurve chainAttackCurve;

	[SerializeField]
	private float chainReleaseSpeed;

	[SerializeField]
	private AnimationCurve chainReleaseCurve;
	[SerializeField]
	private float chainAltReleaseSpeed;

	[SerializeField]
	private AnimationCurve chainAltReleaseCurve;

	[SerializeField]
	private float chainPullSpeed;

	[SerializeField]
	private AnimationCurve chainPullCurve;
	[SerializeField]
	private GameObject turretPrefab;

	private float staminaCooldown => 2.5f * staminaMod * (HasBuff(BuffType.GHOSTFORM) ? 0.5f : 1);
	private float skillCooldown => skillId == -1 ? 1 : cooldowns[skillId] * skillMod;

	private int currentColor = -1;

	private int _money;
	private int money
	{
		get { return _money; }
		set
		{
			victoryStats.MoneyHeld = value;
			if (value > money)
				victoryStats.MoneyCollected += value - _money;

			_money = value;
		}
	}
	private int crowns;

	private int health;
	private int maxHealth => 100 + healthMod;
	private bool healthEmptied;

	private float nextStamina;
	private float nextSkill;

	private float endHitstop;
	private bool isHitstop;

	private bool grounded;
	private int facing = 1;
	private Vector3 aim;

	private bool acting;
	private bool attackCancel;
	private bool attacking;

	private float hitStunTime;
	private bool hitStun;

	private int attackId = -1;
	private int weaponId = 0; //0
	private int skillId = -1; //-1

	private bool stasis;
	private bool inputLocked;

	private float wallCoyote;
	private int wallDir;
	private bool isWallJumping;
	private float wallJump;
	private bool wallHanging;

	private InvulnState invuln;

	private float endFlight;
	private bool flying;

	private bool charging;
	private float chargeStart;
	private bool chargeGlintReady;
	private float tomeChargeLength = 0.5f;
	private float chainChargeLength = 0.75f;

	private VictoryStats victoryStats;

	private int[] stats = new int[5];
	private float[] buffs = new float[(int)BuffType.RANDOM];
	private bool[] buffDirty = new bool[(int)BuffType.RANDOM];
	private LayerMask worldMask;
	[SyncVar] private int playerId = -1;
	public int PlayerID => playerId;
	private bool hasAmmo = true;
	private int unarmedInstall;

	private int healthMod => 25 * stats[(int)Stat.HEALTH];
	private float skillMod => 1 - Utils.LogisticFunc(stats[(int)Stat.SKILL], 1.5f, 0.4f, 0, 0.75f);
	private float staminaMod => 1 - Utils.LogisticFunc(stats[(int)Stat.STAMINA], 1f, 0.4f, 0, 0.5f);
	private float speedMod => 1f * stats[(int)Stat.SPEED];
	private float speedMult => 1 + (HasBuff(BuffType.SWIFT) ? 0.5f : 0) + (HasBuff(BuffType.GHOSTFORM) ? 0.2f : 0) - (HasBuff(BuffType.BARRIER) ? 0.15f : 0);
	private int powerMod => 30 * stats[(int)Stat.POWER];

	private ScoreCard scorecard;

	new public bool isLocalPlayer => base.isLocalPlayer || GameManager.Instance.IsLocalGame;

	[SyncVar]
	private ulong? friendId = null;
	private string playerProfileName;

	private PurchaseInterface confirmedShop;

	private float nextFootstep;

	private void Start()
	{
		health = maxHealth;
		worldMask = LayerMask.GetMask("World");
		sprite.material = colors[0];
		if (isLocalPlayer)
		{
			if (input == null)
			{
				SetupInput(FindAnyObjectByType<InputHandler>());
			}
			victoryStats.Luck = Random.Range(0, 100);
			SyncFriendID(SteamUser.GetSteamID().m_SteamID);

			[Command(requiresAuthority = false)] void SyncFriendID(ulong id)
			{
				friendId = id;
			}
		}

		if (GameManager.Instance.IsLocalGame && isClient)
		{
			GrantAuthority();
		}

		[Command(requiresAuthority = false)] void GrantAuthority()
		{
			PlayerController[] array = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
			NetworkConnectionToClient conn = null;
			foreach (var player in array)
			{
				if (player.connectionToClient != null)
				{
					conn = player.connectionToClient;
				}
			}
			this.netIdentity.AssignClientAuthority(conn);
		}

		StartCoroutine(SetupNameplate());

		IEnumerator SetupNameplate()
		{
			yield return new WaitUntil(() => friendId.HasValue);
			unitUI.SetNameplate(GetDisplayName());
		}

	}

	public void SetupInput(InputHandler input)
	{
		this.input = input;
		jump.SetInput(input);
		GameManager.Instance.AddLobbyCard(this);
	}

	public void SetupScorecard(ScoreCard card)
	{
		this.scorecard = card;
		if (currentColor != -1)
			scorecard.SetColor(currentColor);

		if (isLocalPlayer)
		{
			SyncName();
		}

		[Command(requiresAuthority = false)] void SyncName()
		{
			RecieveName();
		}

		[ClientRpc] void RecieveName()
		{
			StartCoroutine(WaitUntilCardReady());
			IEnumerator WaitUntilCardReady()
			{
				yield return new WaitUntil(() => scorecard != null && friendId.HasValue);

				scorecard.SetName(GetDisplayName());
			}
		}
	}

	[Server]
	public void AssignId(int id)
	{
		playerId = id;
	}

	private void FixedUpdate()
	{
		unitUI.UpdateStamina(1f - (nextStamina - Time.time) / staminaCooldown);
		unitUI.UpdateSkill(1f - (nextSkill - Time.time) / skillCooldown);
		DoBuffCleanup();
		if (!isLocalPlayer || inputLocked)
		{
			return;
		}
		grounded = ground.CheckGrounded();
		if (hitStun && Time.time > hitStunTime)
		{
			EndAction();
			animator.SetBool("hurt", value: false);
			hitStun = false;
			if (healthEmptied)
			{
				health = maxHealth;
				UpdateHealthDisplay(health, maxHealth);
				healthEmptied = false;
			}
		}
		if (hitStun && healthEmptied)
		{
			health = (int)((1f - (hitStunTime - Time.time) / knockbackDuration) * (float)maxHealth);
			UpdateHealthDisplay(health, maxHealth);
		}
		if (isHitstop && Time.time > endHitstop)
		{
			isHitstop = false;
			animator.speed = 1f;
		}
		if (flying && Time.time > endFlight)
		{
			EndFlight();
		}
		if (charging && chargeGlintReady)
		{
			switch (weaponId)
			{
				case 6:
					if (Time.time - chargeStart > tomeChargeLength)
					{
						chargeGlintReady = false;
						chargeGlintAnimator.Play("sparkle", -1, 0);
					}
					break;
				case 7:
					if (Time.time - chargeStart > chainChargeLength)
					{
						chargeGlintReady = false;
						chargeGlintAnimator.Play("sparkle", -1, 0);
					}
					break;
				default:
					break;

			}
		}
		HandleInputs();
	}

	public void HandleInputs()
	{
		aim = input.aim;
		aim.x = aim.magnitude == 0f ? facing : input.dir;
		aim.Normalize();
		if (isWallJumping && Time.time > wallJump)
		{
			isWallJumping = false;
			if (!input.move.down)
			{
				move.StartDeceleration();
			}
		}
		if (!acting && input.interact.pressed)
		{
			interactBox.FireInteraction();
		}
		if (!acting && isWallJumping)
		{
			if (wallDir + (int)input.dir == 0)
			{
				move.OverrideSpeed(move.GetMaxSpeed() - 1f);
			}
			move.UpdateMovement(wallDir);
			UpdateFacing(wallDir);
		}
		else if (!acting && input.move.pressed)
		{
			move.StartAcceleration(input.dir);
			UpdateFacing(input.dir);
		}
		else if (!acting && input.move.down)
		{
			move.UpdateMovement(input.dir);
			UpdateFacing(input.dir);

			if (Time.time > nextFootstep && grounded)
			{
				nextFootstep = Time.time + 0.35f;
				SFXManager.Instance.PlaySound("step");
			}
		}
		else if (!acting && input.move.released)
		{
			move.StartDeceleration();
		}
		if (attacking)
		{
			int num = attackId;
			int num2 = num;
			if (num2 == 4)
			{
				move.UpdateMovement(facing);
			}
		}
		if (Time.time < endFlight && !acting)
		{
			jump.ForceVelocity(input.aim.y * 12f);
		}
		if (!acting && !grounded && input.move.down && (bool)Utils.Boxcast(transform.position + 0.75f * Vector3.down, new Vector2(1f, 2.4f), facing * Vector2.right, 1f, worldMask))
		{
			animator.SetBool("wall", value: true);
			wallDir = facing * -1;
			wallCoyote = Time.time + 0.15f;
			jump.SetTerminalVelocity(1f);
			wallHanging = true;

			if (isWallJumping)
			{
				isWallJumping = false;
				wallJump = 0;
				if (!input.move.down)
				{
					move.StartDeceleration();
				}
			}
		}
		else if (wallHanging)
		{
			animator.SetBool("wall", value: false);
			jump.ResetTerminalVelocity();
			wallHanging = false;
		}
		if ((!acting || (charging && Time.time > nextStamina)) && (grounded || Time.time < wallCoyote || Time.time > nextStamina) && input.jump.pressed)
		{
			jump.StartJump();
			animator.SetTrigger("jump");
			SFXManager.Instance.PlaySound("jump");
			wallJump = 0f;
			isWallJumping = false;
			if (Time.time < wallCoyote)
			{
				isWallJumping = true;
				wallJump = Time.time + wallJumpDuration;
				move.StartAcceleration(wallDir);
			}
			else if (!grounded || charging)
			{
				nextStamina = Time.time + staminaCooldown;
				UpdateDodgeDisplay(staminaCooldown);
				if (charging)
				{
					InterruptCharge();
					EndAction();
					if (input.dir != 0)
						move.StartAcceleration(input.dir);
				}
			}
		}
		if ((!acting || (charging && Time.time > nextStamina)) && Time.time > nextStamina && grounded && input.dodge.pressed)
		{
			if (charging)
			{
				InterruptCharge();
				EndAction();
			}

			SFXManager.Instance.PlaySound("slide");
			StartAction();
			nextStamina = Time.time + staminaCooldown;
			UpdateDodgeDisplay(staminaCooldown);
			UpdateFacing(input.dir);
			animator.SetTrigger("dodge");
			move.OverrideCurve(CalculateSpeed(dodgeSpeed), dodgeCurve, facing);
			invuln = InvulnState.DODGE;
			VFXManager.Instance.SyncVFX(ParticleType.DUST_LARGE, transform.position, facing == -1);
			unitVFX.StartAfterImageChain(0.5f, 0.1f);
		}
		if ((!acting || attackCancel) && input.attack.pressed)
		{
			if (attackCancel)
				OnAttackCancel();

			StartAction();
			UpdateFacing(input.dir);
			PressAttack();
		}
		if (charging && input.attack.released)
		{
			ReleaseAttack();
		}
		if ((!acting || attackCancel) && skillId != -1 && Time.time > nextSkill && input.item.pressed)
		{
			if (attackCancel)
				OnAttackCancel();

			StartAction();
			UpdateFacing(input.dir);
			PressSkill();
			nextSkill = Time.time + skillCooldown;
			UpdateSkillDisplay(skillCooldown);
		}
		if (input.item.released)
		{
			ReleaseSkill();
		}
		animator.SetBool("grounded", grounded);
		animator.SetBool("moving", input.move.down && !acting);
	}

	private void OnAttackCancel()
	{
		EndAction();
	}

	public void StartAction()
	{
		if (isLocalPlayer)
		{
			acting = true;
			wallJump = 0f;
			EndFlight();
			isWallJumping = false;
		}
	}

	public void EndAction()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		acting = false;
		charging = false;
		attackCancel = false;
		if (attacking)
		{
			switch (attackId)
			{
				case 14:
				case 15:
					jump.ResetGravity();
					jump.ResetTerminalVelocity();
					break;

			}
		}
		attacking = false;
		move.ResetCurves();
		invuln = InvulnState.NONE;
		animator.ResetTrigger("release");
		if (!input.move.down || input.move.released)
		{
			move.StartDeceleration();
		}
	}

	public void PressAttack()
	{
		void StartCharge()
		{
			charging = true;
			chargeStart = Time.time;
			chargeGlintReady = true;
		}

		attacking = true;
		switch (weaponId)
		{
			case 0: // pickaxe
				attackId = 0;
				if (input.aim.y < 0f)
				{
					attackId = 11;
				}
				break;
			case 1: // sword
				attackId = 1;
				if (input.aim.y < 0f)
				{
					attackId = 2;
				}
				break;
			case 2: // unarmed

				if (input.aim.y < 0f)
				{
					attackId = 4;
				}
				else
				{
					attackId = 3;
					switch (unarmedInstall)
					{
						case 1:
							attackId = 13;
							break;
						case 2:
							attackId = 14;
							invuln = InvulnState.ARMOR;
							break;
					}
					unarmedInstall = 0;
				}
				break;
			case 3: // shield
				attackId = 5;
				if (input.aim.y < 0f)
				{
					invuln = InvulnState.PARRY;
					attackId = 6;
				}
				break;
			case 4: // greatweapon
				StartCharge();
				attackId = 7;
				break;
			case 5: // bow
				if (hasAmmo)
				{
					StartCharge();
					attackId = 8;
				}
				else
				{
					attackId = 9;
				}
				break;
			case 6: // tome
				StartCharge();
				attackId = 10;
				break;
			case 7: // chain
				StartCharge();
				attackId = 12;
				break;
		}
		animator.SetInteger("attackId", attackId);
		animator.SetTrigger("attack");
		if (grounded)
		{
			VFXManager.Instance.SyncVFX(ParticleType.DUST_SMALL, transform.position, facing == -1);
		}
		switch (attackId)
		{
			case 0:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(pickAttackSpeed), pickAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 1:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(swordAttackSpeed), swordAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 2:
				move.OverrideCurve(CalculateSpeed(swordLungeSpeed), swordLungeCurve, facing);
				break;
			case 3:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(unarmedAttackSpeed), unarmedAttackCurve, facing);
				}
				else if (grounded)
				{
					move.OverrideCurve(CalculateSpeed(unarmedShortSpeed), unarmedShortCurve, facing);
				}
				break;
			case 4:
				move.OverrideCurve(CalculateSpeed(unarmedStanceSpeed), unarmedStanceCurve, facing);
				break;
			case 5:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(shieldAttackSpeed), shieldAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 6:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(shieldParrySpeed), shieldParryCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 7:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(greatweaponAttackSpeed), greatweaponAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 8:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(bowAttackSpeed), bowAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 9:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(shieldAttackSpeed), shieldAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 10:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(tomeAttackSpeed), tomeAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				jump.SetGravity(0.5f);
				jump.ForceVelocity(0);
				jump.SetTerminalVelocity(2);
				break;
			case 11:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(pickMineSpeed), pickMineCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				break;
			case 12:
				if (input.dir != 0f)
				{
					move.OverrideCurve(CalculateSpeed(chainAttackSpeed), chainAttackCurve, facing);
				}
				else if (grounded)
				{
					move.StartDeceleration();
				}
				jump.SetGravity(0.5f);
				jump.ForceVelocity(0);
				jump.SetTerminalVelocity(2);
				break;
			case 13:
				move.OverrideCurve(CalculateSpeed(unarmedBlinkSpeed), unarmedBlinkCurve, facing);
				break;
			case 14:
				move.OverrideCurve(CalculateSpeed(unarmedUpperSpeed), unarmedUpperCurve, facing);
				break;
		}
	}

	public void ReleaseAttack()
	{
		switch (attackId)
		{
			case 7:
				charging = false;
				animator.SetTrigger("release");
				if (input.dir != 0f)
				{
					UpdateFacing(input.dir);
					move.OverrideCurve(CalculateSpeed(greatweaponReleaseSpeed), greatweaponReleaseCurve, facing);
				}
				else
				{
					move.StartDeceleration();
				}
				break;
			case 8:
				charging = false;
				animator.SetTrigger("release");
				CreateAttack();
				if (input.dir != 0f)
				{
					UpdateFacing(input.dir);
				}
				move.OverrideCurve(CalculateSpeed(bowReleaseSpeed), bowReleaseCurve, -facing);
				break;
			case 10:
				charging = false;
				animator.SetTrigger("release");
				CreateAttack();
				if (input.dir != 0f)
				{
					UpdateFacing(input.dir);
				}
				move.OverrideCurve(CalculateSpeed(tomeReleaseSpeed), tomeReleaseCurve, -facing);
				break;
			case 12:
				charging = false;

				if (input.dir != 0f)
				{
					UpdateFacing(input.dir);
				}
				CreateAttack();

				if (Time.time - chargeStart > chainChargeLength)
				{
					animator.SetTrigger("releaseAlt");
					move.OverrideCurve(CalculateSpeed(chainAltReleaseSpeed), chainAltReleaseCurve, facing);
				}
				else
				{

					animator.SetTrigger("release");
					move.OverrideCurve(CalculateSpeed(chainReleaseSpeed), chainReleaseCurve, facing);
				}
				break;
		}
	}

	public void PressSkill()
	{
		switch (skillId)
		{
			case 0:
			case 1:
			case 7:
			case 8:
				SFXManager.Instance.PlaySound("use");
				animator.SetTrigger("skill_self");
				break;
			case 3:
				SFXManager.Instance.PlaySound("use");
				animator.SetTrigger("skill_self");
				invuln = InvulnState.GENERIC;
				break;
			case 2:
			case 4:
			case 5:
			case 6:
			case 9:
				SFXManager.Instance.PlaySound("throw");
				animator.SetTrigger("skill_other");
				break;
		}
		move.Pause(Time.time + 0.5f);
		jump.Pause(Time.time + 0.5f);
		if (grounded)
		{
			move.StartDeceleration();
			VFXManager.Instance.SyncVFX(ParticleType.DUST_SMALL, transform.position, facing == -1);
		}
	}
	public void CreateSkillFX()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		switch (skillId)
		{
			case 3:
				attackCancel = true;
				break;
		}
	}
	public void CreateSkill()
	{
		if (!isLocalPlayer)
		{
			return;
		}
		switch (skillId)
		{
			case 0:
				{
					unitVFX.SetFXState(PlayerVFX.VACCUM, state: true);
					RaycastHit2D[] array = Physics2D.BoxCastAll(transform.position, 18f * Vector2.one, 0f, Vector2.right, 0f, LayerMask.GetMask("Pickup"));
					List<Vector3> list = new List<Vector3>();
					List<Vector3> list2 = new List<Vector3>();
					List<Vector3> list3 = new List<Vector3>();
					List<Vector3> list4 = new List<Vector3>();
					List<Vector3> list5 = new List<Vector3>();
					List<Vector3> list6 = new List<Vector3>();
					List<Vector3> list7 = new List<Vector3>();
					RaycastHit2D[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						RaycastHit2D raycastHit2D3 = array2[i];
						PickupData component = raycastHit2D3.transform.GetComponent<PickupData>();
						switch (component.GetPickupType())
						{
							case PickupType.MONEY_SMALL:
								switch (component.GetPickupVariant())
								{
									case PickupVariant.GOLD:
										list.Add(raycastHit2D3.transform.position);
										break;
									case PickupVariant.SILVER:
										list2.Add(raycastHit2D3.transform.position);
										break;
									case PickupVariant.COPPER:
										list3.Add(raycastHit2D3.transform.position);
										break;
								}
								break;
							case PickupType.MONEY_LARGE:
								switch (component.GetPickupVariant())
								{
									case PickupVariant.GOLD:
										list4.Add(raycastHit2D3.transform.position);
										break;
									case PickupVariant.SILVER:
										list5.Add(raycastHit2D3.transform.position);
										break;
									case PickupVariant.COPPER:
										list6.Add(raycastHit2D3.transform.position);
										break;
								}
								break;
							case PickupType.ITEM_CROWN:
								list7.Add(raycastHit2D3.transform.position);
								break;
						}
						DoPickup(component);
					}
					unitVFX.TriggerPickupFX(list, 0);
					unitVFX.TriggerPickupFX(list2, 1);
					unitVFX.TriggerPickupFX(list3, 2);
					unitVFX.TriggerPickupFX(list4, 3);
					unitVFX.TriggerPickupFX(list5, 4);
					unitVFX.TriggerPickupFX(list6, 5);
					unitVFX.TriggerPickupFX(list7, 6);
					break;
				}
			case 1:
				StartFlight();
				break;
			case 2:
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position)
					.SetAnimation(0)
					.SetVelocity(1.5f * (float)Random.Range(16, 23) * (Quaternion.Euler(0f, 0f, Random.Range(-5, -15)) * aim))
					.SetLifetime(0.29999998f)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.FIREWORK_POP)
					.Finish();
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position)
					.SetAnimation(0)
					.SetVelocity(1.5f * (float)Random.Range(16, 23) * (Quaternion.Euler(0f, 0f, Random.Range(5, 15)) * aim))
					.SetLifetime(0.29999998f)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.FIREWORK_POP)
					.Finish();
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position)
					.SetAnimation(0)
					.SetVelocity(30f * (Quaternion.Euler(0f, 0f, 10f) * aim))
					.SetLifetime(0.29999998f)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.FIREWORK_POP)
					.Finish();
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position)
					.SetAnimation(0)
					.SetVelocity(33f * (Quaternion.Euler(0f, 0f, -5f) * aim))
					.SetLifetime(4f / 15f)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.FIREWORK_POP)
					.Finish();
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position)
					.SetAnimation(0)
					.SetVelocity(37.5f * aim)
					.SetLifetime(4f / 15f)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.FIREWORK_POP)
					.Finish();
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position)
					.SetAnimation(0)
					.SetVelocity(19.5f * (Quaternion.Euler(0f, 0f, 5f) * aim))
					.SetLifetime(1f / 3f)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.FIREWORK_POP)
					.Finish();
				break;
			case 3:
				{
					Vector3 vector = transform.position + 10f * (Vector3)aim.normalized;
					RaycastHit2D raycastHit2D = Physics2D.BoxCast(vector, new Vector2(1.5f, 2.5f), 0f, Vector2.right, 0f, worldMask);
					if (!raycastHit2D && !GameManager.Instance.GetCurrentLevel().IsPointInGeometry(vector))
					{
						transform.position = vector;
					}
					else
					{
						RaycastHit2D raycastHit2D2 = Physics2D.BoxCast(transform.position, new Vector2(1.5f, 2.5f), 0f, aim, 10f, worldMask);
						transform.position = raycastHit2D2.centroid;
					}
					invuln = InvulnState.NONE;
					break;
				}
			case 4:
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(3f, 3f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down)
					.SetAnimation(1)
					.SetVelocity(8f * aim)
					.SetLifetime(4f)
					.DisableEntityImpact()
					.FlipSprite(aim.x < 0f)
					.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
					.Finish();
				break;
			case 5:
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down)
					.SetAnimation(2)
					.SetVelocity(40f * aim)
					.DisableWorldImpact()
					.SetLifetime(5)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
					.SetUnique(UniqueProjectile.DRILL)
					.Finish();
				break;
			case 6:
				ProjectileBuilder.GetProjectile(transform.position)
					.SetOwner(transform)
					.SetAnimation(4)
					.SetVelocity(30f * (Vector2)aim + (aim.y >= 0 ? 20 * Vector2.up : Vector2.zero))
					.SetGravity(10f)
					.DisableEntityImpact()
					.SetUnique(UniqueProjectile.BOMB)
					.Finish();
				break;
			case 7:
				GiveBuff(BuffType.RANDOM);
				break;
			case 8:
				CreateTurret();
				[Command] void CreateTurret()
				{
					var turret = Instantiate(turretPrefab, transform.position, Quaternion.identity, GameManager.Instance.GetLevelObjectRoot());
					turret.GetComponent<TurretController>().SetOwner(transform);
					NetworkServer.Spawn(turret);
				}
				break;
			case 9:
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(2f, 1.5f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down)
					.SetAnimation(10)
					.SetVelocity(20f * aim)
					.RotateWithVelocity()
					.DisableEntityImpact()
					.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
					.SetUnique(UniqueProjectile.BOOMERANG)
					.Finish();
				break;
		}
	}

	public void ReleaseSkill()
	{
	}

	public void OnHitTrigger(Transform source, bool playerHit = false)
	{

		Vector3 position = source == null ? transform.position : source.position;

		if (isServer)
		{
			RecieveTrigger(position, playerHit);
		}
		else
		{
			SendTrigger(position, playerHit);
		}

		[Command] void SendTrigger(Vector3 pos, bool playerHit)
		{
			RecieveTrigger(pos, playerHit);
		}

		[ClientRpc] void RecieveTrigger(Vector3 pos, bool playerHit)
		{
			if (isLocalPlayer)
			{
				if (HasBuff(BuffType.GREED))
				{
					GameManager.Instance.SpawnGemBurst(pos, Random.Range(1, 6));
				}

				if (playerHit)
				{
					victoryStats.HitsLanded++;
				}
			}
		}

	}
	public void OnHit(HitboxData source)
	{
		if (source.FriendlyFire || !(source.Owner == transform))
		{
			Transform parent = source.transform;
			bool flag = false;
			if (parent.parent != null)
			{
				parent = parent.parent;
				flag = true;
			}
			SendHit((int)Mathf.Sign(transform.position.x - parent.position.x), source.transform.position, flag ? parent : null, source.Owner);
		}
		[Command(requiresAuthority = false)]
		void SendHit(int knockbackDir, Vector3 hitPosition, Transform source, Transform owner)
		{
			RecieveHit(knockbackDir, hitPosition, source, owner);
		}
	}

	[Command]
	private void SpoofHit()
	{
		RecieveHit(facing, Vector3.zero, null, null);
	}

	[ClientRpc]
	private void RecieveHit(int knockbackDir, Vector3 hitPosition, Transform source, Transform owner)
	{
		if (!isLocalPlayer || stasis || GameManager.Instance.IsLevelShop())
		{
			return;
		}

		if (source != null && source.TryGetComponent<ProjectileData>(out var p))
		{
			if (hurtbox.HasSeenHitbox(source))
			{
				return;
			}
			hurtbox.MarkHitboxSeen(source);
		}

		if (invuln != 0)
		{
			if (invuln == InvulnState.PARRY)
			{
				animator.SetInteger("attackId", -1);
				animator.SetTrigger("attack");
				CreateAttack();
				invuln = InvulnState.GENERIC;
			}
			if (invuln == InvulnState.ARMOR)
			{
				VFXManager.Instance.SyncVFX(ParticleType.ARMOR, transform.position, facing == -1);
				invuln = InvulnState.NONE;
			}
			if (invuln == InvulnState.DODGE)
			{
				victoryStats.CloseCalls++;
			}
			return;
		}

		if (HasBuff(BuffType.BARRIER))
		{
			VFXManager.Instance.SyncVFX(ParticleType.ARMOR, transform.position, facing == -1);
			EndBuff(BuffType.BARRIER);
			return;
		}

		if (charging)
			InterruptCharge();

		invuln = InvulnState.HITSTUN;
		StartAction();
		UpdateFacing(-knockbackDir);
		animator.SetBool("hurt", value: true);
		hitStun = true;
		hitStunTime = Time.time + knockbackDuration;
		move.OverrideCurve(CalculateSpeed(knockbackSpeed), knockbackCurve, knockbackDir);
		jump.ForceLanding();
		jump.ForceVelocity(30f);
		DoHitstop(0.15f);

		PlayerController attackingPlayer = null;
		if (owner != null)
			owner.TryGetComponent(out attackingPlayer);

		attackingPlayer.OnHitTrigger(source, true);

		if (money > 0)
		{
			int num = Mathf.CeilToInt((float)money / 2f);
			if (attackingPlayer != null
				&& attackingPlayer.HasBuff(BuffType.BLOODLUST))
			{
				num = money;
			}
			DropLoot(num);
			money -= num;
			UpdateMoneyDisplay();
		}
		unarmedInstall = 0;
		if (health > 0 && crowns > 0)
		{
			if (attackingPlayer != null)
			{

				if (attackingPlayer.HasBuff(BuffType.BLOODLUST))
				{
					health = 0;
				}
				else
				{
					health -= 50 + attackingPlayer.powerMod;
				}
			}
			else
			{
				health -= 50;
			}
			UpdateHealthDisplay(health, maxHealth);
			SFXManager.Instance.PlaySound("hurt");
			if (health <= 0)
			{
				VFXManager.Instance.SyncScreenshake(0.2f, 0.3f);
				healthEmptied = true;
				DropCrown();
				crowns--;
				victoryStats.CrownsHeld--;
				UpdateCrownDisplay();
			}
			else
			{
				VFXManager.Instance.SyncScreenshake(0.1f, 0.2f);
			}
		}
		VFXManager.Instance.SyncVFX(ParticleType.HITSPARK, 0.5f * (transform.position + hitPosition), flip: false);
		if (source != null && source.TryGetComponent<ProjectileData>(out var component))
		{
			component.OnEntityCollide();
		}
	}

	[Command(requiresAuthority = false)]
	private void DropLoot(int amt)
	{
		GameManager.Instance.SpawnGoldBurst(transform.position, amt);
	}

	[Command(requiresAuthority = false)]
	private void DropCrown()
	{
		GameManager.Instance.SpawnCrown(transform.position);
	}

	public bool TrySpendMoney(int amt)
	{
		if (!isLocalPlayer)
		{
			return false;
		}
		if (money < amt)
		{
			return false;
		}
		money -= amt;
		UpdateMoneyDisplay();
		victoryStats.MoneySpent += amt;
		return true;
	}

	public bool TryAddMoney(int amt)
	{
		if (!isLocalPlayer)
		{
			return false;
		}
		money += amt;
		UpdateMoneyDisplay();
		return true;
	}

	public void UpdateFacing(float dir)
	{
		int num = facing;
		if (dir > 0f)
		{
			facing = 1;
		}
		else if (dir < 0f)
		{
			facing = -1;
		}
		if (num != facing)
		{
			bool flipX = facing < 0;
			sprite.flipX = flipX;
			SendFacing(flipX);
		}
		[Command]
		void SendFacing(bool facing)
		{
			RecieveFacing(facing);
			[ClientRpc]
			void RecieveFacing(bool facing)
			{
				sprite.flipX = facing;
			}
		}
	}

	public void DoPickup(PickupData pickup)
	{
		if (isLocalPlayer && invuln != InvulnState.HITSTUN && pickup.CanPickup())
		{
			SFXManager.Instance.PlaySound("pickup", SFXManager.AudioMode.SINGLE);
			victoryStats.ThingsCollected++;
			GetItem(pickup.GetPickupType());
			unitVFX.SetFXState(PlayerVFX.PICKUP, true);
			pickup.OnPickup();
		}
	}

	public void GetItem(PickupType type)
	{
		switch (type)
		{
			case PickupType.MONEY_SMALL:
				money++;
				UpdateMoneyDisplay();
				break;
			case PickupType.MONEY_LARGE:
				money += 10;
				UpdateMoneyDisplay();
				break;
			case PickupType.MONEY_BONUS:
				money += 5;
				UpdateMoneyDisplay();
				break;
			case PickupType.ITEM_CROWN:
				health = maxHealth;
				UpdateHealthDisplay(health, maxHealth);
				crowns++;
				victoryStats.CrownsHeld++;
				UpdateCrownDisplay();
				break;
			case PickupType.ITEM_POTION_HEALTH:
				VFXManager.Instance.SyncFloatingText("Health +1", transform.position + 3 * Vector3.up, Color.red);
				IncreaseStat(Stat.HEALTH);
				break;
			case PickupType.ITEM_POTION_POWER:
				VFXManager.Instance.SyncFloatingText("Power +1", transform.position + 3 * Vector3.up, Color.green);
				IncreaseStat(Stat.POWER);
				break;
			case PickupType.ITEM_POTION_SKILL:
				VFXManager.Instance.SyncFloatingText("Skill +1", transform.position + 3 * Vector3.up, Color.blue);
				IncreaseStat(Stat.SKILL);
				break;
			case PickupType.ITEM_POTION_SPEED:
				VFXManager.Instance.SyncFloatingText("Speed +1", transform.position + 3 * Vector3.up, Color.cyan);
				IncreaseStat(Stat.SPEED);
				break;
			case PickupType.ITEM_POTION_STAMINA:
				VFXManager.Instance.SyncFloatingText("Stamina +1", transform.position + 3 * Vector3.up, Color.yellow);
				IncreaseStat(Stat.STAMINA);
				break;
			case PickupType.WEAPON_PICK:
				weaponId = 0;
				break;
			case PickupType.WEAPON_SWORD:
				weaponId = 1;
				break;
			case PickupType.WEAPON_UNARMED:
				weaponId = 2;
				break;
			case PickupType.WEAPON_SHIELD:
				weaponId = 3;
				break;
			case PickupType.WEAPON_CLUB:
				weaponId = 4;
				break;
			case PickupType.WEAPON_BOW:
				hasAmmo = true;
				weaponId = 5;
				break;
			case PickupType.WEAPON_TOME:
				weaponId = 6;
				break;
			case PickupType.WEAPON_CHAIN:
				weaponId = 7;
				break;
			case PickupType.SKILL_MAGNET:
				NewSkill(0);
				break;
			case PickupType.SKILL_FLIGHT:
				NewSkill(1);
				break;
			case PickupType.SKILL_SHOTGUN:
				NewSkill(2);
				break;
			case PickupType.SKILL_TELEPORT:
				NewSkill(3);
				break;
			case PickupType.SKILL_SHOT:
				NewSkill(4);
				break;
			case PickupType.SKILL_DRILL:
				NewSkill(5);
				break;
			case PickupType.SKILL_BOMB:
				NewSkill(6);
				break;
			case PickupType.SKILL_FLASK:
				NewSkill(7);
				break;
			case PickupType.SKILL_TURRET:
				NewSkill(8);
				break;
			case PickupType.SKILL_BOOMERANG:
				NewSkill(9);
				break;
		}


		void NewSkill(int id)
		{
			ResetCooldown();
			skillId = id;
		}
	}

	private void UpdateMoneyDisplay()
	{
		unitUI.UpdateMoney(money);
		SendMoney(money);
		[Command(requiresAuthority = false)]
		void SendMoney(int val)
		{
			RecieveMoney(val);
			[ClientRpc]
			void RecieveMoney(int val)
			{
				scorecard.SetGold(val);
				if (!isLocalPlayer)
				{
					unitUI.UpdateMoney(val);
				}
			}
		}
	}

	private void UpdateCrownDisplay()
	{
		unitUI.UpdateCrowns(crowns);
		SendCrownsDisplay(crowns);
		[Command(requiresAuthority = false)]
		void SendCrownsDisplay(int val)
		{
			RecieveCrownDisplay(val);
			[ClientRpc]
			void RecieveCrownDisplay(int val)
			{
				scorecard.SetCrowns(val);
				if (!isLocalPlayer)
				{
					unitUI.UpdateCrowns(val);
				}
			}
		}
	}

	private void UpdateMoneyLock(bool val)
	{
		SendLock(val);
		[Command]
		void SendLock(bool val)
		{
			RecieveLock(val);
			[ClientRpc]
			void RecieveLock(bool val)
			{
				unitUI.SetMoneyLock(val);
			}
		}
	}

	private void UpdateDodgeDisplay(float cooldown)
	{
		SendValue(cooldown);
		[Command(requiresAuthority = false)]
		void SendValue(float val)
		{
			RecieveValue(val);
			[ClientRpc]
			void RecieveValue(float val)
			{
				if (!isLocalPlayer)
				{
					nextStamina = Time.time + val;
				}
			}
		}
	}

	public void UpdateSkillDisplay(float cooldown)
	{
		SendValue(cooldown);
		[Command(requiresAuthority = false)]
		void SendValue(float val)
		{
			RecieveValue(val);
			[ClientRpc]
			void RecieveValue(float val)
			{
				if (!isLocalPlayer)
				{
					nextSkill = Time.time + val;
				}
			}
		}
	}

	public void UpdateHealthDisplay(int health, int max)
	{
		SendValue(health, max);
		[Command(requiresAuthority = false)]
		void SendValue(int health, int max)
		{
			RecieveValue(health, max);
			[ClientRpc]
			void RecieveValue(int health, int max)
			{
				unitUI.UpdateHealth(health, max);
			}
		}
	}

	public void CreateAttackFX()
	{
		switch (attackId)
		{
			case 4:
				unarmedInstall = (unarmedInstall + 1) % 3;
				break;
			case 6:
				invuln = InvulnState.NONE;
				break;
			case 7:
				invuln = InvulnState.ARMOR;
				break;
			case 10:
				InterruptCharge();
				break;
			case 11:
				if (GameManager.Instance.IsLevelShop() || GameManager.Instance.GetLevelIndex() == 0)
				{
					VFXManager.Instance.SyncPrefabVFX(ParticlePrefabType.DUST_PUFF, transform.position + new Vector3(facing * 2f, -2f, 0));
					return;
				}

				int roll = Random.Range(0, 100);
				if (roll < 5)
				{
					GameManager.Instance.SpawnGemBurst(transform.position + new Vector3(facing * 2f, -2.5f, 0), 1);
				}
				else if (roll < 35)
				{
					GameManager.Instance.SpawnGoldBurst(transform.position + new Vector3(facing * 2f, -2.5f, 0), 2);
				}
				else
				{
					GameManager.Instance.SpawnGoldBurst(transform.position + new Vector3(facing * 2f, -2.5f, 0), 1);
				}
				break;
			case 12:
				InterruptCharge();
				break;
		}
	}

	private void InterruptCharge()
	{
		switch (attackId)
		{
			case 10:
			case 12:
				jump.ResetGravity();
				jump.ResetTerminalVelocity();
				break;

		}

	}

	public void CreateAttack()
	{
		if (HasBuff(BuffType.BARRIER))
		{
			EndBuff(BuffType.BARRIER);
		}
		
		switch (attackId)
		{
			case 0:
				SFXManager.Instance.PlaySound("bluntswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.2f)
					.SetPosition(new Vector3((float)facing * 1.75f, 0f, 0f))
					.SetSize(new Vector2(3.5f, 5f))
					.Finish();
				break;
			case 1:
				SFXManager.Instance.PlaySound("swiftswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(new Vector3(facing * 2, -0.5f, 0f))
					.SetSize(new Vector2(4f, 2.5f))
					.Finish();
				break;
			case 2:
				SFXManager.Instance.PlaySound("swiftswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(new Vector3(facing * 2, -0.5f, 0f))
					.SetSize(new Vector2(4f, 2.5f))
					.Finish();
				break;
			case 3:
				SFXManager.Instance.PlaySound("bluntswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(new Vector3(1.5f * facing, -0.5f, 0f))
					.SetSize(new Vector2(3.5f, 2f))
					.Finish();
				break;
			case 5:
				SFXManager.Instance.PlaySound("swiftswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(new Vector3(facing * 2, -1f, 0f))
					.SetSize(new Vector2(2f, 2f))
					.Finish();
				break;
			case 6:
				SFXManager.Instance.PlaySound("bluntswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(Vector3.zero)
					.SetSize(new Vector2(6f, 6f))
					.Finish();
				break;
			case 7:
				SFXManager.Instance.PlaySound("bluntswing");
				invuln = InvulnState.NONE;
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.2f)
					.SetPosition(new Vector3(1f, 1f, 0f))
					.SetSize(new Vector2(6f, 6f))
					.Finish();
				break;
			case 8:
				SFXManager.Instance.PlaySound("swiftswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(2f, 0.5f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down)
					.SetAnimation(3)
					.SetVelocity(40f * aim)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
					.SetUnique(UniqueProjectile.ARROW)
					.Finish();
				hasAmmo = false;
				break;
			case 9:
				SFXManager.Instance.PlaySound("swiftswing");
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(new Vector3(facing * 1.5f, -1f, 0f))
					.SetSize(new Vector2(2f, 2f))
					.Finish();
				break;
			case 10:
				SFXManager.Instance.PlaySound("bluntswing");
				if (Time.time - chargeStart > tomeChargeLength)
				{
					AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(12f, 1f)).SetDuration(0.2f)
						.MakeProjectile(transform.position + 0.5f * Vector3.down + 6 * aim)
						.SetAnimation(6)
						.SetVelocity(0.1f * aim)
						.DisableEntityImpact()
						.DisableWorldImpact()
						.RotateWithVelocity()
						.SetLifetime(0.5f)
						.Finish();
				}
				else
				{
					AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(2f, 2f)).SetDuration(0.2f)
						.MakeProjectile(transform.position + 0.5f * Vector3.down + 3 * aim)
						.SetAnimation(7)
						.SetVelocity(0.1f * aim)
						.RotateWithVelocity()
						.DisableEntityImpact()
						.DisableWorldImpact()
						.SetLifetime(0.55f)
						.Finish();
				}
				break;
			case 12:
				SFXManager.Instance.PlaySound("swiftswing");
				if (Time.time - chargeStart > chainChargeLength)
				{
					AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(2f, 0.5f))
						.MakeProjectile(transform.position + 0.5f * Vector3.down)
						.SetAnimation(8)
						.SetLifetime(0.2f)
						.SetVelocity(60f * aim)
						.RotateWithVelocity()
						.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
						.SetUnique(UniqueProjectile.CHAIN)
						.Finish();
				}
				else
				{
					AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
						.SetPosition(new Vector3((float)facing * 4.5f, 0f, 0f))
						.SetSize(new Vector2(3f, 4f))
						.Finish();
				}
				break;
			case 13:
				SFXManager.Instance.PlaySound("bluntswing");
				RaycastHit2D raycastHit2D = Physics2D.BoxCast(transform.position, new Vector2(1.5f, 2.5f), 0f, facing * Vector2.right, 7f, worldMask);
				if (!raycastHit2D && !GameManager.Instance.GetCurrentLevel().IsPointInGeometry(transform.position + facing * 7f * Vector3.right))
				{
					transform.position = transform.position + facing * 7f * Vector3.right;
				}
				else
				{
					transform.position = raycastHit2D.centroid;
				}


				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
					.SetPosition(new Vector3(1.5f * facing, -0.5f, 0f))
					.SetSize(new Vector2(3.5f, 2f))
					.Finish();
				break;
			case 14:
				SFXManager.Instance.PlaySound("bluntswing");
				jump.ForceLanding();
				jump.ForceVelocity(20);
				jump.SetGravity(6f);
				AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.2f)
					.SetPosition(new Vector3(1f * facing, 0f, 0f))
					.SetSize(new Vector2(2f, 4f))
					.Finish();
				break;

		}
	}

	public void SetColor(int colorId)
	{
		SendColor(colorId);
		[Command(requiresAuthority = false)]
		void SendColor(int id)
		{
			RecieveColor(id);
			[ClientRpc]
			void RecieveColor(int id)
			{
				sprite.material = colors[id];
				currentColor = id;

				if (scorecard != null)
					scorecard.SetColor(id);
			}
		}
	}

	public void DoHitstop(float duration)
	{
		isHitstop = true;
		endHitstop = Time.time + duration;
		animator.speed = 0f;
		move.Pause(endHitstop);
		jump.Pause(endHitstop);
	}

	public void SetInputLocked(bool value)
	{
		inputLocked = value;
	}

	public void SetStasis(bool value)
	{
		SetInputLocked(value);
		move.ResetCurves();
		move.ForceStop();
		jump.ForceLanding();
		jump.ForceVelocity(0f);
		animator.SetBool("moving", value: false);
		if (value)
		{
			jump.SetGravity(0f);
		}
		else
		{
			jump.ResetGravity();
		}
		move.enabled = !value;
		jump.enabled = !value;
		stasis = value;
	}

	public void NoitifyInteraction(bool state)
	{
		unitUI.SetInteractActive(state);
	}

	private void EndFlight()
	{
		if (flying)
		{
			VFXManager.Instance.SyncVFX(ParticleType.FLIGHT_END, transform.position, flip: false);
		}
		endFlight = 0f;
		flying = false;
		unitVFX.SetFXState(PlayerVFX.FLIGHT, state: false);
	}

	private void StartFlight()
	{
		endFlight = Time.time + 2.5f;
		flying = true;
		unitVFX.SetFXState(PlayerVFX.FLIGHT, state: true);
	}

	public void EnterLevel()
	{
		hasAmmo = true;
		if (GameManager.Instance.IsLevelShop())
		{
			UpdateMoneyDisplay();
			UpdateMoneyLock(val: true);
			GetShopReward();
		}
	}

	public void GetShopReward()
	{
		SendShopReward();
		[Command] void SendShopReward()
		{
			RecieveShopReward();
		}

		[ClientRpc] void RecieveShopReward()
		{
			if (!isLocalPlayer)
				return;

			StartCoroutine(DelayGiveReward());
		}

		IEnumerator DelayGiveReward()
		{
			yield return new WaitForSeconds(0.75f);
			switch (GameManager.Instance.GetShopRanking(this))
			{
				case 1:
					money += 5;
					UpdateMoneyDisplay();
					VFXManager.Instance.SendFloatingText("+5g", transform.position + 2 * Vector3.up, Color.yellow);
					break;
				case 2:
					money += 15;
					UpdateMoneyDisplay();
					VFXManager.Instance.SendFloatingText("+15g", transform.position + 2 * Vector3.up, Color.yellow);
					break;
				case 3:
					money += 25;
					UpdateMoneyDisplay();
					VFXManager.Instance.SendFloatingText("+25g", transform.position + 2 * Vector3.up, Color.yellow);
					break;
			}
		}
	}

	public void LeaveLevel(bool first)
	{
		UpdateMoneyLock(false);
		if (first)
			victoryStats.DoorsEntered++;
		UpdateVictoryStats();
	}

	public void UpdateVictoryStats()
	{
		SendVictoryStats(victoryStats);
		[Command] void SendVictoryStats(VictoryStats stats)
		{
			RecieveVictoryStats(stats);
		}

		[ClientRpc] void RecieveVictoryStats(VictoryStats stats)
		{
			victoryStats = stats;
		}
	}

	public VictoryStats GetVictoryStats()
	{
		return victoryStats;
	}

	public void PingNameplate()
	{
		unitUI.UpdateNameplate();
	}

	public void RefreshAmmo()
	{
		if (isServer)
		{
			RecieveAmmoRefresh();
		}
		else
		{
			SendAmmoRefresh();
		}

		[Command] void SendAmmoRefresh()
		{
			RecieveAmmoRefresh();
		}

		[ClientRpc] void RecieveAmmoRefresh()
		{
			hasAmmo = true;
		}
	}

	private void IncreaseStat(Stat stat)
	{
		if (isServer)
		{
			RecieveStatIncrease(stat);
		}
		else
		{
			SendStatIncrease(stat);
		}

		[Command] void SendStatIncrease(Stat stat)
		{
			RecieveStatIncrease(stat);
		}

		[ClientRpc] void RecieveStatIncrease(Stat stat)
		{
			stats[(int)stat]++;

			string currstats = "Stats:\n";
			foreach (var s in stats)
			{
				currstats += $"{s}\n";
			}
			move.AdjustBaseSpeed(speedMod, speedMult);
		}
	}

	public void GiveBuff(BuffType type)
	{
		var buffColor = Color.magenta;

		if (type == BuffType.RANDOM)
			type = (BuffType)Random.Range(0, (int)BuffType.RANDOM);

		switch (type)
		{
			case BuffType.BARRIER:
				buffColor = Color.gray;
				break;
			case BuffType.SWIFT:
				buffColor = Color.yellow;
				break;
			case BuffType.BLOODLUST:
				buffColor = Color.red;
				break;
			case BuffType.GREED:
				unitVFX.SetFXState(PlayerVFX.PICKUP_RANGE, true);
				buffColor = Color.green;
				break;
			case BuffType.GHOSTFORM:
				buffColor = Color.cyan;
				break;
		}

		unitVFX.SetFXState(PlayerVFX.POWERUP_GENERIC, true);
		VFXManager.Instance.SyncFloatingText(type.ToString(), transform.position, buffColor);

		buffColor.a = 0.5f;
		unitVFX.EndChain(type.ToString());
		unitVFX.StartAfterImageChain(10f, 0.05f, 0.2f, false, buffColor, type.ToString());

		SendBuff(type);

		[Command(requiresAuthority = false)] void SendBuff(BuffType type)
		{
			RecieveBuff(type);
		}

		[ClientRpc] void RecieveBuff(BuffType type)
		{
			buffs[(int)type] = Time.time + 10f;
			buffDirty[(int)type] = true;

			move.AdjustBaseSpeed(speedMod, speedMult);

			if (type == BuffType.GREED)
				pickupBox.radius = 5f;
		}
	}

	public bool HasBuff(BuffType type)
	{
		return Time.time < buffs[(int)type];
	}

	public void EndBuff(BuffType type)
	{
		unitVFX.EndChain(type.ToString());
		if (type == BuffType.GREED)
		{
			unitVFX.SetFXState(PlayerVFX.PICKUP_RANGE, false);
		}

		if (isServer)
		{
			RecieveBuffEnd(type);
		}
		else
		{
			SendBuffEnd(type);
		}
		[Command] void SendBuffEnd(BuffType type)
		{
			RecieveBuffEnd(type);
		}

		[ClientRpc] void RecieveBuffEnd(BuffType type)
		{
			buffs[(int)type] = 0;
		}
	}

	public void DoBuffCleanup()
	{
		for (int i = 0; i < (int)BuffType.RANDOM; i++)
		{
			if (buffDirty[i] && Time.time > buffs[i])
			{
				buffDirty[i] = false;
				if ((BuffType)i == BuffType.SWIFT
				|| (BuffType)i == BuffType.GHOSTFORM
				|| (BuffType)i == BuffType.BARRIER)
					move.AdjustBaseSpeed(speedMod, speedMult);
				if ((BuffType)i == BuffType.GREED)
				{
					pickupBox.radius = 1f;
					if (isLocalPlayer)
					{
						unitVFX.SetFXState(PlayerVFX.PICKUP_RANGE, false);
					}
				}
			}
		}
	}

	private float CalculateSpeed(float baseSpeed)
	{
		return (baseSpeed + speedMod) * speedMult;
	}

	public int GetCurrentColor()
	{
		return currentColor;
	}

	public string GetDisplayName()
	{
		if (GameManager.Instance.IsLocalGame)
		{
			return playerProfileName;
		}
		if (friendId.HasValue)
			return Utils.GetSteamName(friendId.Value);
		return null;
	}

	public void UpdateProfileName(string name)
	{
		playerProfileName = name;
	}

	public InputHandler GetInput()
	{
		return input;
	}


	public bool IsShopConfirmed(PurchaseInterface shop)
	{
		return confirmedShop == shop;
	}

	public void ConfirmShop(PurchaseInterface shop)
	{
		confirmedShop = shop;
	}

	public void GrapplePull(Vector3 pos)
	{
		SendPull(pos);
		[Command] void SendPull(Vector3 pos)
		{
			RecievePull(pos);
		}

		[ClientRpc] void RecievePull(Vector3 pos)
		{
			if (!isLocalPlayer)
				return;

			Vector3 dir = (pos - transform.position).normalized;

			OnAttackCancel();
			StartAction();
			UpdateFacing(input.dir);

			attackCancel = true;
			attacking = true;

			attackId = 15;

			animator.SetInteger("attackId", attackId);
			animator.SetTrigger("attack");

			move.OverrideCurve(Mathf.Abs(dir.x) * chainPullSpeed, chainPullCurve, Mathf.Sign(dir.x));
			jump.SetGravity(0);
			jump.SetTerminalVelocity(chainPullSpeed);
			jump.ForceVelocity(dir.y * chainPullSpeed);
		}
	}

	public void MarkAttackCancellable()
	{
		attackCancel = true;
	}

	public void ResetCooldown()
	{

		if (isServer)
			RecieveReset();
		else
			SendReset();

		[Command] void SendReset()
		{
			RecieveReset();
		}


		[ClientRpc] void RecieveReset()
		{
			if (!isLocalPlayer)
				return;
			nextSkill = Time.time + 0.05f;
			UpdateSkillDisplay(0.05f);
		}

	}
}