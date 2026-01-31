namespace PersonalFinanceManager.Enums;

public enum AccountType
{
    Checking = 1,
    Savings = 2,
    CreditCard = 3,
    Cash = 4,
    Investment = 5,
    Loan = 6,
    Other = 99
}

public enum CategoryType
{
    Income = 1,
    Expense = 2
}

public enum TransactionType
{
    Income = 1,
    Expense = 2,
    Transfer = 3
}

public enum RecurrenceFrequency
{
    Daily = 1,
    Weekly = 2,
    BiWeekly = 3,
    Monthly = 4,
    Quarterly = 5,
    Yearly = 6
}

public enum BudgetPeriod
{
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    Yearly = 4
}

public enum GoalStatus
{
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    OnHold = 4
}