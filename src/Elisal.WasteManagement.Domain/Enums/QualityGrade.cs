namespace Elisal.WasteManagement.Domain.Enums;

/// <summary>
/// Classificação de qualidade do material triado.
/// </summary>
public enum QualityGrade
{
    A,          // Alta qualidade — pronto para venda/reciclagem directa
    B,          // Qualidade média — requer algum processamento
    C,          // Qualidade baixa — valor reduzido
    Rejected    // Rejeitado — contaminado ou inaproveitável
}
