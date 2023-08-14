using System;
using Sandbox.Utils;

namespace Sandbox.Manager.GameFlow;

public partial class GameFlowModule : AGameModule, IEventListener
{
	public EventDispatcher EventDispatcher { private set; get; } = new EventDispatcher();
	
	[GameEvent.Server.ClientJoined]
	private void ClientJoined( ClientJoinedEvent e )
	{
		EventDispatcher.SendEvent<EventOnClientJoined>( e.Client );
	}
}

public class EventOnClientJoined : Utils.Event
{
	public IClient Client { private set; get; }
	EventOnClientJoined( IClient client ) { Client = client; }
}
