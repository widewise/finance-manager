namespace FinanceManager.Events;

public static class EventConstants
{
    private static readonly string RejectedPostfix = "rejected";

    private const string AddOperation = "add";
    private const string DeleteOperation = "delete";

    public const string DepositExpenseCommonExchange = "deposit-expense-common";

    public const string DepositExchange = "deposit";
    public const string AddDepositEvent = $"{DepositExchange}-{AddOperation}";
    public static readonly string AddDepositRejectedEvent = $"{AddDepositEvent}-{RejectedPostfix}";
    public static readonly string DeleteDepositEvent = $"{DepositExchange}-{DeleteOperation}";

    public const string ExpenseExchange = "expence";
    public const string AddExpenseEvent = $"{ExpenseExchange}-{AddOperation}";
    public static readonly string AddExpenseRejectedEvent = $"{AddExpenseEvent}-{RejectedPostfix}";
    public static readonly string DeleteExpenseEvent = $"{ExpenseExchange}-{DeleteOperation}";

    public const string TransferExchange = "transfer";
    public const string AddTransferEvent = $"{TransferExchange}-{AddOperation}";
    public static readonly string AddTransferRejectedEvent = $"{AddTransferEvent}-{RejectedPostfix}";

    public const string AccountExchange = "account";
    private const string ChangeBalanceOperation = "change-balance";
    public const string ChangeAccountBalanceEvent = $"{AccountExchange}-{ChangeBalanceOperation}";
    public static readonly string DepositChangeAccountBalanceRejectedEvent = $"{ChangeAccountBalanceEvent}-Deposit-{RejectedPostfix}";
    public static readonly string ExpenseChangeAccountBalanceRejectedEvent = $"{ChangeAccountBalanceEvent}-Expense-{RejectedPostfix}";
    
    private const string TransferBetweenAccountsOperation = "transfer-between";
    public const string TransferBetweenAccountsEvent = $"{AccountExchange}-{TransferBetweenAccountsOperation}";
    public static readonly string TransferBetweenAccountsRejectedEvent = $"{TransferBetweenAccountsEvent}-Expense-{RejectedPostfix}";
    
    public static readonly string TransferChangeAccountBalanceRejectedEvent = $"{ChangeAccountBalanceEvent}-Transfer-{RejectedPostfix}";

    public const string StatisticsExchange = "statistics";
    private const string ChangeStatisticsOperation = "change";
    public const string ChangeStatisticsEvent = $"{StatisticsExchange}-{ChangeStatisticsOperation}";

    public const string NotificationExchange = "notification";
    private const string NotificationSendOperation = "send";
    public const string NotificationSendEvent = $"{NotificationExchange}-{NotificationSendOperation}";

    public const string FileExchange = "file";
}