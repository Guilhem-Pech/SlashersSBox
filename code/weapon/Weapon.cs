using System;
using System.Collections.Generic;
using Sandbox.pawn;
using Sandbox.pawn.PawnControllers;
using Sandbox.Utility;
using Sandbox.Utils;

namespace Sandbox.weapon;

public abstract partial class Weapon : BaseWeapon
{
	public abstract WeaponConfig Config { get; }
	public virtual string MuzzleAttachment => "muzzle";
	public virtual string MuzzleFlashEffect => "particles/pistol_muzzleflash.vpcf";

	public virtual List<string?> FlybySounds => new()
	{
		"flyby.rifleclose1", "flyby.rifleclose2", "flyby.rifleclose3", "flyby.rifleclose4"
	};

	public virtual bool ShowHitMarkerCrosshair => true;
	public virtual bool ShowChargeCrosshair => true;
	public virtual bool ShowRegularCrosshair => false;
	public virtual string? ImpactEffect => null;
	public virtual int ClipSize => 16;
	public virtual float AutoReloadDelay => 1.5f;
	public virtual float ReloadTime => 3f;
	public virtual bool IsMelee => false;
	public virtual float DamageFalloffStart => 0f;
	public virtual float DamageFalloffEnd => 0f;
	public virtual float BulletRange => 20000f;
	public virtual bool AutoReload => true;
	public virtual string? TracerEffect => null;
	public virtual bool UnlimitedAmmo => false;
	public virtual bool CanMeleeAttack => false;
	public virtual bool IsPassive => false;
	public virtual bool HasFlashlight => true;
	public virtual bool IsAutomatic => true;
	public virtual float MeleeDuration => 0.4f;
	public virtual float MeleeDamage => 80f;
	public virtual float MeleeForce => 2f;
	public virtual float MeleeRange => 200f;
	public virtual float MeleeRate => 1f;
	public virtual float ChargeAttackDuration => 2f;
	public virtual string DamageType => "bullet";
	public virtual CitizenAnimationHelper.HoldTypes HoldType => CitizenAnimationHelper.HoldTypes.Pistol;
	public virtual CitizenAnimationHelper.Hand Handedness => CitizenAnimationHelper.Hand.Both;
	public virtual int ViewModelMaterialGroup => 0;
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";

	[Net] public int Slot { get; set; }

