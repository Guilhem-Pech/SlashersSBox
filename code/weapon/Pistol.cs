﻿using System;
using Sandbox.pawn.PawnControllers;

namespace Sandbox.weapon
{
	[Library]
	public class PistolConfig : WeaponConfig
	{
		public override string Name => "M1911";
		public override string Description => "Short-range hitscan pistol";
		public override string ClassName => "hdn_pistol";
		public override string Icon => "ui/weapons/pistol.png";
		public override AmmoType AmmoType => AmmoType.Pistol;
		public override WeaponType Type => WeaponType.Hitscan;
		public override int Ammo => 0;
		public override int Damage => 20;
	}

	[Library( "hdn_pistol" )]
	public partial class Pistol : Weapon
	{
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
		public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
		public override WeaponConfig Config => new PistolConfig();
		public override bool UnlimitedAmmo => true;
		public override bool IsAutomatic => false;
		public override int ClipSize => 10;
		public override float PrimaryRate => 3f;
		public override float SecondaryRate => 1.0f;
		public override float DamageFalloffStart => 500f;
		public override float DamageFalloffEnd => 8000f;
		public override float ReloadTime => 3.0f;
		
		public override CitizenAnimationHelper.HoldTypes HoldType => CitizenAnimationHelper.HoldTypes.Pistol;

		[ClientRpc]
		protected override void ShootEffects()
		{
			Game.AssertClient();
			base.ShootEffects();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );
		}
		public override void PlayReloadSound()
		{
			PlaySound( "rust_pistol.reload" );
			base.PlayReloadSound();
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			Game.SetRandomSeed( Time.Tick );

			ShootEffects();
			PlaySound( $"rust_pistol.shoot" );
			ShootBullet( 0.01f, 1.5f, Config.Damage, 8.0f );
			PlayAttackAnimation();
			AddRecoil( new Angles( Game.Random.Float( -0.75f, -1f ), 0f, 0f ) );
		}
	}
}
