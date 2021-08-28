using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;

namespace API.Interfaces

{

  public interface IUserRepository
  {
    public void UpdateUser(AppUser user);
    public Task<bool> SaveAllChangesAsync();
    public Task<IEnumerable<AppUser>> GetUsersAsync();
    public Task<AppUser> GetUserByIdAsync(int id);
    public Task<AppUser> GetUserByUsernameAsync(string username);
    public Task<MemberDto> GetMemberAsync(string username);
    public Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
  }
}
