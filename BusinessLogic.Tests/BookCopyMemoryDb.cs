using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.Entities;

namespace BusinessLogic.Tests
{
    public class BookCopyMemoryDb
    {
        private readonly Dictionary<Guid, BookCopy> catalog = new Dictionary<Guid, BookCopy>();

        public List<BookCopy> GetBorrowedBookCopiesByMember(Guid memberId)
        {
            return catalog.Values.Where(b => memberId.Equals(b.BorrowedToMemberId)).ToList();
        }

        public void SaveBookCopy(BookCopy bookCopy)
        {
            var existsCopy = catalog.ContainsKey(bookCopy.Id);

            if (existsCopy) catalog.Remove(bookCopy.Id);

            catalog.Add(bookCopy.Id, bookCopy);
        }

        public List<BookCopy> GetAvailableCopiesByBookId(Guid bookIsbn)
        {
            return catalog.Values.Where(b => bookIsbn.Equals(b.BookIsbn) && !b.BorrowedToMemberId.HasValue)
                .ToList();
        }
    }
}