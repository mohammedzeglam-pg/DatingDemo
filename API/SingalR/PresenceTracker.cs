using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SingalR
{
  //TODO: scale to redis
  //FIXME: dict not thread safe to use that why use lock here but it will be slow
  public class PresenceTracker
  {
    private static readonly Dictionary<string, List<string>> OnlineUsers = new();

    public Task<bool> UserConnected(string username, string connectionId)
    {
      bool isOnline = false;
      lock (OnlineUsers)
      {
        if (OnlineUsers.ContainsKey(username))
        {
          OnlineUsers[username].Add(connectionId);
        }
        else
        {
          OnlineUsers.Add(username, new List<string> { connectionId });
          isOnline = true;
        }
      }
      return Task.FromResult(isOnline);
    }
    public Task<bool> UserDisconnected(string username, string connectionId)
    {

      bool isOffline = false;
      lock (OnlineUsers)
      {
        if (!OnlineUsers.ContainsKey(username))
        {
          return Task.FromResult(isOffline);
        }

        _ = OnlineUsers[username].Remove(connectionId);

        if (OnlineUsers[username].Count == 0)
        {
          _ = OnlineUsers.Remove(username);
          isOffline = true;
        }

      }
      return Task.FromResult(isOffline);
    }
    public Task<string[]> GetOnlineUsers()
    {
      string[] onlineUsers;
      lock (OnlineUsers)
      {
        onlineUsers = OnlineUsers.OrderBy(u => u.Key).Select(u => u.Key).ToArray();
      }
      return Task.FromResult(onlineUsers);
    }

    public Task<List<string>> GetConnectionsForUser(string username)
    {
      List<string> connections;
      lock (OnlineUsers)
      {
        connections = OnlineUsers.GetValueOrDefault(username);
      }
      return Task.FromResult(connections);
    }
  }
}
