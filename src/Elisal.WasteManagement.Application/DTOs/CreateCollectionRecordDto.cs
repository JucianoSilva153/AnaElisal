using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Application.DTOs;

public class CreateCollectionRecordDto
{
    [Required]
    public DateTime DateTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "A quantidade é obrigatória")]
    [Range(0.01, 10000, ErrorMessage = "A quantidade deve ser superior a 0")]
    public double AmountKg { get; set; }

    [MaxLength(250)]
    public string Notes { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o tipo de resíduo")]
    [Range(1, int.MaxValue, ErrorMessage = "Tipo de resíduo inválido")]
    public int WasteTypeId { get; set; }

    public List<int> WasteTypeIds { get; set; } = new();

    [Required]
    public int CollectionPointId { get; set; }

    [Required]
    public int UserId { get; set; }
}
