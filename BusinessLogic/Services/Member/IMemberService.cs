using System;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Member
{
    public interface IMemberService
    {
        Task<bool> MemberIsRegistered(Guid memberId);
    }
}