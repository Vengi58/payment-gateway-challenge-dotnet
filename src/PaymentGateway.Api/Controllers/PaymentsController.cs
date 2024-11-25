using System.Runtime.InteropServices;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        //var payment = _paymentsRepository.GetPaymentById(id);

        return new OkObjectResult("");
    }

    [HttpPost()]
    public async Task<ActionResult<PostPaymentResponse?>> CreatePaymentAsync([FromBody] PostPaymentRequest postPaymentRequest, [Optional][FromHeader(Name = "idempotency-key")] Guid? idempotencyKey, CancellationToken cancellationToken)
    {
        var createPaymentCommand = postPaymentRequest.MapToCreatePaymentCommand(idempotencyKey);

        var paymentDetail = await _mediator.Send(createPaymentCommand, cancellationToken);

        return postPaymentRequest.MapToPostPaymentResponse(paymentDetail);
    }
}