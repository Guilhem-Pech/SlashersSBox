using MyGame;

namespace Sandbox.pawn.PawnControllers;

public class AnimatorController : EntityComponent<Pawn>, ISingletonComponent
{
	public void Simulate( IClient client )
	{
		var helper = new CitizenAnimationHelper( Entity );
		helper.WithVelocity( Entity.Velocity );
		helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.GroundEntity.IsValid();

		if (Entity.Controller?.HasEvent( "jump" ) == true )
		{
			helper.TriggerJump();
		}
	}
}
