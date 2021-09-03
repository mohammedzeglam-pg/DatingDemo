using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class MessagesController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
      _userRepository = userRepository;
      _messageRepository = messageRepository;
      _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
      string username = User.GetUsername();
      if (username == createMessageDto.RecipientUsername.ToLower(CultureInfo.CurrentCulture))
      {
        return BadRequest(" You can not send message to your self");
      }

      AppUser sender = await _userRepository.GetUserByUsernameAsync(username);
      AppUser recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
      if (recipient == null)
      {
        return NotFound();
      }

      Message message = new()
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };
      _messageRepository.AddMessage(message);
      return await _messageRepository.SaveAllAsync() ? Ok(_mapper.Map<MessageDto>(message)) : BadRequest("failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
      messageParams.UserName = User.GetUsername();
      PagedList<MessageDto> messages = await _messageRepository.GetMessagesForUser(messageParams);
      Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
      return messages;
    }
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessageThread(string username)
    {
      string currentUsername = User.GetUsername();
      return base.Ok(await _messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
      string username = User.GetUsername();
      Message message = await _messageRepository.GetMessage(id);
      if (message.SenderUsername != username && message.RecipientUsername != username)
      {
        return NotFound();
      }

      if (message.SenderUsername == username)
      {
        message.SenderDeleted = true;
      }

      if (message.RecipientUsername == username)
      {
        message.RecipientDeleted = true;
      }
      if (message.RecipientDeleted && message.SenderDeleted)
      {
        _messageRepository.DeleteMessage(message);
      }
      return await _messageRepository.SaveAllAsync() ? Ok() : BadRequest("Problem when deleteing message");
    }
  }
}
