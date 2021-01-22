using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Services
{
    public class BorrowBookService : IBorrowBookService
    {
        private readonly IMemberService memberService;
        private readonly IBookCopyRepository bookCopyRepository;

        public BorrowBookService(IMemberService memberService, IBookCopyRepository bookCopyRepository)
        {
            this.memberService = memberService;
            this.bookCopyRepository = bookCopyRepository;
        }


        public async Task<Guid> BorrowBook(Guid memberId, Guid bookId)
        {
            var memberIsRegistered = await this.memberService.MemberIsRegistered(memberId);
            if (!memberIsRegistered)
            {
                throw new MemberNotRegisteredException($"The member with id ${memberId.ToString()} is not registered");
            }

            var availableCopies = await bookCopyRepository.GetAvailableCopiesByBookId(bookId);

            if (!availableCopies.Any())
            {
                throw new NoAvailableBookCopiesException($"The book ${bookId} has no available copies");
            }

            var borrowedBooksByMember = await GetBorrowedBookCopiesByMember(memberId);

            if (borrowedBooksByMember.Count >= Constants.MaxBorrowedBooks)
            {
                throw new MaxBorrowedBooksExceededException(
                    $"Member ${memberId} has already borrowed ${Constants.MaxBorrowedBooks} books");
            }

            var copyToBorrow = SelectCopyToBorrow(availableCopies);

            copyToBorrow.LoanTo(memberId);

            await this.bookCopyRepository.SaveBookCopy(copyToBorrow);

            return copyToBorrow.Id;
        }

        private BookCopy SelectCopyToBorrow(List<BookCopy> availableCopies)
        {
            // no specific algorithm to select which copy to borrow just pick one
            return availableCopies[0];
        }

        public Task<List<BookCopy>> GetBorrowedBookCopiesByMember(Guid memberId)
        {
            return bookCopyRepository.GetBorrowedBookCopiesByMember(memberId);
        }
    }
}