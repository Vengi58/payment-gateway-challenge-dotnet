﻿using FluentValidation;

using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Domain.Enums;

using System.Text.RegularExpressions;

namespace PaymentGateway.Application.Behaviors
{
    public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        private const string CardNumberPattern = "^(?:4[0-9]{12}(?:[0-9]{3})?" + //Visa
                "|5[1-5][0-9]{14}" + //Mastercard
                "|3[47][0-9]{13})$" +//Amex
                "|2222[0-9]{11}";//sample card numbers provided
        private const string MonthPattern = "^([1-9]|[1][0-2])$";
        private const string YearPattern = "^20[2-9][0-9]$";
        private const string cvvPattern = "^[0-9]{3,4}$";

        public CreatePaymentCommandValidator()
        {
            RuleFor(command => command.CardNumber)
                .Must(n => Regex.IsMatch(n.ToString(), CardNumberPattern, RegexOptions.IgnoreCase))
                .WithMessage("Invalid card number");

            RuleFor(command => command.ExpiryYear)
                .Must(n => Regex.IsMatch(n.ToString(), YearPattern, RegexOptions.IgnoreCase))
                .WithMessage("Incorrect Card Expiration Year");

            RuleFor(command => command.ExpiryMonth)
                .Must(n => Regex.IsMatch(n.ToString(), MonthPattern, RegexOptions.IgnoreCase))
                .WithMessage("Incorrect Card Expiration Month");

            RuleFor(command => Tuple.Create(command.ExpiryYear, command.ExpiryMonth ))
                .Must(n => Regex.IsMatch(n.Item1.ToString(), YearPattern, RegexOptions.IgnoreCase) &&
                    Regex.IsMatch(n.Item2.ToString(), MonthPattern, RegexOptions.IgnoreCase) &&
                    new DateTime(n.Item1, n.Item2, 1) > DateTime.Now)
                .WithMessage("Card expired");

            RuleFor(command => command.Currency)
                .Must(n => Enum.TryParse(n.ToUpper(), out Currencies currency))
                .WithMessage("Invalid currency");

            RuleFor(command => command.Amount)
                .GreaterThan(-1)
                .WithMessage("Invalid amount");

            RuleFor(command => command.Cvv)
                .Must(n => Regex.IsMatch(n.ToString(), cvvPattern, RegexOptions.IgnoreCase))
                .WithMessage("CVV must be 3-4 numeric characters");
        }
    }
}
