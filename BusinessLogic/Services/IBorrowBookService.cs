using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;

namespace BusinessLayer.Services
{
    public interface IBorrowBookService
    {
        Task<Guid> BorrowBook(Guid memberId, Guid bookId);
        Task<List<BookCopy>> GetBorrowedBookCopiesByMember(Guid memberId);
    }
}