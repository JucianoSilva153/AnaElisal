namespace Elisal.WasteManagement.Domain.Enums;

/// <summary>
/// Tipo de destino final do resíduo após triagem.
/// </summary>
public enum WasteDestinationType
{
    Recycling,          // Reciclagem (venda a cooperativas)
    Composting,         // Compostagem (resíduos orgânicos)
    Landfill,           // Aterro sanitário
    Incineration,       // Incineração
    InternalReuse,      // Reaproveitamento interno Elisal
    HazardousDisposal   // Resíduos perigosos (tratamento especial)
}
