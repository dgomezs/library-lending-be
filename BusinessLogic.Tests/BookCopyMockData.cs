using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using BusinessLayer.Entities;

namespace BusinessLogic.Tests
{
    public class BookCopyMockData
    {
        public static List<BookCopy> GenerateRandomAvailableCopies(Guid bookIsbn, int numberOfAvailableCopies)
        {
            var fakeBookCopy = new Faker<BookCopy>()
                .RuleFor(b => b.Id, b => Guid.NewGuid())
                .RuleFor(b => b.BookIsbn, bookIsbn);

            return Enumerable.Range(0, numberOfAvailableCopies)
                .Select(_ => fakeBookCopy.Generate()).ToList();
        }
    }
}