﻿using System.Collections.Generic;
using Sandbox.pawn;
using Sandbox.pawn.PawnControllers;

namespace Sandbox.weapon;

public abstract partial class BaseWeapon : AnimatedEntity
{
	public virtual string? ViewModelPath => null;
	public virtual string? ModelPath => null;
	
	public BaseViewModel? ViewModelEntity { get; protected set; }

	public virtual ModelEntity EffectEntity =>
		(ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;

	public virtual float PrimaryRate => 5.0f;
	public virtual float SecondaryRate => 15.0f;

	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }

	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }

	public override void Simulate( IClient player )
	{
		if ( CanReload() )
		{
			Reload();
		}

		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}
		}

		if ( !Owner.IsValid() )
			return;

		if ( CanSecondaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSinceSecondaryAttack = 0;
				AttackSecondary();
			}
		}

		var owner = Owner as Pawn;
		if ( owner?.AnimatorController != null ) SimulateAnimator( owner.AnimatorController );
	}

	public virtual bool CanReload()
	{
		if ( !Owner.IsValid() || !Input.Down( "reload" ) ) return false;

		return true;
	}

	public virtual void Reload()
	{
	}

	public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) ) return false; // Todo handle automatic vs semi-automatic weapons here (Input.Pressed( "attack1" ) for semi and Input.Down( "attack1" ) for automatic)

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual void AttackPrimary()
	{
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack2" ) ) return false;

		var rate = SecondaryRate;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual void AttackSecondary()
	{
	}

	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc" )
			.WithoutTags( "trigger" )
			.Ignore( this )
			.Size( radius );

		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var result = trace.Run();

		if ( result.Hit )
			yield return result;
	}

	public virtual bool CanCarry( Entity carrier )
	{
		return true;
	}

	public virtual void OnCarryStart( Entity carrier )
	{
		if ( Game.IsClient ) return;

		SetParent( carrier, true );
		Owner = carrier;
		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	public virtual void SimulateAnimator( AnimatorController anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		anim.Handedness = CitizenAnimationHelper.Hand.Both;
		anim.AimBodyWeight = 1f;
	}

	public virtual void OnCarryDrop( Entity dropper )
	{
		if ( Game.IsClient ) return;

		SetParent( null );
		Owner = null;
		EnableDrawing = true;
		EnableAllCollisions = true;
	}

	public virtual void ActiveStart( Entity ent )
	{
		EnableDrawing = true;

		if ( IsLocalPawn )
		{
			DestroyViewModel();
			CreateViewModel();
		}
	}

	public virtual void ActiveEnd( Entity ent, bool dropped )
	{
		if ( !dropped )
		{
			EnableDrawing = false;
		}

		if ( Game.IsClient )
		{
			DestroyViewModel();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient && ViewModelEntity.IsValid() )
		{
			DestroyViewModel();
		}
	}

	public virtual void CreateViewModel()
	{
		Game.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new BaseViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( ViewModelPath );
	}

	public virtual void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}

	public override Sound PlaySound( string soundName, string attachment )
	{
		if ( Owner.IsValid() )
			return Owner.PlaySound( soundName, attachment );

		return base.PlaySound( soundName, attachment );
	}

	public override void Spawn()
	{
		base.Spawn();

		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}
}
