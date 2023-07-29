namespace Sandbox.Utils;

public interface IComponentSimulable : IComponent
{
	void Simulate( IClient client );
}
