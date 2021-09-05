using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SingalR
{
  [Authorize]
  public class PresenceHub : Hub
  {
    private readonly PresenceTracker _tracker;

    public PresenceHub(PresenceTracker tracker)
    {
      _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
      string username = Context.User.GetUsername();
      bool isOnline = await _tracker.UserConnected(username, Context.ConnectionId);
      if (isOnline)
      {

        await Clients.Others.SendAsync("UserIsOnline", username);
      }
      string[] onlineUsers = await _tracker.GetOnlineUsers();
      await Clients.Caller.SendAsync("GetOnlineUsers", onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      string username = Context.User.GetUsername();
      bool isOffline = await _tracker.UserDisconnected(username, Context.ConnectionId);
      if (isOffline)
      {
        await Clients.Others.SendAsync("UserIsOffline", username);
      }
      await base.OnDisconnectedAsync(exception);
    }

  }
}
