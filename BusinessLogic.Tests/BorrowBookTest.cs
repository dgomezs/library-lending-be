using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using BusinessLayer.Entities;
using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using Moq;
using Xunit;

namespace BusinessLogic.Tests
{
    public class BorrowBookTest
    {
        private readonly IBookCopyRepository bookCopyRepository;
        private readonly IBorrowBookService borrowBookService;
        private readonly Mock<IMemberService> memberService;

        public BorrowBookTest()
        {
            memberService = new Mock<IMemberService>();
            bookCopyRepository = new BookCopyInMemoryRepository();
            borrowBookService = new BorrowBookService(memberService.Object, bookCopyRepository);
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

            // Arrange
            MemberIsRegistered(memberId);
            await BookHasNoAvailableCopies(bookIsbn);
            await MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);


            // Act
            await Assert.ThrowsAsync<NoAvailableBookCopiesException>(() => BorrowBook(memberId, bookIsbn));
        }

        private async Task BookHasNoAvailableCopies(Guid bookIsbn)
        {
            await BookHasAvailableCopies(bookIsbn, new List<BookCopy>());
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
                await bookCopyRepository.SaveBookCopy(bookCopy);
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
            var availableCopiesByBookId = await bookCopyRepository.GetAvailableCopiesByBookId(bookIsbn);
            Assert.Equal(numberOfAvailableCopies, availableCopiesByBookId.Count);
        }


        private async Task<List<BookCopy>> GetBorrowedBooksByMember(Guid memberId)
        {
            return await bookCopyRepository.GetBorrowedBookCopiesByMember(memberId);
        }
    }
}