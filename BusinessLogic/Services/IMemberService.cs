using System;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface IMemberService
    {
        Task<bool> MemberIsRegistered(Guid memberId);
    }
}