using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Mappings;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Payments.Queries.GetPayment;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(
        [Required][FromHeader(Name = "merchant-id")] Guid merchantId,
        [Required] Guid paymentId,
        CancellationToken cancellationToken)
    {
        GetPaymentResponse getPaymentResponse = (await _mediator.Send(new GetPaymentQuery(paymentId, new Merchant(merchantId)), cancellationToken)).MaptToGetPaymentResponse();

        return ResponseByBankPaymentStatus(getPaymentResponse.Status, getPaymentResponse);
    }

    [HttpPost()]
    public async Task<ActionResult<PostPaymentResponse?>> PostPaymentAsync(
        [Required][FromHeader(Name = "merchant-id")] Guid merchantId,
        [Optional][FromHeader(Name = "idempotency-key")] Guid? idempotencyKey,
        [FromBody] PostPaymentRequest postPaymentRequest,
        CancellationToken cancellationToken)
    {
        PostPaymentResponse createPaymentResponse = (await _mediator.Send((postPaymentRequest, idempotencyKey, new Merchant(merchantId)).MapToCreatePaymentCommand(), cancellationToken)).MapToPostPaymentResponse();

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