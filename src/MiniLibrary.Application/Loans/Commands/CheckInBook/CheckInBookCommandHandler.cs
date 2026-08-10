using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Loans.DTOs;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Loans.Commands.CheckInBook;

/// <summary>
/// Handles the check-in (return) of a book.
/// Members can only check-in their own loans; Librarians/Admins can check-in any loan.
/// Sets ReturnedAt, changes book status to Available, and dispatches BookReturnedEvent.
/// </summary>
public class CheckInBookCommandHandler : IRequestHandler<CheckInBookCommand, LoanResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CheckInBookCommandHandler(
        IBookRepository bookRepository,
        ILoanRepository loanRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanResponse> Handle(CheckInBookCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the book; throw if not found
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), request.BookId);

        // 2. Determine if the current user is a Librarian or Admin (can return any loan)
        var isStaff = _currentUserService.Role is UserRole.Librarian or UserRole.Admin;

        // 3. Find the active loan for this book
        BookLoan? loan;

        if (isStaff)
        {
            // Librarian/Admin can check-in any active loan for this book
            loan = await _loanRepository.GetActiveLoanByBookAsync(request.BookId, cancellationToken);
        }
        else
        {
            // Member can only check-in their own loan
            loan = await _loanRepository.GetActiveLoanAsync(request.BookId, request.UserId, cancellationToken);
        }

        // 4. If no active loan found, determine the appropriate error
        if (loan is null)
        {
            if (!isStaff)
            {
                // Check if there IS an active loan for this book but by another user
                var anyLoan = await _loanRepository.GetActiveLoanByBookAsync(request.BookId, cancellationToken);
                if (anyLoan is not null)
                {
                    // Book has an active loan but not by this member → 403
                    throw new UnauthorizedAccessException("You can only check in books that you have borrowed.");
                }
            }

            // No active loan exists for this book at all → 409
            throw new ConflictException("This book does not have an active loan.");
        }

        // 5. Set ReturnedAt = now (domain method also raises BookReturnedEvent)
        var now = DateTime.UtcNow;
        loan.Return(now);

        // 6. Change book status to Available
        book.MakeAvailable();

        // 7. Persist changes atomically
        await _loanRepository.UpdateAsync(loan, cancellationToken);
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // 8. Return the loan response
        return new LoanResponse(
            Id: loan.Id,
            BookId: loan.BookId,
            BookTitle: book.Title,
            BorrowedAt: loan.BorrowedAt,
            DueDate: loan.DueDate,
            ReturnedAt: loan.ReturnedAt);
    }
}
