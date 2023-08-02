namespace Sandbox.Utils;

public interface IComponentSimulable : IComponent
{
	void Simulate( IClient client );
}
public interface IComponentBuildInputs : IComponent
{
	void BuildInput();
}


