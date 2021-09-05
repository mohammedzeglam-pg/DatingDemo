using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{

  public class MessageRepository : IMessageRepository
  {

    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public void AddMessage(Message message)
    {
      _ = _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _ = _context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages.FindAsync(id);
    }

    public Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
      IQueryable<Message> query = _context.Messages.OrderByDescending(m => m.MessageSend)
                                   .AsQueryable();
      query = messageParams.Container switch
      {
        "Inbox" => query.Where(m => m.RecipientUsername == messageParams.UserName && m.RecipientDeleted == false),
        "Outbox" => query.Where(m => m.SenderUsername == messageParams.UserName && m.SenderDeleted == false),
        _ => query.Where(m => m.RecipientUsername == messageParams.UserName && m.RecipientDeleted == false && m.DateRead == null),
      };

      IQueryable<MessageDto> messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
      return PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {

      List<Message> messages = await _context.Messages
        .Include(m => m.Sender)
        .ThenInclude(u => u.Photos)
        .Include(m => m.Recipient)
        .ThenInclude(u => u.Photos)
        .Where(m =>
          (m.RecipientUsername == currentUsername && m.RecipientDeleted == false
          && m.SenderUsername == recipientUsername)
          || (m.RecipientUsername == recipientUsername
          && m.SenderUsername == currentUsername && m.SenderDeleted == false)
          ).OrderBy(m => m.MessageSend)
        .ToListAsync();
      List<Message> unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();
      if (unreadMessages.Any())
      {
        foreach (Message message in unreadMessages)
        {
          message.DateRead = DateTime.UtcNow;
        }
        _ = await _context.SaveChangesAsync();
      }
      return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public void AddGroup(Group group)
    {
      _ = _context.Groups.Add(group);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
      return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
      return await _context.Groups.Include(g => g.Connections).FirstOrDefaultAsync(g => g.Name == groupName);
    }

    public void RemoveConnection(Connection connection)
    {
      _ = _context.Connections.Remove(connection);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
      return await _context.Groups
        .Include(g => g.Connections)
        .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
        .FirstOrDefaultAsync();
    }
  }
}
