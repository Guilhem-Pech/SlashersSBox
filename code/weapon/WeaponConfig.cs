﻿namespace Sandbox.weapon;

public enum WeaponType
{
	None,
	Hitscan,
	Projectile
}

public enum AmmoType
{
	None,
	Pistol,
	SMG,
	Rifle,
	Sniper,
	Shotgun,
	Grenade,
	LMG
}

public abstract class WeaponConfig
{
	public virtual string Name => "";
	public virtual string ClassName => "";
	public virtual string Description => "";
	public virtual string SecondaryDescription => "";
	public virtual string Icon => "";
	public virtual AmmoType AmmoType => AmmoType.Pistol;
	public virtual WeaponType Type => WeaponType.None;
	public virtual int Damage => 0;
	public virtual int Ammo => 0;
}
