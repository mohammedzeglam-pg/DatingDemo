using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;

namespace API.Interfaces{

  public interface IUserRepository
  {
    public void UpdateUser(AppUser user);
    public Task<bool> SaveAllChangesAsync();
    public Task<IEnumerable<AppUser>> GetUsersAsync();
    public Task<AppUser> GetUserByIdAsync(int id);
    public Task<AppUser> GetUserByUsernameAsync(string username);
    public Task<MemberDto> GetMemberAsync(string username);
    public Task<IEnumerable<MemberDto>> GetMembersAsync();
  }
}
