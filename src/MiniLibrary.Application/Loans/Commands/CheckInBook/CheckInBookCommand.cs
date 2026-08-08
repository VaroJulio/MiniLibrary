using MediatR;
using MiniLibrary.Application.Loans.DTOs;

namespace MiniLibrary.Application.Loans.Commands.CheckInBook;

/// <summary>
/// Command to check in (return) a book.
/// UserId is the currently authenticated user.
/// </summary>
public record CheckInBookCommand(Guid BookId, Guid UserId) : IRequest<LoanResponse>;
