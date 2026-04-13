using LiveExpert.Application.Features.Withdrawals.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class WithdrawalsController : BaseController
{
    public WithdrawalsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Request a withdrawal (Tutor only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] RequestWithdrawalCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get withdrawal history
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWithdrawals([FromQuery] GetWithdrawalsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}

[Authorize]
[Route("api/bank-accounts")]
public class BankAccountsController : BaseController
{
    public BankAccountsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Add a bank account
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddBankAccount([FromBody] AddBankAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all bank accounts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBankAccounts()
    {
        var query = new GetBankAccountsQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a bank account
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBankAccount(Guid id)
    {
        var command = new DeleteBankAccountCommand { BankAccountId = id };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}

