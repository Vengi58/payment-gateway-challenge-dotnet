using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Commands;
using PaymentGateway.Api.Models.Requests;
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
        var payment = _paymentsRepository.GetPaymentById(id);

        return new OkObjectResult(payment);
    }

    [HttpPost()]
    public async Task<ActionResult<PostPaymentResponse?>> CreatePaymentAsync([FromBody] PostPaymentRequest postPaymentRequest, [Optional][FromHeader(Name = "idempotency-key")] Guid? idempotencyKey, CancellationToken cancellationToken)
    {
        CreatePaymentCommand createPaymentCommand = new(
            idempotencyKey ?? Guid.NewGuid(),
            postPaymentRequest.Currency,
            postPaymentRequest.Amount,
            postPaymentRequest.CardNumber,
            postPaymentRequest.ExpiryMonth,
            postPaymentRequest.ExpiryYear,
            postPaymentRequest.Cvv);

        var paymentDetail = await _mediator.Send(createPaymentCommand, cancellationToken);
        return paymentDetail;
    }
}