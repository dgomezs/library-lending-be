using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories;
using BusinessLayer.Services;
using BusinessLayer.Services.BorrowBook;
using BusinessLayer.Services.Member;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BusinessLogic.Tests
{
    public class BorrowBookTest
    {
        private readonly IBookCopyRepository bookCopyRepository;
        private readonly IBorrowBookService borrowBookService;
        private readonly LoggerDouble<BorrowBookService> logger;
        private readonly Mock<IMemberService> memberService;

        public BorrowBookTest()
        {
            var builder = new DbContextOptionsBuilder<LibraryContext>();
            builder.UseInMemoryDatabase("library");
            var libraryContext = new LibraryContext(builder.Options);

            logger = new LoggerDouble<BorrowBookService>();
            bookCopyRepository = new FakeBookCopyRepository();
            //bookCopyRepository = new EfBookCopyRepository(libraryContext);
            memberService = new Mock<IMemberService>();
            borrowBookService = new BorrowBookService(logger, memberService.Object, bookCopyRepository);
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

        [Fact]
        public async Task ShouldNotBorrowABookIfMemberIsNotRegistered()
        {
            var memberId = GenerateRandomMemberId();
            var bookIsbn = GenerateRandomBookIsbn();

            // Arrange
            MemberIsNotRegistered(memberId);

            // Act
            await Assert.ThrowsAsync<MemberNotRegisteredException>(() => BorrowBook(memberId, bookIsbn));
        }

        [Fact]
        public async Task ShouldNotBorrowABookIfMemberHasAlreadyMaxNumberOfBorrowedBooks()
        {
            var memberId = GenerateRandomMemberId();
            var bookIsbn = GenerateRandomBookIsbn();
            var currentBorrowedBooks =
                BookCopyMockData.GenerateRandomAvailableCopies(bookIsbn, Constants.MaxBorrowedBooks);
            var numberOfAvailableCopies = 2;
            var availableCopies = BookCopyMockData.GenerateRandomAvailableCopies(bookIsbn, numberOfAvailableCopies);

            // Arrange
            MemberIsRegistered(memberId);
            await BookHasAvailableCopies(bookIsbn, availableCopies);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);


            // Act
            await Assert.ThrowsAsync<MaxBorrowedBooksExceededException>(() => BorrowBook(memberId, bookIsbn));
        }

        [Fact]
        public async Task ShouldNotBorrowABookIfNoAvailableCopies()
        {
            var memberId = GenerateRandomMemberId();
            var bookIsbn = GenerateRandomBookIsbn();
            var currentBorrowedBooks = new List<BookCopy>();

            // Arrange
            MemberIsRegistered(memberId);
            await BookHasNoAvailableCopies(bookIsbn);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);


            // Act
            await Assert.ThrowsAsync<NoAvailableBookCopiesException>(() => BorrowBook(memberId, bookIsbn));
        }


        [Fact]
        public async Task ShouldLogAnErrorIfMemberHasMoreThanMaxNumberOfBorrowedBooks()
        {
            var memberId = GenerateRandomMemberId();
            var bookIsbn = GenerateRandomBookIsbn();
            var currentBorrowedBooks =
                BookCopyMockData.GenerateRandomAvailableCopies(bookIsbn, Constants.MaxBorrowedBooks + 1);
            var numberOfAvailableCopies = 2;
            var availableCopies = BookCopyMockData.GenerateRandomAvailableCopies(bookIsbn, numberOfAvailableCopies);

            // Arrange
            MemberIsRegistered(memberId);
            await BookHasAvailableCopies(bookIsbn, availableCopies);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);

            // Act
            await Assert.ThrowsAsync<MaxBorrowedBooksExceededException>(() => BorrowBook(memberId, bookIsbn));
            var errorEntries = logger.ErrorEntries.ToList();
            Assert.True(errorEntries.Count == 1);
        }

        private async Task BookHasNoAvailableCopies(Guid bookIsbn)
        {
            await BookHasAvailableCopies(bookIsbn, new List<BookCopy>());
        }


        private async Task<Guid> BorrowBook(Guid memberId, Guid bookIsbn)
        {
            return await borrowBookService.BorrowBook(memberId, bookIsbn);
        }


        private void MemberIsRegistered(Guid memberId)
        {
            memberService.Setup(c => c.MemberIsRegistered(memberId)).ReturnsAsync(true);
        }

        private void MemberIsNotRegistered(Guid memberId)
        {
            memberService.Setup(c => c.MemberIsRegistered(memberId)).ReturnsAsync(false);
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
                await bookCopyRepository.UpdateBookCopy(bookCopy);
            }

            await bookCopyRepository.Save();
        }

        private async Task BookHasAvailableCopies(Guid bookIsbn, List<BookCopy> availableBookCopies)
        {
            foreach (var bookCopy in availableBookCopies)
            {
                bookCopy.BookIsbn = bookIsbn;
                await bookCopyRepository.UpdateBookCopy(bookCopy);
            }

            await bookCopyRepository.Save();
        }


        private async Task VerifyNumberOfCopiesAvailableForBook(Guid bookIsbn, int numberOfAvailableCopies)
        {
            var availableCopiesByBookId = await bookCopyRepository.GetAvailableCopiesByBookId(bookIsbn);
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