using Hub.Shared.DataContracts.Banking.Constants;
using MudBlazor;

namespace Banking.Web.WebApp.Utils;

public static class IconUtils
{
    public static string GetAccountTypeIcon(string accountType)
    {
        return accountType switch
        {
            AccountTypes.Investment => Icons.Sharp.StackedLineChart,
            AccountTypes.Standard => Icons.Sharp.AccountBalanceWallet,
            AccountTypes.CreditCard => Icons.Sharp.CreditCard,
            AccountTypes.Billing => Icons.Sharp.ReceiptLong,
            AccountTypes.Saving => Icons.Sharp.Savings,
            _ => ""
        };
    }

    public static readonly string AccountIcon = Icons.Sharp.AccountBalance;
    public static readonly string TransactionIcon = Icons.Sharp.Receipt;
    public static readonly string ScheduledTransactionIcon = Icons.Sharp.Update;
    public static readonly string TransactionCategoryIcon = Icons.Sharp.Category;
    public static readonly string BankIcon = Icons.Sharp.Verified;
    
    public static readonly string TransactionDescription = Icons.Sharp.TextSnippet;

}