using MyGame;
using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public class AnimatorController : EntityComponent<Pawn>, ISingletonComponent, IEventListener, IComponentSimulable
{
	private CitizenAnimationHelper m_helper;
	public CitizenAnimationHelper.HoldTypes HoldType { set; get; } = CitizenAnimationHelper.HoldTypes.None;
	public CitizenAnimationHelper.Hand Handedness { set; get; } = CitizenAnimationHelper.Hand.Right;
	public float AimBodyWeight { get; set; }


	private int m_duckLevel = 0;
	
	protected override void OnActivate()
	{
		AimBodyWeight = Entity.GetAnimParameterFloat( "aim_body_weight" ); // No idea what's the default value so there is that lol
		
		m_helper = new CitizenAnimationHelper( Entity );
		Entity.EventDispatcher.RegisterEvent<EventOnJump>( this, OnPawnJumped );
		Entity.EventDispatcher.RegisterEvent<EventOnDuck>( this, OnPawnDuck );
		Entity.EventDispatcher.RegisterEvent<EventOnUnDuck>( this, OnPawnUnduck );
		base.OnActivate();
	}
 
	private void OnPawnDuck( EventOnDuck obj )
	{
		m_duckLevel = 1;
	}
	
	private void OnPawnUnduck( EventOnUnDuck obj )
	{
		m_duckLevel = 0;
	} 

	private void OnPawnJumped( EventOnJump obj )
	{
		m_helper.TriggerJump();
	}

	protected override void OnDeactivate()
	{
		Entity.EventDispatcher.UnregisterAllEvents( this );
		base.OnDeactivate();
	}

	public void Simulate( IClient client )
	{
		m_helper.WithVelocity( Entity.Velocity );
		m_helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		m_helper.HoldType = HoldType;
		m_helper.Handedness = Handedness;
		m_helper.IsGrounded = Entity.GroundEntity.IsValid();
		m_helper.DuckLevel = m_duckLevel;
		m_helper.AimBodyWeight = AimBodyWeight;
	}
}
