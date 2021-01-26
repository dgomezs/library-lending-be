using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using BusinessLayer.Services;

namespace BusinessLayer.Tests
{
    public class BookCopyInMemoryRepository : IBookCopyRepository
    {
        private Dictionary<Guid, BookCopy> catalog = new Dictionary<Guid, BookCopy>();

        public Task<List<BookCopy>> GetBorrowedBookCopiesByMember(Guid memberId)
        {
            var bookCopies = catalog.Values.Where(b => memberId.Equals(b.BorrowedToMemberId)).ToList();
            return Task.FromResult(bookCopies);
        }

        public Task SaveBookCopy(BookCopy bookCopy)
        {
            var existsCopy = catalog.ContainsKey(bookCopy.Id);

            if (existsCopy)
            {
                catalog.Remove(bookCopy.Id);
            }

            catalog.Add(bookCopy.Id, bookCopy);
            return Task.CompletedTask;
        }

        public Task<List<BookCopy>> GetAvailableCopiesByBookId(Guid bookIsbn)
        {
            var bookCopies = catalog.Values.Where(b => bookIsbn.Equals(b.BookIsbn) && !b.BorrowedToMemberId.HasValue)
                .ToList();
            return Task.FromResult(bookCopies);
        }
    }
}