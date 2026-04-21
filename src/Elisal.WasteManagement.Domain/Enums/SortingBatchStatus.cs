namespace Elisal.WasteManagement.Domain.Enums;

/// <summary>
/// Estado do lote de triagem.
/// </summary>
public enum SortingBatchStatus
{
    Pending,      // Pendente — aguarda início
    InProgress,   // Em progresso — triagem a decorrer
    Completed,    // Concluída
    Cancelled     // Cancelada
}
