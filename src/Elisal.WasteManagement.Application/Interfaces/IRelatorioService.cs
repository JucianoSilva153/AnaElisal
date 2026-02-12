using System;
using System.IO;
using System.Threading.Tasks;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IRelatorioService
{
    Task<byte[]> GerarRelatorioPdfAsync(string tipoRelatorio, DateTime inicio, DateTime fim, FiltrosRelatorio filtros);
    Task<byte[]> GerarRelatorioExcelAsync(string tipoRelatorio, DateTime inicio, DateTime fim, FiltrosRelatorio filtros);
}

public class FiltrosRelatorio
{
    public int? MunicipioId { get; set; }
    public int? TipoResiduoId { get; set; }
    public int? CooperativaId { get; set; }
}
