using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using BusinessLayer.Repositories;
using BusinessLayer.Services.BorrowBook;
using BusinessLayer.Services.Member;
using Moq;
using Xunit;

namespace BusinessLogic.Tests
{
    public class BorrowBookMemoryDbInRepositoryTest
    {
        private readonly BookCopyMemoryDb bookCopyMemoryDb;
        private readonly Mock<IBookCopyRepository> bookCopyRepository;
        private readonly IBorrowBookService borrowBookService;
        private readonly Mock<IMemberService> memberService;

        public BorrowBookMemoryDbInRepositoryTest()
        {
            bookCopyMemoryDb = new BookCopyMemoryDb();
            bookCopyRepository = new Mock<IBookCopyRepository>();
            memberService = new Mock<IMemberService>();
            borrowBookService = new BorrowBookService(memberService.Object, bookCopyRepository.Object);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(1)]
        public async Task ShouldBorrowABook(int numberOfAvailableCopies)
        {
            var memberId = GenerateRandomMemberId();
            var bookIsbn = GenerateRandomBookIsbn();
            var currentBorrowedBooks = new List<BookCopy>();
            var availableCopies = BookCopyMockData.GenerateRandomAvailableCopies(bookIsbn, numberOfAvailableCopies);

            // Arrange
            SetupRepositoryToUseInMemoryDb();

            MemberIsRegistered(memberId);
            await BookHasAvailableCopies(bookIsbn, availableCopies);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);

            // Act
            var bookCopyId = await BorrowBook(memberId, bookIsbn);

            // Assert
            await VerifyBookCopyIsInMemberBorrowedList(memberId, bookCopyId);
            await VerifyBorrowedBookByMemberSizeIs(1, memberId);
            await VerifyNumberOfCopiesAvailableForBook(bookIsbn, numberOfAvailableCopies - 1);
        }

        private void SetupRepositoryToUseInMemoryDb()
        {
            bookCopyRepository.Setup(m => m.UpdateBookCopy(It.IsAny<BookCopy>()))
                .Callback<BookCopy>(bookCopy => { bookCopyMemoryDb.SaveBookCopy(bookCopy); })
                .Returns(Task.CompletedTask);
            bookCopyRepository.Setup(m => m.GetBorrowedBookCopiesByMember(It.IsAny<Guid>()))
                .Returns<Guid>(memberId => Task.FromResult(bookCopyMemoryDb.GetBorrowedBookCopiesByMember(memberId)));

            bookCopyRepository.Setup(m => m.GetAvailableCopiesByBookId(It.IsAny<Guid>()))
                .Returns<Guid>(bookIsbn => Task.FromResult(bookCopyMemoryDb.GetAvailableCopiesByBookId(bookIsbn)));
        }

        private async Task<Guid> BorrowBook(Guid memberId, Guid bookIsbn)
        {
            return await borrowBookService.BorrowBook(memberId, bookIsbn);
        }


        private void MemberIsRegistered(Guid memberId)
        {
            memberService.Setup(c => c.MemberIsRegistered(memberId)).ReturnsAsync(true);
        }


        private async Task VerifyBorrowedBookByMemberSizeIs(int numberOfBooks, Guid memberId)
        {
            var borrowedBooks = await GetBorrowedBooksByMember(memberId);
            Assert.Equal(numberOfBooks, borrowedBooks.Count);
        }

        private async Task VerifyBookCopyIsInMemberBorrowedList(Guid memberId, Guid bookCopyId)
        {
            var borrowedBooks = await GetBorrowedBooksByMember(memberId);
            Assert.Contains(borrowedBooks, b => b.Id.Equals(bookCopyId));
        }

        private async Task MemberHasNumberBorrowedBooks(Guid memberId, List<BookCopy> currentBorrowedBooks)
        {
            foreach (var bookCopy in currentBorrowedBooks)
            {
                bookCopy.LoanTo(memberId);
                await bookCopyRepository.Object.UpdateBookCopy(bookCopy);
            }
        }

        private async Task BookHasAvailableCopies(Guid bookIsbn, List<BookCopy> availableBookCopies)
        {
            foreach (var bookCopy in availableBookCopies)
            {
                bookCopy.BookIsbn = bookIsbn;
                await bookCopyRepository.Object.UpdateBookCopy(bookCopy);
            }
        }


        private async Task VerifyNumberOfCopiesAvailableForBook(Guid bookIsbn, int numberOfAvailableCopies)
        {
            var availableCopiesByBookId = await bookCopyRepository.Object.GetAvailableCopiesByBookId(bookIsbn);
            Assert.Equal(numberOfAvailableCopies, availableCopiesByBookId.Count);
        }

        private async Task<List<BookCopy>> GetBorrowedBooksByMember(Guid memberId)
        {
            return await borrowBookService.GetBorrowedBookCopiesByMember(memberId);
        }

        private Guid GenerateRandomBookIsbn()
        {
            return Guid.NewGuid();
        }

        private Guid GenerateRandomMemberId()
        {
            return Guid.NewGuid();
        }
    }
}