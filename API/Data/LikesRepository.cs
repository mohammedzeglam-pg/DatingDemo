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
  public class LikesRepository : ILikesRepository
  {
    private readonly DataContext _context;

    private readonly IMapper _mapper;
    public LikesRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
    {
      return await _context.Likes.FindAsync(sourceUserId, likedUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
      IQueryable<AppUser> users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
      IQueryable<UserLike> likes = _context.Likes.AsQueryable();
      if (likesParams.Predicate == "liked")
      {
        likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
        users = likes.Select(like => like.LikedUser);
      }

      if (likesParams.Predicate == "likedBy")
      {
        likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
        users = likes.Select(like => like.SourceUser);
      }
      IQueryable<LikeDto> likedUsers = users.ProjectTo<LikeDto>(_mapper.ConfigurationProvider);
      return await PagedList<LikeDto>.CreateAsync(likedUsers,
                                                  likesParams.PageNumber,
                                                  likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
      return await _context.Users
        .Include(x => x.LikedUsers)
        .FirstOrDefaultAsync(x => x.Id == userId);
    }
  }
}
