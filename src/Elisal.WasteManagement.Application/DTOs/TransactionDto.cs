using System;
using System.ComponentModel.DataAnnotations;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Application.DTOs;

public class TransactionDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A data é obrigatória")]
    public DateTime DateTime { get; set; }

    [Required(ErrorMessage = "A empresa parceira é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma empresa parceira válida")]
    public int CooperativeId { get; set; }

    [Required(ErrorMessage = "O tipo de resíduo é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um tipo de resíduo válido")]
    public int WasteTypeId { get; set; }

    [Required(ErrorMessage = "A quantidade é obrigatória")]
    [Range(0.01, 1000000, ErrorMessage = "A quantidade deve ser superior a 0")]
    public double AmountKg { get; set; }

    public TransactionStatus Status { get; set; }

    public decimal PricePerKg { get; set; }

    [Required(ErrorMessage = "O valor total é obrigatório")]
    [Range(0, 1000000000, ErrorMessage = "O valor não pode ser negativo")]
    public decimal TotalValue { get; set; }
}
