# Why Rich Domain Models Beat Anemic Ones

An anemic domain model treats entities as mere data structures (bags of public getters and setters), pushing all business rules into external controllers or application services. While initially simple, this approach causes validation logic to be duplicated across the application and makes the system's state extremely fragile.

A Rich Domain Model completely encapsulates both data and behavior. By forcing object creation through a static factory (`Quote.Create()`) and using private setters, the entity acts as its own gatekeeper. It guarantees that an invalid `Quote` object can mathematically never exist in system memory. 

**Bug Scenario Prevented:**
Imagine another developer is assigned to build a bulk-import feature. In an anemic model, they would likely instantiate `new Quote()` directly and bypass the controller where the length validations live. They might also accidentally overwrite the `Text` property because the setter is publicly exposed. This ships a bug where the database fills with corrupted data, silently violating the "Text is immutable" business rule. 

With our rich model, the compiler actively prevents this. `Text` has no public setter, and `Quote.Create` refuses to instantiate the object if the text or author lengths violate the invariants, making illegal states entirely unrepresentable.
