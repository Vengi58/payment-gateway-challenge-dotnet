# Payment Gateway API

## Context - Online card payment processing

E-Commerce is experiencing exponential growth and merchants who sell their goods or services online need a way to easily collect money from their customers.

We would like to build a payment gateway, an API based application that will allow a merchant to offer a way for their shoppers to pay for their product.

Processing a card payment online involves multiple steps and entities:

![1732719233852](image/README/1732719233852.png)

**Shopper:** Individual who is buying the product online.

**Merchant:** The seller of the product. For example, Apple or Amazon.

**Payment Gateway:** Responsible for validating requests, storing card information and forwarding payment requests and accepting payment responses to and from the acquiring bank.

**Acquiring Bank:** Allows us to do the actual retrieval of money from the shopper’s card and pay out to the merchant. It also performs some validation of the card information and then sends the payment details to the appropriate 3rd party organization for processing.

## Containers - Building a payment gateway

### Payment Gateway

We will be building the **payment gateway** only and simulating the acquiring bank component in order to allow us to fully test the payment flow.

#### Requirements

The product requirements for this initial phase are the following:

##### Processing a payment

The payment gateway will need to provide merchants with a way to process a card payment. To do this, the merchant should be able to submit a request to the payment gateway and receive one of the following types of response:

* * Authorized - the payment was authorized by the call to the acquiring bank
  * Declined - the payment was declined by the call to the acquiring bank
  * Rejected - No payment could be created as invalid information was supplied to the payment gateway and therefore it has rejected the request without calling the acquiring bank
* A merchant should be able to retrieve the details of a previously made payment

### Bank simulator

A bank simulator is provided. The simulator provides responses based on a set of known test cards, each of which return a specific response so that successful authorizations and declines can be tested.

## Components - Payment Gateway API architecture

The software is designed following the Clean Architecture principles, where the dependencies on other layers are from outside in

#### Structure

Project structure dependencies based on this:

**Api** -> Applicataion, Persistance, Services

**Persistance** -> Application

**Services** -> Applicaation

**Application** -> Domain

**Domain**

#### Tech Stack

