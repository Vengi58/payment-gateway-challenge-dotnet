using System;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentResponse
    {
        public Guid PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
