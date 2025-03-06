using System.ComponentModel.DataAnnotations;

namespace CashFlow.Domain.Enums;

public enum TransactionType
{
    [Display(Name = "Crédito")]
    Credit = 1,
    [Display(Name = "Débito")]
    Debit = 2
}
