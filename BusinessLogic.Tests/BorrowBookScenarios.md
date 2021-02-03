# Feature: Borrow book


## Examples
- A registered member has no borrowed books and borrows "Harry potter" that has available copies
## Scenario: Successfully borrow a book

### Arrange:

- Member is registered
- Book has available copies
- Member has less than two borrowed books

### Act:

- Member borrows book


### Assert:
- Book copy is in member's borrowed book list
- One less copy available 


## Scenario: Can't borrow a book. Member not registered

### Arrange:

- Member is not registered
- Book has available copies
- Member has less than two borrowed books

### Act:

- Member borrows book

### Assert:

- Error message to the user
- Book is not borrowed

## Scenario: Can't borrow a book. No available copies

### Arrange:

- Book has no available copies
- Registered member

### Act:

- Member borrows the book

### Assert:

- Error message to the user