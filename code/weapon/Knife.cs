using System;
using Sandbox.pawn;
using Sandbox.pawn.PawnControllers;

namespace Sandbox.weapon
{
	[Library]
	public class KnifeConfig : WeaponConfig
	{
		public override string Name => "Knife";
		public override string Description => "Brutal slashing melee weapon";
		public override string ClassName => "hdn_knife";
		public override string Icon => "ui/weapons/knife.png";
		public override WeaponType Type => WeaponType.Hitscan;
		public override int Ammo => 0;
		public override int Damage => 40;
	}

	[Library( "hdn_knife" )]
	public partial class Knife : Weapon
	{
		public override string ViewModelPath => "models/knife/fp_knife.vmdl";
		public override WeaponConfig Config => new KnifeConfig();

		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 0.5f;
		public override bool IsMelee => true;
		public override CitizenAnimationHelper.HoldTypes HoldType => CitizenAnimationHelper.HoldTypes.Swing;
		public override CitizenAnimationHelper.Hand Handedness => CitizenAnimationHelper.Hand.Right;
		public override float MeleeRange => 80f;
		public override bool HasFlashlight => false;

		private MovementsController? MovementsController => Player?.MovementsController;
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/knife/w_knife.vmdl" );
		}

		public override void AttackSecondary()
		{
			ShootEffects();
			PlaySound( "knife.swipe" );
			PlayAttackAnimation();
			MeleeStrike( Config.Damage * 3f, 2f );
		}

		public override void AttackPrimary()
		{
			ShootEffects();
			PlaySound( "knife.swipe" );
			PlayAttackAnimation();
			MeleeStrike( Config.Damage, 1.5f );
		}

		protected override void OnMeleeStrikeHit( Entity entity, DamageInfo info )
		{
			if ( entity is Pawn player )
			{
				if ( info.Damage > Config.Damage * 1.5f )
					Sound.FromEntity( "pigstick.slash", player );
				else
					Sound.FromEntity( "slash", player );
			}
			/* TODO Handle slash on corpses
			else if ( entity is PlayerCorpse corpse )
			{
				if ( corpse.NumberOfFeedsLeft > 0 )
				{
					if ( Owner is HiddenPlayer owner && owner.Health < 100f )
					{
						var impact = Particles.Create( "particles/blood/large_blood/large_blood.vpcf" );
						impact.SetPosition( 0, info.Position );

						owner.Health = Math.Min( owner.Health + 15f, 100f );
						Sound.FromEntity( "hidden.feed", owner );

						corpse.NumberOfFeedsLeft--;

						if ( corpse.NumberOfFeedsLeft == 0 )
						{
							corpse.SetMaterialGroup( 5 );
						}
					}
				}

				Sound.FromEntity( "slash", corpse );
			}
			*/
			else
			{
				Sound.FromWorld( "knife.slash", info.Position );
			}
		}

		[GameEvent.Client.Frame]
		protected virtual void OnFrame()
		{
			//EnableDrawing = false;
		}
	}
}
