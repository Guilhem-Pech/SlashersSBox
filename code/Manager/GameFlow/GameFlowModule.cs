using System;

namespace Sandbox.Manager.GameFlow;

public partial class GameFlowModule : AGameModule
{
	private EventHandler<ClientJoinedEvent> m_onClientJoined;
	
	[GameEvent.Server.ClientJoined]
	private void ClientJoined( ClientJoinedEvent e )
	{
		m_onClientJoined?.Invoke( this, e );
	}
}
