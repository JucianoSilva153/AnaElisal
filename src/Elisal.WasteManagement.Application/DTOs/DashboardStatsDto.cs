using System.Collections.Generic;

namespace Elisal.WasteManagement.Application.DTOs;

public class DashboardStatsDto
{
    public double TotalResiduosMensal { get; set; }
    public double VariacaoTotalResiduos { get; set; }
    public double TaxaReaproveitamento { get; set; }
    public double VariacaoTaxaReaproveitamento { get; set; }
    public int PontosAtivos { get; set; }
    public int VariacaoPontosAtivos { get; set; }
    public int AlertasOperacionais { get; set; }

    // Novos campos para perfis específicos
    public int RotasAtivasHoje { get; set; }
    public int MinhasRotasConcluidas { get; set; }
    public double MeuAproveitamentoKg { get; set; }
    public int PontosCriticosOcupacao { get; set; } // > 90%
    public List<PieChartDto> ImpactoAmbientalPessoal { get; set; } = new();

    public List<ChartSeriesDto> VolumeMensal { get; set; } = new();
    public List<PieChartDto> DistribuicaoPorTipo { get; set; } = new();
    public List<RecentCollectionDto> ColetasRecentes { get; set; } = new();
    public ActiveRoutePreviewDto? ProximaRota { get; set; }
}

public class ActiveRoutePreviewDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int TotalPontos { get; set; }
    public int PontosConcluidos { get; set; }
}

public class ChartSeriesDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class PieChartDto
{
    public string Category { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class RecentCollectionDto
{
    public string DataHora { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Peso { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
