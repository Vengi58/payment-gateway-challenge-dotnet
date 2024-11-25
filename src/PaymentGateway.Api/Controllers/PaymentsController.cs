using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Mappings;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync([Required] Guid id, CancellationToken cancellationToken)
    {
        GetPaymentResponse getPaymentResponse = (await _mediator.Send(new Application.Payments.Queries.GetPayment.GetPaymentQuery(id), cancellationToken)).MaptToGetPaymentResponse();

        return ResponseByBankPaymentStatus(getPaymentResponse.Status, getPaymentResponse);
    }

    [HttpPost()]
    public async Task<ActionResult<PostPaymentResponse?>> PostPaymentAsync([FromBody] PostPaymentRequest postPaymentRequest, [Optional][FromHeader(Name = "idempotency-key")] Guid? idempotencyKey, CancellationToken cancellationToken)
    {
        PostPaymentResponse createPaymentResponse = (await _mediator.Send(postPaymentRequest.MapToCreatePaymentCommand(idempotencyKey), cancellationToken)).MapToPostPaymentResponse();

        return ResponseByBankPaymentStatus(createPaymentResponse.Status, createPaymentResponse);
    }


    private ActionResult<T?> ResponseByBankPaymentStatus<T>(BankPaymentStatus bankPaymentStatus, T response)
    {
        return bankPaymentStatus switch
        {
            BankPaymentStatus.Authorized => (ActionResult<T?>)StatusCode(200, response),
            BankPaymentStatus.Declined => (ActionResult<T?>)StatusCode(422, response),
            BankPaymentStatus.Rejected => (ActionResult<T?>)StatusCode(401, response),
            _ => (ActionResult<T?>)StatusCode(401, response),
        };
    }
}