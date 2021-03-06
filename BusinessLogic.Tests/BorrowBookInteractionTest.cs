using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using BusinessLayer.Repositories;
using BusinessLayer.Services.BorrowBook;
using BusinessLayer.Services.Member;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessLogic.Tests
{
    public class BorrowBookInteractionTest
    {
        private readonly Mock<IBookCopyRepository> bookCopyRepository;
        private readonly IBorrowBookService borrowBookService;
        private readonly Mock<ILogger<BorrowBookService>> logger;
        private readonly Mock<IMemberService> memberService;

        public BorrowBookInteractionTest()
        {
            bookCopyRepository = new Mock<IBookCopyRepository>();
            memberService = new Mock<IMemberService>();
            logger = new Mock<ILogger<BorrowBookService>>();
            borrowBookService = new BorrowBookService(logger.Object, memberService.Object, bookCopyRepository.Object);
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
            BookHasAvailableCopies(bookIsbn, availableCopies);
            MemberHasNumberBorrowedBooks(memberId, currentBorrowedBooks);

            // Act
            var bookCopyId = await BorrowBook(memberId, bookIsbn);

            // Assert
            VerifyBookCopyIsInMemberBorrowedList(memberId, bookIsbn);
        }


        private void BookHasNoAvailableCopies(Guid bookIsbn)
        {
            BookHasAvailableCopies(bookIsbn, new List<BookCopy>());
        }

        private void VerifyBookCopyIsInMemberBorrowedList(Guid memberId, Guid bookIsbn)
        {
            bookCopyRepository.Verify(
                m => m.UpdateBookCopy(BookCopyBelongsToMember(memberId, bookIsbn)), Times.Once());
            bookCopyRepository.Verify(m => m.Save(), Times.Once);
        }

        private static BookCopy BookCopyBelongsToMember(Guid memberId, Guid bookIsbn)
        {
            return It.Is<BookCopy>(b =>
                memberId.Equals(b.BorrowedToMemberId) && bookIsbn.Equals(b.BookIsbn));
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

        private void MemberHasNumberBorrowedBooks(Guid memberId, List<BookCopy> currentBorrowedBooks)
        {
            bookCopyRepository.Setup(m => m.GetBorrowedBookCopiesByMember(memberId))
                .ReturnsAsync(currentBorrowedBooks);
        }

        private void BookHasAvailableCopies(Guid bookIsbn, List<BookCopy> availableBookCopies)
        {
            bookCopyRepository.Setup(m => m.GetAvailableCopiesByBookId(bookIsbn))
                .ReturnsAsync(availableBookCopies);
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