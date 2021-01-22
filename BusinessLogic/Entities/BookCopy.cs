using System;

namespace BusinessLayer.Entities
{
    /// <summary>
    /// Represents a copy of the book that can be borrowed by a member
    /// </summary>
    public class BookCopy
    {
        public Guid Id { get; set; }

        public Guid BookIsbn { get; set; }

        public Guid? BorrowedToMemberId { get; private set; }

        public void LoanTo(Guid memberId)
        {
            BorrowedToMemberId = memberId;
        }
    }
}