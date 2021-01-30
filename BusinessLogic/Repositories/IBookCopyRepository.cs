using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;

namespace BusinessLayer.Repositories
{
    public interface IBookCopyRepository
    {
        Task<List<BookCopy>> GetBorrowedBookCopiesByMember(Guid memberId);
        Task SaveBookCopy(BookCopy bookCopy);

        Task<List<BookCopy>> GetAvailableCopiesByBookId(Guid bookIsbn);
    }
}