# Feature: Borrow book

## Scenario: Successfully borrow a book

### Arrange:
- Member is valid
- Book has available copies
- Member has less than two borrowed books


### Act:

- Member borrows book


### Assert:

- Book copy is in member's borrowed book list
- One less copy available 


## Scenario: Can't borrow a book. Member not registered

### Arrange:
- Member is not registered valid
- Book has available copies
- Member has less than two borrowed books


### Act:

- Member borrows book


### Assert:

- Error message to the user
- Book is not borrowed
