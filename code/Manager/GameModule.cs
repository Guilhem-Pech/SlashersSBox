namespace Sandbox.Manager;

public abstract class AGameModule : EntityComponent
{
	
	protected override void OnActivate()
	{
		Name = $"{GetType().Name} ({(Game.IsClient ? "Client" : "Server")})";
		base.OnActivate();
	}

	
	protected override void OnDeactivate()
	{
		base.OnDeactivate();
	} 
	
}
