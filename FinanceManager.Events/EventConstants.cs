namespace FinanceManager.Events;

public static class EventConstants
{
    private static readonly string RejectedPostfix = "rejected";

    public static string AddOperation = "add";
    public static string DeleteOperation = "delete";

    public static string DepositExpenseCommonExchange = "deposit-expense-common";

    public static string DepositExchange = "deposit";
    public static string AddDepositEvent = $"{DepositExchange}-{AddOperation}";
    public static string AddDepositRejectedEvent = $"{AddDepositEvent}-{RejectedPostfix}";
    public static string DeleteDepositEvent = $"{DepositExchange}-{DeleteOperation}";

    public static string ExpenseExchange = "expence";
    public static string AddExpenseEvent = $"{ExpenseExchange}-{AddOperation}";
    public static string AddExpenseRejectedEvent = $"{AddExpenseEvent}-{RejectedPostfix}";
    public static string DeleteExpenseEvent = $"{ExpenseExchange}-{DeleteOperation}";

    public static string TransferExchange = "transfer";
    public static string AddTransferEvent = $"{TransferExchange}-{AddOperation}";
    public static string AddTransferRejectedEvent = $"{AddTransferEvent}-{RejectedPostfix}";

    public static string AccountExchange = "account";
    public static string ChangeBalanceOperation = "change-balance";
    public static string ChangeAccountBalanceEvent = $"{AccountExchange}-{ChangeBalanceOperation}";
    public static string DepositChangeAccountBalanceRejectedEvent = $"{ChangeAccountBalanceEvent}-Deposit-{RejectedPostfix}";
    public static string ExpenseChangeAccountBalanceRejectedEvent = $"{ChangeAccountBalanceEvent}-Expense-{RejectedPostfix}";
    
    public static string TransferBetweenAccountsOperation = "transfer-between";
    public static string TransferBetweenAccountsEvent = $"{AccountExchange}-{TransferBetweenAccountsOperation}";
    public static string TransferBetweenAccountsRejectedEvent = $"{TransferBetweenAccountsEvent}-Expense-{RejectedPostfix}";
    
    public static string TransferChangeAccountBalanceRejectedEvent = $"{ChangeAccountBalanceEvent}-Transfer-{RejectedPostfix}";

    public static string StatisticsExchange = "statistics";
    public static string ChangeStatisticsOperation = "change";
    public static string ChangeStatisticsEvent = $"{StatisticsExchange}-{ChangeStatisticsOperation}";

    public static string NotificationExchange = "notification";
    public static string NotificationSendOperation = "send";
    public static string NotificationSendEvent = $"{NotificationExchange}-{NotificationSendOperation}";

    public static string FileExchange = "file";
}