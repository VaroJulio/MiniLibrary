using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.Commands.DeleteBook;

/// <summary>
/// Handles soft-deletion of a book from the catalog.
/// Validates book existence and checks for active loans before deletion.
/// </summary>
public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;

    public DeleteBookCommandHandler(IBookRepository bookRepository, ILoanRepository loanRepository)
    {
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
    }

    public async Task Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        // Get book by Id
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException("Book", request.Id);
        }

        // Check for active loans
        var activeLoan = await _loanRepository.GetActiveLoanByBookAsync(request.Id, cancellationToken);
        if (activeLoan is not null)
        {
            throw new ConflictException("Cannot delete book because it has active loans.");
        }

        // Soft-delete the book
        book.Delete();

        await _bookRepository.DeleteAsync(book, cancellationToken);
    }
}
