using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.Extensions;
using BusinessLayer.Entities;
using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using Moq;
using Xunit;

namespace BusinessLayer.Tests
{
    public class BorrowBookTest
    {
        private readonly IBorrowBookService borrowBookService;
        private readonly Mock<IMemberService> memberService;
        private readonly IBookCopyRepository bookCopyRepository;

        public BorrowBookTest()
        {
            this.memberService = new Mock<IMemberService>();
            this.bookCopyRepository = new BookCopyInMemoryRepository();
            this.borrowBookService = new BorrowBookService(memberService.Object, bookCopyRepository);
        }


        [Theory]
        [InlineData(2)]
        [InlineData(1)]
        public async Task ShouldBorrowABook(int numberOfAvailableCopies)
        {
            var memberId = GenerateRandomMemberId();
            var bookIsbn = GenerateRandomBookIsbn();
            var currentBorrowedBooks = new List<BookCopy>();
            var availableCopies = GenerateRandomAvailableCopies(bookIsbn, numberOfAvailableCopies);

            // Arrange
            MemberIsRegistered(memberId);
            await BookHasAvailableCopies(bookIsbn, availableCopies);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);

            // Act
            var bookCopyId = await BorrowBook(memberId, bookIsbn);

            // Assert
            var borrowedBooks = await GetBorrowedBooksByMember(memberId);
            VerifyBookCopyIsInMemberBorrowedList(borrowedBooks, bookCopyId);
            VerifyBorrowedBookByMemberSizeIs(1, borrowedBooks);
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
            var currentBorrowedBooks = GenerateRandomAvailableCopies(bookIsbn, Constants.MaxBorrowedBooks);
            var numberOfAvailableCopies = 2;
            var availableCopies = GenerateRandomAvailableCopies(bookIsbn, numberOfAvailableCopies);

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
            var availableCopies = new List<BookCopy>();

            // Arrange
            MemberIsRegistered(memberId);
            await BookHasAvailableCopies(bookIsbn, availableCopies);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);


            // Act
            await Assert.ThrowsAsync<NoAvailableBookCopiesException>(() => BorrowBook(memberId, bookIsbn));
        }


        private static void VerifyBorrowedBookByMemberSizeIs(int numberOfBooks, List<BookCopy> borrowedBooks)
        {
            Assert.Equal(numberOfBooks, borrowedBooks.Count);
        }

        private static void VerifyBookCopyIsInMemberBorrowedList(List<BookCopy> borrowedBooks, Guid bookCopyId)
        {
            Assert.Contains(borrowedBooks, b => b.Id.Equals(bookCopyId));
        }

        private List<BookCopy> GenerateRandomAvailableCopies(Guid bookIsbn, int numberOfAvailableCopies)
        {
            var fakeBookCopy = new Faker<BookCopy>()
                .RuleFor(b => b.Id, b => Guid.NewGuid())
                .RuleFor(b => b.BookIsbn, bookIsbn);

            return Enumerable.Range(0, numberOfAvailableCopies)
                .Select(_ => fakeBookCopy.Generate()).ToList();
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

        private async Task MemberHasNumberBorrowedBooks(Guid memberId, List<BookCopy> currentBorrowedBooks)
        {
            foreach (var bookCopy in currentBorrowedBooks)
            {
                bookCopy.LoanTo(memberId);
                await this.bookCopyRepository.SaveBookCopy(bookCopy);
            }
        }

        private async Task BookHasAvailableCopies(Guid bookIsbn, List<BookCopy> availableBookCopies)
        {
            foreach (var bookCopy in availableBookCopies)
            {
                bookCopy.BookIsbn = bookIsbn;
                await bookCopyRepository.SaveBookCopy(bookCopy);
            }
        }

        private Guid GenerateRandomBookIsbn()
        {
            return Guid.NewGuid();
        }

        private Guid GenerateRandomMemberId()
        {
            return Guid.NewGuid();
        }


        private async Task VerifyNumberOfCopiesAvailableForBook(Guid bookIsbn, int numberOfAvailableCopies)
        {
            var availableCopiesByBookId = await this.bookCopyRepository.GetAvailableCopiesByBookId(bookIsbn);
            Assert.Equal(numberOfAvailableCopies, availableCopiesByBookId.Count);
        }


        private async Task<List<BookCopy>> GetBorrowedBooksByMember(Guid memberId)
        {
            return await borrowBookService.GetBorrowedBookCopiesByMember(memberId);
        }
    }
}