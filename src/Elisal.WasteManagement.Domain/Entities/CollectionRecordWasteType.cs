using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elisal.WasteManagement.Domain.Entities;

public class CollectionRecordWasteType
{
    public int CollectionRecordId { get; set; }
    
    [ForeignKey(nameof(CollectionRecordId))]
    public CollectionRecord CollectionRecord { get; set; } = null!;

    public int WasteTypeId { get; set; }
    
    [ForeignKey(nameof(WasteTypeId))]
    public WasteType WasteType { get; set; } = null!;
}