	[Net, Predicted] public int AmmoClip { get; set; }

	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted] public bool IsReloading { get; set; }

	[Net, Predicted] public TimeSince TimeSinceDeployed { get; set; }

	[Net, Predicted] public TimeSince TimeSinceChargeAttack { get; set; }

	[Net, Predicted] public TimeSince TimeSinceMeleeAttack { get; set; }

	public float ChargeAttackEndTime { get; private set; }
	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity ?? throw new InvalidOperationException();

	private Queue<Angles> RecoilQueue { get; set; } = new();

	public Pawn Player => Owner as Pawn ?? throw new InvalidOperationException();

	private InventoryController? OwnerInventory => (Owner as Pawn)?.InventoryController;
	public int AvailableAmmo()
	{
		if ( OwnerInventory == null ) return 0;
		return OwnerInventory.AmmoCount( Config.AmmoType );
	}

	public void AddRecoil( Angles angles )
	{
		if ( Game.IsServer ) return;
		RecoilQueue.Enqueue( angles );
	}

	public float GetDamageFalloff( float distance, float damage )
	{
		return Util.Weapons.GetDamageFalloff( distance, damage, DamageFalloffStart, DamageFalloffEnd );
	}

	public virtual void Restock()
	{
		var remainingAmmo = (ClipSize - AmmoClip);

		if ( remainingAmmo > 0 && OwnerInventory is {} inventory )
		{
			inventory.GiveAmmo( Config.AmmoType, remainingAmmo );
		}
	}

	public virtual bool IsAvailable()
	{
		return true;
	}

	public virtual void PlayAttackAnimation()
	{
		AnimationOwner?.SetAnimParameter( "b_attack", true );
	}

	public virtual void PlayReloadAnimation()
	{
		AnimationOwner?.SetAnimParameter( "b_reload", true );
	}
	public virtual void OnMeleeAttack()
	{
		ViewModelEntity?.SetAnimParameter( "melee", true );
		TimeSinceMeleeAttack = 0f;
		MeleeStrike( MeleeDamage, MeleeForce );
		PlaySound( "player.melee" );
	}

	public override bool CanReload()
	{
		if ( CanMeleeAttack && TimeSinceMeleeAttack < (1 / MeleeRate) )
		{
			return false;
		}

		if ( AutoReload && TimeSincePrimaryAttack > AutoReloadDelay && AmmoClip == 0 )
		{
			return true;
		}

		return base.CanReload();
	}

	public override void ActiveStart( Entity owner )
	{
		base.ActiveStart( owner );

		TimeSinceDeployed = 0f;
	}

	public override void ActiveEnd( Entity owner, bool dropped )
	{
		base.ActiveEnd( owner, dropped );

		IsReloading = false;
	}

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = ClipSize;
		EnableShadowInFirstPerson = false;

		SetModel( ModelPath );
	}

	public override void Reload()
	{
		if ( IsMelee || IsReloading )
			return;

		if ( AmmoClip >= ClipSize )
			return;

		TimeSinceReload = 0f;

		if ( OwnerInventory is {} playerInventory )
		{
			if ( !UnlimitedAmmo )
			{
				if ( playerInventory.AmmoCount( Config.AmmoType ) <= 0 )
					return;
			}
		}

		IsReloading = true;

		PlayReloadAnimation();
		PlayReloadSound();
		DoClientReload();
	}

	public override void Simulate( IClient owner )
	{
		if ( Player.LifeState == LifeState.Alive )
		{
			if ( ChargeAttackEndTime > 0f && Time.Now >= ChargeAttackEndTime )
			{
				OnChargeAttackFinish();
				ChargeAttackEndTime = 0f;
			}
		}
		else
		{
			ChargeAttackEndTime = 0f;
		}

		/* TODO: Remove this? survivors will have flashlights as weapon and it'll be the primary attack?
		 if ( Input.Down( "zoom" ) )
		{
			if ( CanMeleeAttack && TimeSinceMeleeAttack > (1 / MeleeRate) )
			{
				IsReloading = false;
				OnMeleeAttack();
				return;
			}
		}*/

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public override void SimulateAnimator( AnimatorController anim )
	{
		base.SimulateAnimator( anim );
		anim.HoldType = HoldType;
		anim.Handedness = Handedness;
	}
	public override bool CanPrimaryAttack()
	{
		if ( ChargeAttackEndTime > 0f && Time.Now < ChargeAttackEndTime )
			return false;

		if ( TimeSinceMeleeAttack < MeleeDuration )
			return false;

		if ( TimeSinceDeployed < 0.3f )
			return false;

		if ( !IsAutomatic && !Input.Pressed( "attack1" ) )
			return false;

		return base.CanPrimaryAttack();
	}

	public override bool CanSecondaryAttack()
	{
		if ( ChargeAttackEndTime > 0f && Time.Now < ChargeAttackEndTime )
			return false;

		return base.CanSecondaryAttack();
	}

	public virtual void StartChargeAttack()
	{
		ChargeAttackEndTime = Time.Now + ChargeAttackDuration;
	}

	public virtual void OnChargeAttackFinish() { }

	public virtual void OnReloadFinish()
	{
		IsReloading = false;

		if ( OwnerInventory is {} playerInventory )
		{
			if ( !UnlimitedAmmo )
			{
				var ammo = playerInventory.TakeAmmo( Config.AmmoType, ClipSize - AmmoClip );

				if ( ammo == 0 )
					return;

				AmmoClip += ammo;
			}
			else
			{
				AmmoClip = ClipSize;
			}
		}
	}

	public virtual void PlayReloadSound()
	{
	}

	public virtual void RenderHud( Vector2 screenSize )
	{
		if ( Owner is not Pawn player )
			return;

		RenderCrosshair( player, screenSize * 0.5f );
	}

	private static Color _lightRed = Color.Red.WithAlpha( 0.8f );
	
	public virtual void RenderCrosshair( Pawn player, Vector2 center )
	{
		var draw = Util.Draw;
		var lastHitMarkerTime = player.TimeSinceLastHit.Relative;
		var color = Color.Lerp( Color.Red, Color.White, lastHitMarkerTime.LerpInverse( 0.0f, 0.4f ) );
		var hitEase = Easing.BounceIn( lastHitMarkerTime.LerpInverse( 0.5f, 0.0f ) );
		var circleSize = 56f * hitEase;
		var circleThickness = 24f * hitEase;

		if ( ShowHitMarkerCrosshair )
		{
			draw.BlendMode = BlendMode.Lighten;
			draw.Color = _lightRed;
			draw.CircleEx( center, circleSize, circleSize - circleThickness );
		}

		if ( ShowChargeCrosshair && ChargeAttackEndTime > 0f )
		{
			var fraction = Easing.EaseIn( (ChargeAttackEndTime - Time.Now) / ChargeAttackDuration );

			circleSize = 48f * (1f - fraction);
			circleThickness = 8f * (1f - fraction);

			draw.BlendMode = BlendMode.Lighten;
			draw.Color = Color.Red.Darken( 0.3f ).WithAlpha( 0.8f );
			draw.CircleEx( center, circleSize, circleSize - circleThickness );
		}

		if ( ShowRegularCrosshair )
		{
			var lastAttackTime = TimeSincePrimaryAttack.Relative;
			var shootEase = Easing.EaseIn( lastAttackTime.LerpInverse( 0.2f, 0.0f ) );

			draw.Color = color.WithAlpha( 0.4f + lastAttackTime.LerpInverse( 1f, 0 ) * 0.5f );

			var length = 8.0f - shootEase * 2.0f;
			var gap = 10.0f + shootEase * 50.0f;
			var thickness = 6.0f;

			draw.Line( thickness, center + Vector2.Left * gap, center + Vector2.Left * (length + gap) );
			draw.Line( thickness, center - Vector2.Left * gap, center - Vector2.Left * (length + gap) );
			draw.Line( thickness, center + Vector2.Up * gap, center + Vector2.Up * (length + gap) );
			draw.Line( thickness, center - Vector2.Up * gap, center - Vector2.Up * (length + gap) );
		}
	}

	[ClientRpc]
	public virtual void DoClientReload()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0f;
		TimeSinceSecondaryAttack = 0f;

		Game.SetRandomSeed( Time.Tick );

		ShootEffects();
		ShootBullet( 0.05f, 1.5f, Config.Damage, 3.0f );
	}

	public virtual void MeleeStrike( float damage, float force )
	{
		var traceSize = 20f;
		var forward = Player.EyeRotation.Forward.Normal;

		foreach ( var trace in TraceBullet( Player.EyePosition, Player.EyePosition + forward * MeleeRange, traceSize ) )
		{
			if ( !trace.Entity.IsValid() )
				continue;

			if ( !IsValidMeleeTarget( trace.Entity ) )
				continue;

			var impactTrace = trace;
			impactTrace.EndPosition -= trace.Normal * (traceSize * 0.5f);
			trace.Surface.DoBulletImpact( impactTrace );

			if ( !string.IsNullOrEmpty( ImpactEffect ) )
			{
				var impact = Particles.Create( ImpactEffect, trace.EndPosition );
				impact?.SetForward( 0, trace.Normal );
			}

			if ( Game.IsServer )
			{
				using ( Prediction.Off() )
				{
					var damageInfo = new DamageInfo()
						.WithPosition( trace.EndPosition )
						.WithTag( "blunt" )
						.WithForce( forward * 100f * force )
						.UsingTraceResult( trace )
						.WithAttacker( Owner )
						.WithWeapon( this );

					damageInfo.Damage = damage;

					trace.Entity.TakeDamage( damageInfo );

					OnMeleeStrikeHit( trace.Entity, damageInfo );
				}
			}
		}
	}

	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		var forward = Player.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		foreach ( var trace in TraceBullet( Player.EyePosition, Player.EyePosition + forward * BulletRange,
			         bulletSize ) )
		{
			var impactTrace = trace;
			impactTrace.EndPosition -= trace.Normal * (bulletSize * 0.5f);
			trace.Surface.DoBulletImpact( impactTrace );

			var fullEndPos = trace.EndPosition + trace.Direction * bulletSize;

			if ( !string.IsNullOrEmpty( TracerEffect ) )
			{
				var tracer = Particles.Create( TracerEffect, GetEffectEntity(), MuzzleAttachment );
				tracer?.SetPosition( 1, fullEndPos );
				tracer?.SetPosition( 2, trace.Distance );
			}

			if ( !string.IsNullOrEmpty( ImpactEffect ) )
			{
				var impact = Particles.Create( ImpactEffect, fullEndPos );
				impact?.SetForward( 0, trace.Normal );
			}

			if ( !Game.IsServer )
				continue;

			Util.Weapons.PlayFlybySounds( Owner, trace.Entity, trace.StartPosition, trace.EndPosition, bulletSize * 2f,
				bulletSize * 50f, FlybySounds );

			if ( trace.Entity.IsValid() )
			{
				using ( Prediction.Off() )
				{
					var damageInfo = new DamageInfo()
						.WithPosition( trace.EndPosition )
						.WithTag( DamageType )
						.WithForce( forward * 100f * force )
						.UsingTraceResult( trace )
						.WithAttacker( Owner )
						.WithWeapon( this );

					damageInfo.Damage = GetDamageFalloff( trace.Distance, damage );

					trace.Entity.TakeDamage( damageInfo );
				}
			}
		}
	}

	public bool TakeAmmo( int amount )
	{
		if ( Config.AmmoType == AmmoType.None )
			return false;

		if ( AmmoClip < amount )
			return false;

		AmmoClip -= amount;
		return true;
	}

	public override void CreateViewModel()
	{
		Game.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel { EnableViewmodelRendering = true, Position = Position, Owner = Owner };

		ViewModelEntity.SetModel( ViewModelPath );
		ViewModelEntity.SetMaterialGroup( ViewModelMaterialGroup );
	}

	public bool IsUsable()
	{
		if ( IsMelee || ClipSize == 0 || AmmoClip > 0 || UnlimitedAmmo )
		{
			return true;
		}

		return AvailableAmmo() > 0;
	}

	public override void BuildInput()
	{
		if ( RecoilQueue.Count > 0 )
		{
			var recoil = RecoilQueue.Dequeue();
			if ( Player is { } player)
			{
				player.ViewAngles += recoil;
			}
		}
	}

	public override IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		yield return Trace.Ray( start, end )
			.UseHitboxes()
			.Ignore( Owner )
			.WithoutTags( "trigger" )
			.Ignore( this )
			.Size( radius )
			.Run();
	}

	protected virtual void CreateMuzzleFlash()
	{
		if ( !string.IsNullOrEmpty( MuzzleFlashEffect ) )
		{
			Particles.Create( MuzzleFlashEffect, GetEffectEntity(), "muzzle" );
		}
	}

	protected virtual void OnMeleeStrikeHit( Entity entity, DamageInfo info )
	{
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		if ( !IsMelee )
		{
			CreateMuzzleFlash();
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	protected virtual ModelEntity GetEffectEntity()
	{
		return EffectEntity;
	}

	protected virtual bool IsValidMeleeTarget( Entity target )
	{
		return true;
	}

	protected void DealDamage( Entity target, Vector3 position, Vector3 force )
	{
		DealDamage( target, position, force, Config.Damage );
	}

	protected void DealDamage( Entity target, Vector3 position, Vector3 force, float damage )
	{
		var damageInfo = new DamageInfo()
			.WithAttacker( Owner )
			.WithWeapon( this )
			.WithPosition( position )
			.WithForce( force )
			.WithTag( DamageType );

		damageInfo.Damage = damage;

		target.TakeDamage( damageInfo );
	}
}
