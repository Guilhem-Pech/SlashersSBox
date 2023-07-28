using Sandbox;
using System.ComponentModel;
using Sandbox.pawn.PawnControllers;

namespace MyGame;

public partial class Pawn : AnimatedEntity
{
	[Net, Predicted]
	public Weapon ActiveWeapon { get; set; }

	[ClientInput]
	public Vector3 InputDirection { get; set; }
	
	[ClientInput]
	public Angles ViewAngles { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}

	[BindComponent] public MovementsController Controller { get; }
	
	[BindComponent] public CameraController CameraController { get; }
	[BindComponent] public AnimatorController AnimatorController { get; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );
	
	private float m_timeStampLastFootStep = 0f;

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		ActiveWeapon?.OnHolster();
		ActiveWeapon = weapon;
		ActiveWeapon.OnEquip( this );
	}

	public void Respawn()
	{
		Components.Create<MovementsController>();
		Components.Create<AnimatorController>();
		Components.Create<CameraController>();

		SetActiveWeapon( new Pistol() );
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		CameraController?.Simulate( cl );
		Controller?.Simulate( cl );
		AnimatorController?.Simulate(cl);
		ActiveWeapon?.Simulate( cl );
		EyeLocalPosition = Vector3.Up * (64f * Scale);
	}
	
	public override void FrameSimulate( IClient cl )
	{
		CameraController?.FrameSimulate( cl );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	public override void OnAnimEventFootstep( Vector3 position, int foot, float volume )
	{
		if(Game.IsServer || Time.Now - m_timeStampLastFootStep < 0.2f )
			return;
		
		var trace = Trace.Ray( position, position + Vector3.Down * 10f )
			.Ignore( this )
			.Run();
		
		if ( !trace.Hit ) return;

		float sqrJogSpeed = Controller.JogSpeed * Controller.JogSpeed;
		volume *= Velocity.WithZ( 0f ).LengthSquared.LerpInverse( 0,  sqrJogSpeed * 0.8f) * 1f;

		trace.Surface.DoFootstep( this, trace, foot, volume );
		
		m_timeStampLastFootStep = Time.Now;
	}
}
