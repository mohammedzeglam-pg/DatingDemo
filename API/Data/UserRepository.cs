using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
      return await _context.Users
        .Where(user => user.UserName == username)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();

    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
      DateTime minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
      DateTime maxDob = DateTime.Today.AddYears(-userParams.MinAge);

      IQueryable<AppUser> userQuery = _context.Users
        .Where(u => u.UserName != userParams.CurrentUsername)
        .Where(u => u.Gender == userParams.Gender)
        .Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

      userQuery = userParams.OrderBy switch
      {
        "Created" => userQuery.OrderByDescending(u => u.Created),
        _ => userQuery.OrderByDescending(u => u.LastActive)
      };

      IQueryable<MemberDto> query = userQuery
      .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
      .AsNoTracking();
      return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
      return await _context.Users
        .Include(entity => entity.Photos)
        .SingleOrDefaultAsync(user => user.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      return await _context.Users
        .Include(entity => entity.Photos)
        .ToListAsync();
    }

    public async Task<bool> SaveAllChangesAsync()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public void UpdateUser(AppUser user)
    {
      _context.Entry(user).State = EntityState.Modified;
    }
  }
}
