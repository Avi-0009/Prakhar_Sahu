# Refactoring Notes

1. **God Method / SRP Violation**: The controller handles validation, business logic, and database access. **Fix**: Split into OrderController, IOrderService, and IOrderRepository.
2. **Direct DbContext Instantiation**: Tightly coupled to AppDbContext. **Fix**: Inject IOrderRepository via constructor DI.
3. **Sync over Async**: Uses SaveChanges() inside an async method, blocking the thread. **Fix**: Use SaveChangesAsync(CancellationToken).
4. **Empty Catch Blocks**: Swallows exceptions silently, making debugging impossible. **Fix**: Remove try/catch; let global exception middleware handle unexpected errors, or catch specific exceptions to return structured ProblemDetails.
5. **Dynamic/Untyped Payloads**: [FromBody] dynamic payload circumvents type safety and Swagger generation. **Fix**: Create a strongly-typed CreateOrderRequest DTO.
6. **Untyped Return Values**: Returning anonymous objects (
ew { Error = ... }). **Fix**: Return ActionResult<CreateOrderResponse>.
7. **Null Dereference Bug**: payload.Customer.Name will throw if Customer is null. **Fix**: Proper DTO validation before mapping.
8. **Off-by-one Error**: Loop uses i <= payload.Items.Count, throwing IndexOutOfRangeException. **Fix**: Use oreach or LINQ (.Sum()).
9. **Missing Cancellation Tokens**: Client disconnects won't halt database operations. **Fix**: Flow CancellationToken ct from the controller down to EF Core.
10. **Hardcoded Dependencies**: DateTime.Now makes testing time-dependent logic hard. **Fix**: Use TimeProvider or abstract it behind the service layer.
