using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Commands;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IMediator _mediator;

    public PaymentsController(PaymentsRepository paymentsRepository, IMediator mediator)
    {
        _paymentsRepository = paymentsRepository;
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        return new OkObjectResult(payment);
    }

    [HttpPost()]
    public async Task<ActionResult<PostPaymentResponse?>> CreatePaymentAsync([FromBody] CreatePaymentCommand createPaymentCommand, [FromHeader(Name = "idempotency-key")] string idKey, CancellationToken cancellationToken)
    {
        var paymentDetail = await _mediator.Send(createPaymentCommand, cancellationToken);
        return paymentDetail;
    }
}