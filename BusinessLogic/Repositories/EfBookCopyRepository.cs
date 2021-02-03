using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repositories
{
    public class EfBookCopyRepository : IBookCopyRepository
    {
        private readonly LibraryContext _libraryContext;

        public EfBookCopyRepository(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        public Task<List<BookCopy>> GetBorrowedBookCopiesByMember(Guid memberId)
        {
            return _libraryContext.BookCopies.Where(b => memberId.Equals(b.BorrowedToMemberId)).ToListAsync();
        }

        public Task UpdateBookCopy(BookCopy bookCopy)
        {
            if (_libraryContext.Entry(bookCopy).State == EntityState.Detached) _libraryContext.BookCopies.Add(bookCopy);
            return Task.CompletedTask;
        }

        public Task Save()
        {
            return _libraryContext.SaveChangesAsync();
        }

        public Task<List<BookCopy>> GetAvailableCopiesByBookId(Guid bookIsbn)
        {
            return _libraryContext.BookCopies.Where(b => b.BookIsbn == bookIsbn && b.BorrowedToMemberId == null)
                .ToListAsync();
        }
    }
}