[C# ](https://learn.microsoft.com/en-us/dotnet/csharp/)

[Microsoft .NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

[Microsoft .Net Core](https://dotnet.microsoft.com/en-us/download)

[ASP.Net Core](https://dotnet.microsoft.com/en-us/apps/aspnet)

[Entity Framework Core 9](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/?tabs=dotnet-core-cli)

[MediatR](https://github.com/jbogard/MediatR)

[Docker](https://www.docker.com/)

[xUnit](https://xunit.net)

#### Api

[PaymentGateway.Api](https://github.com/Vengi58/payment-gateway-challenge-dotnet/tree/main/src/PaymentGateway.Api)

Provides REST API endpoints for Creating a new payment or retrieveing payment details by payment id.

**GET /api/payments/{paymentId}**

**Header Parameters:**

| Name        | Type | Description                                                                                                                          |
| ----------- | ---- | ------------------------------------------------------------------------------------------------------------------------------------ |
| merchant-id | Guid | Required. The implementation assusmes a separate user management and authentication and only takes care about request authorization. |
| paymentId   | Guid | Required. The merchant must provide this as a payment identification.                                                                |

**Returns:**

Json object with the following members:

| Name                     | Type   | Description                                                        |
| ------------------------ | ------ | ------------------------------------------------------------------ |
| id                       | Guid   | The payment id                                                     |
| status                   | string | The status representing the outcome of the payment request         |
| cardNumberLastFourDigits | string | The last four digits of the credit/debit card used for the payment |
| expiryMonth              | number | The expiry month of the credit/debit card used for the payment    |
| expiryYear               | number | The expiry year of the credit/debit card used for the payment      |
| currency                 | string | The currency that the payment was made of                          |
| amount                   | number | The amount of the payment                                          |

**Response statuses:**

Http 200 - Payment request found

Http 404 - Merchant not found or Payment request not found for the merchant

Http 422 - Request validation error, the request cannot be processed

Http 502 - Request processing internal error

**POST /api/payments**

**Header Parameters:**

| Name            | Type | Description                                                                                                                                |
| --------------- | ---- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| merchant-id     | Guid | Required. The implementation assusmes a separate user management and authentication and only takes care about request authorization.       |
| idempotency-key | Guid | Optional. The merchant could provide this as an idempotency key, otherwise a paymentId would be generated for each Create Payment request. |

**Request body:**

Json object with the following members:

| Name        | Type   | Validation                                                                                                     | Description                                    |
| ----------- | ------ | -------------------------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| cardNumber  | string | Required. Most be a valid Visa / Mastercard card number. Can contain 16 digits only                            | Represents a 16 digit credit/debit card number |
| expiryMonth | number | Required. Must be a 4 digit number. Cannot be before the current year.                                         | Credit/debit card expiry month                 |
| expiryYear  | number | Required. Must be a 1 or 2 digit number. expiryYear and expiryMonth must be later than current year and month | Credit/debit card expiry year                  |
| currency    | string | Required. 3 digit currency code. Currently only GBP, USD or EUR is allowed.                                    | Currency of the payment                        |
| amount      | number | Required. Most be non negative number                                                                          | The amount of payment                          |
| cvv         | number | Required. 3 or 4 digit number                                                                                  | The credit/debit card CVV                      |

**Returns:**

Json object with the following members:

| Name                     | Type   | Description                                                        |
| ------------------------ | ------ | ------------------------------------------------------------------ |
| id                       | Guid   | The payment id                                                     |
| status                   | string | The status representing the outcome of the payment request         |
| cardNumberLastFourDigits | string | The last four digits of the credit/debit card used for the payment |
| expiryMonth              | number | The expiry month of the credit/debit card used for the payment    |
| expiryYear               | number | The expiry year of the credit/debit card used for the payment      |
| currency                 | string | The currency that the payment was made of                          |
| amount                   | number | The amount of the payment                                          |

**Response statuses:**

Http 200 - Payment request found

Http 404 - Merchant not found

Http 422 - Request validation error, the request cannot be processed

Http 502 - Request processing internal error

#### Persistance

[PaymentGateway.Persistance](https://github.com/Vengi58/payment-gateway-challenge-dotnet/tree/main/src/PaymentGateway.Persistance)

Provides database implementation for storing card and payment details. Merchant and Payment entities stored in separate tables. Sensitive data like card number last four digits and CVV are stored in byte[] format, assuming that they are encrypted byte data.

Two implementations provided for the repository interface.

**PaymentsEfRepository:** Uses entity framework and the DbContext can be configured in the API startup code. Current implementation uses InMemory Database but could be easily used with other providers as well, e.g. Postgres.

**PaymentsRepository:** Acts a test doubles and uses Dictionary and lists to store merchant and payment data.

#### Services

[PaymentGateway.Services](https://github.com/Vengi58/payment-gateway-challenge-dotnet/tree/main/src/PaymentGateway.Services)

**BankSimulator**: Provides implementation for connecting to the Bank Simulator. It is a simple implementation to wrap the interaction with the Bank Simulator REST API.

**RsaCryptoService**: Provides implementation for data encryption services, which is needed to securely store sensitive data. Uses a simple RSA encryption. Ideally an external encryption provider service should be used, or even better, the sensitive data should be stored in external trusted key stores.

#### Application

[PaymentGateway.Application](https://github.com/Vengi58/payment-gateway-challenge-dotnet/tree/main/src/PaymentGateway.Application)

The Application used MediatR's PipelineBehavior to chain request validation and logging and RequestHandler for request handling delegation.

To try to avoid double spending the application maintains an internal payment request status, initially persisting the payment in the database as Processing and updates the states based on the response of the Bank Simulator to Completed or Failed. If another payment request with the same payment id is submitted by the merchant while the request is in Processing status it won't reporcesss it again.

**Behaviors:**

The request logging and validation behaviors separated from the core logic, implemented in the LoggingBehavior and CreatePaymentCommandValidator classes.

**Command and Query:**

The Application implementation is separated to Commands and Queries, based on the CQRS design pattern. Currently only a CreatePayment command a GetPayment query is defined, as it satisfies the requirements. The request processing stages are logged within the command and query handler classes.

**Exceptions:**

The Application defines custom exceptions to improve response management.

MerchantNotFoundException thrown when the merchant id provided in the request cannot be found in the database

PaymentNotFoundException thrown when the payment for the provided payment id is not found for the merchant in the database.

**Mappings:**

Provides object mapping between different model classes.

#### Domain

[PaymentGateway.Domain](https://github.com/Vengi58/payment-gateway-challenge-dotnet/tree/main/src/PaymentGateway.Domain)

Provides the model classes and enums used by the Application.

## Code - Implementation Details and Testing

#### Implementation Details

#### Testing

The solution contains xUnit tests in the /test folder to test the Payments API and the Application itself as well. The tests cover various scenarios for valid and invalid request data.

## How To

The application requires the following environment variables to be set:

ASPNETCORE_ENVIRONMENT=Development

ASPNETCORE_HTTP_PORTS=5005

BANK_SIMULATOR_ADDRESS=http://localhost:8080 <--- the address where the Bank Simulator app is running.

INIT_TEST_DATA=true <--- the application could provide initial test data if required.

#### Run from your preferred IDE

Clone the repository https://github.com/Vengi58/payment-gateway-challenge-dotnet.git

Set the environments variable inside or outside your project.

Run the PaymentGateway.Api. Once it starts it will provide a swagger documentation page to easily test the application. Swagger page is generated only if the ASPNETCORE_ENVIRONMENT is set to Development

#### Run as a docker container

You can run the application within Docker. For this you first need to have Docker services running on your computer. You could use Docker Desktop or Rancher to set up your environment.

The solution contains a dockerfile to build your docker image from the application and a docker-compose.yaml file to run the containers.

Build your docker image:

Navigate to the root folder where the .sln file is and run the following command in a console:

```console
docker build -f .\src\PaymentGateway.Api\Dockerfile -t paymentgatewayapi:v1 .
```

Ensure that the image is built by running the following command and find your image paymentgatewayapi

```console
docker images
```

Run the following command to create your containers:

```console
docker-compose -f .\docker-compose.yml up
```

The above mentioned environment variables are configued in the docker-compose.yml file

#### Test the Payment Gateway API

If you use InMemoryDatabase for the data persistance then you should use the test data generator by setting the INIT_TEST_DATA=true environment variable. This will provide two merchants that could be used for testing:

* 47f729c1-c863-4403-ae2e-6e836bf44fee
* cc6afe80-e0c2-478d-a50a-18d68c918352

If you use other database provider then you can create further merchants in the Merchants table.

A test payment is also created intially for merchant 47f729c1-c863-4403-ae2e-6e836bf44fee with paymentId b38eacca-a23e-4988-bbe6-ffba1ed3958b

##### Get Payment by paymentID and merchantId:

request

```console
curl -X 'GET' \
  'https://localhost:7092/api/Payments/b38eacca-a23e-4988-bbe6-ffba1ed3958b' \
  -H 'accept: text/plain' \
  -H 'merchant-id: 47f729c1-c863-4403-ae2e-6e836bf44fee'
```

returns HTTP 200

```json
{
  "id": "b38eacca-a23e-4988-bbe6-ffba1ed3958b",
  "status": "Authorized",
  "cardNumberLastFourDigits": "1677",
  "expiryMonth": 11,
  "expiryYear": 2025,
  "currency": "GBP",
  "amount": 100
}
```

request

```console
curl -X 'GET' \
  'https://localhost:7092/api/Payments/b38eacca-a23e-4988-bbe6-ffba1ed3958b' \
  -H 'accept: text/plain' \
  -H 'merchant-id: 47f729c1-0000-0000-ae2e-6e836bf44fee'
```

returns HTTP 404

```json
{
  "type": "RequestFailure",
  "title": "Request error",
  "status": 400,
  "detail": "Merchant 47f729c1-0000-0000-ae2e-6e836bf44fee not found."
}
```

request

```console
curl -X 'GET' \
  'https://localhost:7092/api/Payments/b38eacca-0000-0000-bbe6-ffba1ed3958b' \
  -H 'accept: text/plain' \
  -H 'merchant-id: 47f729c1-c863-4403-ae2e-6e836bf44fee'
```

returns HTTP 404

```json
{
  "type": "RequestFailure",
  "title": "Request error",
  "status": 400,
  "detail": "Payment b38eacca-0000-0000-bbe6-ffba1ed3958b for merchant 47f729c1-c863-4403-ae2e-6e836bf44fee not found."
}
```

##### Create Payment for merchant:

request

```console
curl -X 'POST' \
  'https://localhost:7092/api/Payments' \
  -H 'accept: text/plain' \
  -H 'merchant-id: 47f729c1-c863-4403-ae2e-6e836bf44fee' \
  -H 'Content-Type: application/json' \
  -d '{
  "CardNumber": "2222405343248877",
  "ExpiryMonth": 4,
  "ExpiryYear": 2025,
  "Currency": "GBP",
  "Amount": 100,
  "Cvv" : "123"
}'
```

response HTTP 200

```json
{
  "id": "7140fcb3-4a96-458b-b297-997700149eaf",
  "status": "Authorized",
  "cardNumberLastFourDigits": "8877",
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "GBP",
  "amount": 100
}
```

request

```console
curl -X 'POST' \
  'https://localhost:7092/api/Payments' \
  -H 'accept: text/plain' \
  -H 'merchant-id: 47f729c1-c863-4403-ae2e-6e836bf44fee' \
  -H 'Content-Type: application/json' \
  -d '{
  "CardNumber": "2222405343248112",
  "ExpiryMonth": 1,
  "ExpiryYear": 2026,
  "Currency": "USD",
  "Amount": 60000,
  "Cvv" : "456"
}'
```

returns HTTP 422

```json
{
  "id": "a710456c-a5f6-4a21-9c5e-59d3bbf0c01d",
  "status": "Declined",
  "cardNumberLastFourDigits": "8112",
  "expiryMonth": 1,
  "expiryYear": 2026,
  "currency": "USD",
  "amount": 60000
}
```

request (idempotency-key provided by merchant)

```console
curl -X 'POST' \
  'https://localhost:7092/api/Payments' \
  -H 'accept: text/plain' \
  -H 'merchant-id: 47f729c1-c863-4403-ae2e-6e836bf44fee' \
  -H 'idempotency-key: 40e06bf8-f658-4a2c-8953-4771fc48ad4d' \
  -H 'Content-Type: application/json' \
  -d '{
  "CardNumber": "2222405343248877",
  "ExpiryMonth": 4,
  "ExpiryYear": 2025,
  "Currency": "GBP",
  "Amount": 100,
  "Cvv" : "123"
}'
```

returns HTTP 200

```json
{
  "id": "40e06bf8-f658-4a2c-8953-4771fc48ad4d",
  "status": "Authorized",
  "cardNumberLastFourDigits": "8877",
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "GBP",
  "amount": 100
}
```

## Notes

The documentation follows the [C4 documentation approach](https://icepanel.medium.com/visualizing-software-architecture-with-the-c4-model-9255025c70b2)
