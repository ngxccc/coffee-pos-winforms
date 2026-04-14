namespace CoffeePOS.Forms.Core;

// WHY: 'out' keyword enables Covariance, ensuring the type T is exclusively returned, maximizing compiler type-inference safety.
public interface IValidatableComponent<out TPayload>
{
    bool ValidateInput();
    TPayload GetPayload();
}
