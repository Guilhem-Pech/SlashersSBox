using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public class InteractionController : EntityComponent<Pawn>, IComponentSimulable
{
	private const float DistanceOfUse = 85f;
	private const float SqrDistanceOfUse = DistanceOfUse * DistanceOfUse;

	/// <summary>
	/// Entity the player is currently using via their interaction key.
	/// </summary>
	public Entity? Using { get; protected set; }
	
	public void Simulate( IClient client )
	{
		// This is serverside only
		if ( !Game.IsServer ) return;

		// Turn prediction off
		using ( Prediction.Off() )
		{
			if ( Input.Pressed( "use" ) )
			{
				Using = FindUsable();

				if ( Using == null )
				{
					UseFail();
					return;
				}
			}

			if ( !Input.Down( "use" ) )
			{
				StopUsing();
				return;
			}

			if ( Using != null && Entity.Position.DistanceSquared( Using.Position ) > SqrDistanceOfUse )
			{
				StopUsing();
				return;
			}

			if ( !Using.IsValid() )
				return;

			//
			// If use returns true then we can keep using it
			//
			if ( Using is IUse use && use.OnUse( Entity ) )
				return;

			StopUsing();
		}
	}
	
	/// <summary>
	/// If we're using an entity, stop using it
	/// </summary>
	protected virtual void StopUsing()
	{
		Using = null;
	}
	
	/// <summary>
	/// Find a usable entity for this player to use
	/// </summary>
	protected virtual Entity? FindUsable()
	{
		// First try a direct 0 width line

		var tr = Trace.Ray( Entity.EyePosition, Entity.EyePosition + Entity.EyeRotation.Forward * DistanceOfUse )
			.Ignore( Entity )
			.Run();

		// See if any of the parent entities are usable if we ain't.
		var ent = tr.Entity;
		while ( ent.IsValid() && !IsValidUseEntity( ent ) )
		{
			ent = ent.Parent;
		}

		// Nothing found, try a wider search
		if ( !IsValidUseEntity( ent ) )
		{
			tr = Trace.Ray( Entity.EyePosition, Entity.EyePosition + Entity.EyeRotation.Forward * DistanceOfUse )
				.Radius( 2 )
				.Ignore( Entity )
				.Run();

			// See if any of the parent entities are usable if we ain't.
			ent = tr.Entity;
			while ( ent.IsValid() && !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		// Still no good? Bail.
		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}
	
	/// <summary>
	/// Returns if the entity is a valid usable entity
	/// </summary>
	protected bool IsValidUseEntity( Entity e )
	{
		if ( e is not IUse use ) return false;
		if ( !use.IsUsable( Entity ) ) return false;

		return true;
	}
	
	/// <summary>
	/// Player tried to use something but there was nothing there.
	/// Tradition is to give a disappointed boop.
	/// </summary>
	protected virtual void UseFail()
	{
		Entity.PlaySound( "player_use_fail" );
	}
}
