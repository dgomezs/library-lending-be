using System;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface IBorrowBookService
    {
        Task<Guid> BorrowBook(Guid memberId, Guid bookId);
    }
}