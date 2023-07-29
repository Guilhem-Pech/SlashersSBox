namespace Sandbox.Manager;

public abstract class AGameModule : EntityComponent<Slashers>
{
	
	protected override void OnActivate()
	{
		Name = $"{GetType().Name}";
		base.OnActivate();
	}

	
	protected override void OnDeactivate()
	{
		base.OnDeactivate();
	} 
	
}
