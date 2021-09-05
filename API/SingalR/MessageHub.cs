using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.SingalR;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  public class MessageHub : Hub
  {
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;

    public MessageHub(IMessageRepository messageRepository,
                      IUserRepository userRepository,
                      IMapper mapper,
                      IHubContext<PresenceHub> presenceHub,
                      PresenceTracker tracker)
    {
      _messageRepository = messageRepository;
      _userRepository = userRepository;
      _mapper = mapper;
      _presenceHub = presenceHub;
      _tracker = tracker;
    }
    public override async Task OnConnectedAsync()
    {
      string username = Context.User.GetUsername();
      HttpContext httpContext = Context.GetHttpContext();
      string otherUser = httpContext.Request.Query["user"].ToString();
      string groupName = GroupName(username, otherUser);
      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
      Group group = await AddToGroup(groupName);
      await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
      IEnumerable<MessageDto> messages = await _messageRepository.GetMessageThread(username, otherUser);
      await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
      Group group = await RemoveFromMessageGroup();
      await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
      await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      string username = Context.User.GetUsername();
      if (username == createMessageDto.RecipientUsername.ToLower(CultureInfo.CurrentCulture))
      {
        throw new HubException(" You can not send message to your self");
      }

      AppUser sender = await _userRepository.GetUserByUsernameAsync(username);
      AppUser recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
      if (recipient == null)
      {
        throw new HubException("Not found user");
      }

      Message message = new()
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };
      string groupName = GroupName(sender.UserName, recipient.UserName);
      Group group = await _messageRepository.GetMessageGroup(groupName);
      if (group.Connections.Any(c => c.Username == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow;
      }
      else
      {
        List<string> connections = await _tracker.GetConnectionsForUser(recipient.UserName);
        if (connections != null)
        {
          await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
              new { username = sender.UserName, knownAs = sender.KnownAs });
        }

      }
      _messageRepository.AddMessage(message);
      if (await _messageRepository.SaveAllAsync())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
      }
    }
    private async Task<Group> AddToGroup(string groupName)
    {
      Group group = await _messageRepository.GetMessageGroup(groupName);
      Connection connection = new(Context.ConnectionId, Context.User.GetUsername());
      if (group == null)
      {
        group = new Group(groupName);
        _messageRepository.AddGroup(group);
      }
      group.Connections.Add(connection);
      return (await _messageRepository.SaveAllAsync()) ? group : throw new HubException("Failed to join group");
    }
    private async Task<Group> RemoveFromMessageGroup()
    {
      string connectionId = Context.ConnectionId;
      Group group = await _messageRepository.GetGroupForConnection(connectionId);
      Connection connection = group.Connections.FirstOrDefault(c => c.ConnectionId == connectionId);
      _messageRepository.RemoveConnection(connection);
      return (await _messageRepository.SaveAllAsync()) ? group : throw new HubException("Failed to remove from group");
    }
    private static string GroupName(string caller, string other)
    {
      bool stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
  }
}
