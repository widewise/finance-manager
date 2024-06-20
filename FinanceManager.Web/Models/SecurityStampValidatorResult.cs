namespace FinanceManager.Web.Models;

public record SecurityStampValidatorResult(
    bool Result,
    string Message = null);