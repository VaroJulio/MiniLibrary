using MediatR;
using MiniLibrary.Application.Loans.DTOs;

namespace MiniLibrary.Application.Loans.Commands.CheckOutBook;

/// <summary>
/// Command to check out a book for a member.
/// </summary>
public record CheckOutBookCommand(Guid BookId, Guid UserId) : IRequest<LoanResponse>;
