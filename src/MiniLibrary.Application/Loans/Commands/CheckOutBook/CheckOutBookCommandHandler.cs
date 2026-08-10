using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Loans.DTOs;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Loans.Commands.CheckOutBook;

/// <summary>
/// Handles the check-out of a book for a member.
/// Verifies preconditions, creates the loan, updates book status,
/// handles optimistic concurrency, and auto-removes from wishlist.
/// All changes are committed atomically via IUnitOfWork.
/// </summary>
public class CheckOutBookCommandHandler : IRequestHandler<CheckOutBookCommand, LoanResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckOutBookCommandHandler(
        IBookRepository bookRepository,
        ILoanRepository loanRepository,
        IWishlistRepository wishlistRepository,
        IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
        _wishlistRepository = wishlistRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanResponse> Handle(CheckOutBookCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the book; throw if not found
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), request.BookId);

        // 2. Verify book is available
        if (book.Status != BookStatus.Available)
        {
            throw new ConflictException("This book is not available for checkout.");
        }

        // 3. Verify user has fewer than 5 active loans
        var activeLoanCount = await _loanRepository.GetActiveLoanCountAsync(request.UserId, cancellationToken);
        if (activeLoanCount >= 5)
        {
            throw new ConflictException("You have reached the maximum limit of 5 simultaneous loans.");
        }

        // 4. Change book status to CheckedOut (domain method enforces Available → CheckedOut)
        book.CheckOut();

        // 5. Create the BookLoan record
        var now = DateTime.UtcNow;
        var loan = BookLoan.Create(request.BookId, request.UserId, now);

        // 6. Stage changes (no SaveChanges yet)
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _loanRepository.AddAsync(loan, cancellationToken);

        // 7. If book is in user's wishlist, auto-remove it (Req 18.9)
        var wishlistEntry = await _wishlistRepository.GetEntryAsync(request.UserId, request.BookId, cancellationToken);
        if (wishlistEntry is not null)
        {
            await _wishlistRepository.DeleteAsync(wishlistEntry, cancellationToken);
        }

        // 8. Commit all changes atomically (single SaveChanges)
        try
        {
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "This book was checked out by another user. Please try again.");
        }

        // 9. Return the loan response
        return new LoanResponse(
            Id: loan.Id,
            BookId: loan.BookId,
            BookTitle: book.Title,
            BorrowedAt: loan.BorrowedAt,
            DueDate: loan.DueDate,
            ReturnedAt: loan.ReturnedAt);
    }
}
