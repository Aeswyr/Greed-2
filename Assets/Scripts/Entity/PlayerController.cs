using System.Collections.Generic;
using Mirror;
using Mirror.BouncyCastle.Security;
using Mirror.RemoteCalls;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
	private enum Stat
	{
		AGILITY = 0,
		SKILL = 1,
		POWER = 2,
		RESILIENCE = 3,
		DEXTERITY = 4
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
	private InteractboxController interactBox;

	[SerializeField]
	private MaterialLibrary colors;

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
	private float staminaCooldown;

	[SerializeField]
	private float dodgeSpeed;

	[SerializeField]
	private AnimationCurve dodgeCurve;

	[SerializeField]
	private float pickAttackSpeed;

	[SerializeField]
	private AnimationCurve pickAttackCurve;

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

	private float skillCooldown = 4f;

	private int currentColor;

	private int money;

	private int crowns;

	private int health;

	private int maxHealth;

	private bool healthEmptied;

	private float nextStamina;

	private float nextSkill;

	private float endHitstop;

	private bool isHitstop;

	private float lastForward;

	private bool grounded;

	private int facing = 1;

	private Vector3 aim;

	private bool acting;
	private bool attackCancel;

	private bool attacking;

	private float hitStunTime;

	private bool hitStun;

	private int attackId = -1;

	private int weaponId = 6; //0

	private int skillId = 5; //-1

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

	private int[] stats = new int[5];

	private LayerMask worldMask;
	[SyncVar] private int playerId = -1;
	public int PlayerID => playerId;
	public bool hasAmmo = true;

	private void Start()
	{
		maxHealth = 100;
		health = maxHealth;
		worldMask = LayerMask.GetMask("World");
		sprite.material = colors[currentColor];
		if (isLocalPlayer)
		{
			input = FindObjectOfType<InputHandler>();
			jump.SetInput(input);
			GameManager.Instance.AddLobbyCard(this, input);
		}
		
		unitUI.SetNameplate(Utils.GetLocalSteamName());
	}

	[Server] public void AssignId(int id) {
		playerId = id;
	}

	private void FixedUpdate()
	{
		unitUI.UpdateStamina(1f - (nextStamina - Time.time) / staminaCooldown);
		unitUI.UpdateSkill(1f - (nextSkill - Time.time) / skillCooldown);
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

			if (isWallJumping) {
				Debug.Log("did a lil hop");
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
				if (charging) {
					InterruptCharge();
					EndAction();
					if (input.dir != 0)
						move.StartAcceleration(input.dir);
				}
			}
		}
		if ((!acting || (charging && Time.time > nextStamina)) && Time.time > nextStamina && grounded && input.dodge.pressed)
		{
			if (charging) {
				InterruptCharge();
				EndAction();
			}
			StartAction();
			nextStamina = Time.time + staminaCooldown;
			UpdateDodgeDisplay(staminaCooldown);
			UpdateFacing(input.dir);
			animator.SetTrigger("dodge");
			move.OverrideCurve(dodgeSpeed, dodgeCurve, facing);
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

	private void OnAttackCancel() {
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
			int num = attackId;
			int num2 = num;
			if (num2 == 4)
			{
				jump.ResetGravity();
				jump.ResetTerminalVelocity();
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
		void StartCharge() {
			charging = true;
			chargeStart = Time.time;
		}

		attacking = true;
		switch (weaponId)
		{
		case 0:
			attackId = 0;
			break;
		case 1:
			attackId = 1;
			if (input.aim.y < 0f)
			{
				attackId = 2;
			}
			break;
		case 2:
			attackId = 3;
			if (input.aim.y < 0f)
			{
				attackId = 4;
			}
			break;
		case 3:
			attackId = 5;
			if (input.aim.y < 0f)
			{
				invuln = InvulnState.PARRY;
				attackId = 6;
			}
			break;
		case 4:
			StartCharge();
			attackId = 7;
			break;
		case 5:
			StartCharge();
			attackId = 8;
			break;
		case 6:
			StartCharge();
			attackId = 9;
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
				move.OverrideCurve(pickAttackSpeed, pickAttackCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
			break;
		case 1:
			if (input.dir != 0f)
			{
				move.OverrideCurve(swordAttackSpeed, swordAttackCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
			break;
		case 2:
			move.OverrideCurve(swordLungeSpeed, swordLungeCurve, facing);
			break;
		case 3:
			if (input.dir != 0f)
			{
				move.OverrideCurve(unarmedAttackSpeed, unarmedAttackCurve, facing);
			}
			else if (grounded)
			{
				move.OverrideCurve(unarmedShortSpeed, unarmedShortCurve, facing);
			}
			break;
		case 4:
			attacking = false;
			jump.ForceVelocity(0f);
			jump.ForceLanding();
			jump.DisableGravity();
			move.OverrideSpeed(0f);
			move.ForceStop();
			break;
		case 5:
			if (input.dir != 0f)
			{
				move.OverrideCurve(shieldAttackSpeed, shieldAttackCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
			break;
		case 6:
			if (input.dir != 0f)
			{
				move.OverrideCurve(shieldParrySpeed, shieldParryCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
			break;
		case 7:
			if (input.dir != 0f)
			{
				move.OverrideCurve(greatweaponAttackSpeed, greatweaponAttackCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
			break;
		case 8:
			if (input.dir != 0f)
			{
				move.OverrideCurve(bowAttackSpeed, bowAttackCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
			break;
		case 9:
			if (input.dir != 0f)
			{
				jump.SetGravity(0.5f);
				jump.ForceVelocity(0);
				jump.SetTerminalVelocity(2);
				move.OverrideCurve(bowAttackSpeed, bowAttackCurve, facing);
			}
			else if (grounded)
			{
				move.StartDeceleration();
			}
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
				move.OverrideCurve(greatweaponReleaseSpeed, greatweaponReleaseCurve, facing);
			}
			else
			{
				move.StartDeceleration();
			}
			break;
		case 8:
			charging = false;
			animator.SetTrigger("release");
			if (input.dir != 0f)
			{
				UpdateFacing(input.dir);
			}
			move.OverrideCurve(bowReleaseSpeed, bowReleaseCurve, -facing);
			break;
		case 9:
			charging = false;
			animator.SetTrigger("release");
			if (input.dir != 0f)
			{
				UpdateFacing(input.dir);
			}
			move.OverrideCurve(bowReleaseSpeed, bowReleaseCurve, -facing);
			break;
		}
	}

	public void PressSkill()
	{
		switch (skillId)
		{
		case 0:
		case 1:
			animator.SetTrigger("skill_self");
			break;
		case 3:
			animator.SetTrigger("skill_self");
			invuln = InvulnState.GENERIC;
			break;
		case 2:
		case 4:
		case 5:
		case 6:
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
	public void CreateSkillFX() {
		if (!isLocalPlayer)
		{
			return;
		}

		switch (skillId) {
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
				.SetVelocity(10f * aim)
				.SetLifetime(2f)
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
		}
	}

	public void ReleaseSkill()
	{
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
			SendHit((int)Mathf.Sign(transform.position.x - parent.position.x), source.transform.position, flag ? parent : null);
		}
		[Command(requiresAuthority = false)]
		void SendHit(int knockbackDir, Vector3 hitPosition, Transform source)
		{
			RecieveHit(knockbackDir, hitPosition, source);
		}
	}

	[Command]
	private void SpoofHit()
	{
		RecieveHit(facing, Vector3.zero, null);
	}

	[ClientRpc]
	private void RecieveHit(int knockbackDir, Vector3 hitPosition, Transform source)
	{
		if (!isLocalPlayer || stasis || GameManager.Instance.IsLevelShop())
		{
			return;
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
			return;
		}
		invuln = InvulnState.HITSTUN;
		StartAction();
		UpdateFacing(-knockbackDir);
		animator.SetBool("hurt", value: true);
		hitStun = true;
		hitStunTime = Time.time + knockbackDuration;
		move.OverrideCurve(knockbackSpeed, knockbackCurve, knockbackDir);
		jump.ForceLanding();
		jump.ForceVelocity(30f);
		DoHitstop(0.15f);
		if (money > 0)
		{
			int num = Mathf.CeilToInt((float)money / 2f);
			DropLoot(num);
			money -= num;
			UpdateMoneyDisplay();
		}
		if (health > 0 && crowns > 0)
		{
			health -= 50;
			UpdateHealthDisplay(health, maxHealth);
			if (health <= 0)
			{
				healthEmptied = true;
				DropCrown();
				crowns--;
				UpdateCrownDisplay();
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
			GetItem(pickup.GetPickupType());
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
		case PickupType.ITEM_CROWN:
			health = maxHealth;
			UpdateHealthDisplay(health, maxHealth);
			crowns++;
			UpdateCrownDisplay();
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
		case PickupType.SKILL_MAGNET:
			skillId = 0;
			break;
		case PickupType.SKILL_FLIGHT:
			skillId = 1;
			break;
		case PickupType.SKILL_SHOTGUN:
			skillId = 2;
			break;
		case PickupType.SKILL_TELEPORT:
			skillId = 3;
			break;
		case PickupType.SKILL_SHOT:
			skillId = 4;
			break;
		case PickupType.SKILL_DRILL:
			skillId = 5;
			break;
		case PickupType.SKILL_BOMB:
			skillId = 6;
			break;
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
		SendCrowns(crowns);
		[Command(requiresAuthority = false)]
		void SendCrowns(int val)
		{
			RecieveCrowns(val);
			[ClientRpc]
			void RecieveCrowns(int val)
			{
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
			attacking = true;
			jump.SetTerminalVelocity(10000f);
			jump.ForceVelocity(-60f);
			move.OverrideSpeed(6f);
			break;
		case 6:
			invuln = InvulnState.NONE;
			break;
		case 7:
			invuln = InvulnState.ARMOR;
			break;
		case 9:
			InterruptCharge();
			break;
		}
	}

	private void InterruptCharge() {
		switch (attackId) {
			case 9:
				jump.ResetGravity();
				jump.ResetTerminalVelocity();
				break;
		}

	} 

	public void CreateAttack()
	{
		switch (attackId)
		{
		case 0:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.2f)
				.SetPosition(new Vector3((float)facing * 1.75f, 0f, 0f))
				.SetSize(new Vector2(3.5f, 5f))
				.Finish();
			break;
		case 1:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
				.SetPosition(new Vector3(facing * 2, -0.5f, 0f))
				.SetSize(new Vector2(4f, 2.5f))
				.Finish();
			break;
		case 2:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
				.SetPosition(new Vector3(facing * 2, -0.5f, 0f))
				.SetSize(new Vector2(4f, 2.5f))
				.Finish();
			break;
		case 3:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.2f)
				.SetPosition(new Vector3(facing, -0.5f, 0f))
				.SetSize(new Vector2(4f, 3f))
				.Finish();
			break;
		case 4:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
				.SetPosition(Vector3.down)
				.SetSize(new Vector2(2f, 3f))
				.Finish();
			break;
		case 5:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
				.SetPosition(new Vector3(facing * 2, -1f, 0f))
				.SetSize(new Vector2(2f, 2f))
				.Finish();
			break;
		case 6:
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.1f)
				.SetPosition(Vector3.zero)
				.SetSize(new Vector2(6f, 6f))
				.Finish();
			break;
		case 7:
			invuln = InvulnState.NONE;
			AttackBuilder.GetAttack(transform).SetParent(transform).SetDuration(0.2f)
				.SetPosition(new Vector3(1f, 1f, 0f))
				.SetSize(new Vector2(6f, 6f))
				.Finish();
			break;
		case 8:
			if (hasAmmo) {
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(2f, 0.5f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down)
					.SetAnimation(3)
					.SetVelocity(40f * aim)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
					.Finish();
				hasAmmo = false;
			}
			break;
		case 9:
			if (Time.time - chargeStart > 0.5f) {
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(1f, 1f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down)
					.SetAnimation(6)
					.SetVelocity(20f * aim)
					.RotateWithVelocity()
					.SetParticleType(ParticleType.PROJECTILE_HITSPARK)
					.SetLifetime(0.5f)
					.Finish();
			} else {
				AttackBuilder.GetAttack(transform).SetParent(transform).SetSize(new Vector2(2f, 2f))
					.MakeProjectile(transform.position + 0.5f * Vector3.down + 3 * aim)
					.SetAnimation(7)
					.SetVelocity(0.1f * aim)
					.RotateWithVelocity()
					.DisableEntityImpact()
					.DisableWorldImpact()
					.SetLifetime(0.2f)
					.Finish();
			}
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
		}
	}

	public void LeaveLevel()
	{
		UpdateMoneyLock(val: false);
	}

	public void PingNameplate() {
		unitUI.UpdateNameplate();
	}
}
