using System;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Application.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public int CooperativeId { get; set; }
    public int WasteTypeId { get; set; }
    public double AmountKg { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal TotalValue { get; set; }
}
