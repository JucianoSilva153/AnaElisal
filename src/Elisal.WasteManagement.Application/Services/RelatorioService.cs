using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;

namespace Elisal.WasteManagement.Application.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IRepository<CollectionRecord> _recordRepo;
    private readonly IRepository<CollectionPoint> _pointRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IRepository<Cooperative> _coopRepo;
    private readonly IRepository<WasteType> _wasteRepo;

    public RelatorioService(
        IRepository<CollectionRecord> recordRepo,
        IRepository<CollectionPoint> pointRepo,
        ITransactionRepository transactionRepo,
        IRepository<Cooperative> coopRepo,
        IRepository<WasteType> wasteRepo)
    {
        _recordRepo = recordRepo;
        _pointRepo = pointRepo;
        _transactionRepo = transactionRepo;
        _coopRepo = coopRepo;
        _wasteRepo = wasteRepo;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GerarRelatorioPdfAsync(string tipoRelatorio, DateTime inicio, DateTime fim,
        FiltrosRelatorio filtros)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(container =>
                {
                    container.Column(col =>
                    {
                        col.Item().PaddingVertical(10).Text($"Relatório de {tipoRelatorio}").FontSize(16).Bold();
                        col.Item().Text($"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}").FontSize(10);
                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        if (tipoRelatorio.ToLower() == "producao")
                        {
                            var records = (_recordRepo as ICollectionRecordRepository).GetByPeriodAsync(inicio, fim)
                                .GetAwaiter().GetResult().ToList();
                            ComposeProducaoSync(col, records);
                        }
                        else if (tipoRelatorio.ToLower() == "transacoes")
                        {
                            var transactions = _transactionRepo.GetByPeriodAsync(inicio, fim, filtros.CooperativaId)
                                .GetAwaiter().GetResult().ToList();
                            ComposeTransacoesSync(col, transactions);
                        }
                        else if (tipoRelatorio.ToLower() == "operadores")
                        {
                            var records = (_recordRepo as ICollectionRecordRepository).GetByPeriodAsync(inicio, fim)
                                .GetAwaiter().GetResult().ToList();
                            if (filtros.UsuarioId.HasValue)
                                records = records.Where(r => r.UserId == filtros.UsuarioId.Value).ToList();
                            ComposeOperadoresSync(col, records);
                        }
                        else if (tipoRelatorio.ToLower() == "desempenho-ponto")
                        {
                            var records = (_recordRepo as ICollectionRecordRepository).GetByPeriodAsync(inicio, fim)
                                .GetAwaiter().GetResult().ToList();
                            ComposeDesempenhoPontoSync(col, records);
                        }
                    });
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Gerado em: ");
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GerarRelatorioExcelAsync(string tipoRelatorio, DateTime inicio, DateTime fim,
        FiltrosRelatorio filtros)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Relatório");

        // Header
        worksheet.Cell(1, 1).Value = "ELISAL - Sistema de Gestão de Resíduos";
        worksheet.Cell(2, 1).Value = $"Relatório: {tipoRelatorio}";
        worksheet.Cell(3, 1).Value = $"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}";

        switch (tipoRelatorio.ToLower())
        {
            case "producao":
                await PreencherProducaoExcel(worksheet, inicio, fim, filtros);
                break;
            case "transacoes":
                await PreencherTransacoesExcel(worksheet, inicio, fim, filtros);
                break;
            case "operadores":
                await PreencherOperadoresExcel(worksheet, inicio, fim, filtros);
                break;
            case "desempenho-ponto":
                await PreencherDesempenhoPontoExcel(worksheet, inicio, fim, filtros);
                break;
            default:
                worksheet.Cell(5, 1).Value = "Tipo de relatório não implementado.";
                break;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ELISAL").FontSize(20).Bold().FontColor(Colors.Green.Darken2);
                col.Item().Text("Sistema de Gestão de Resíduos Sólidos").FontSize(10).Italic();
                col.Item().Text("Luanda, Angola").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }


    private void ComposeProducaoSync(ColumnDescriptor col, List<CollectionRecord> records)
    {
        col.Item().Text($"Total de Recolhas: {records.Count}").FontSize(12).Bold();
        col.Item().Text($"Total Coletado: {records.Sum(r => r.AmountKg):N2} Kg").FontSize(12).Bold();

        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Data").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Ponto").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tipo").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Quantidade (Kg)").Bold();
            });

            foreach (var record in records.Take(100))
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(record.DateTime.ToString("dd/MM/yyyy HH:mm"));
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(record.CollectionPoint?.Name ?? $"Ponto #{record.CollectionPointId}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(record.WasteType?.Name ?? "N/A");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(record.AmountKg.ToString("N2"));
            }
        });
    }

    private void ComposeTransacoesSync(ColumnDescriptor col, List<Transaction> transactions)
    {
        col.Item().Text($"Total de Transações: {transactions.Count}").FontSize(12).Bold();
        col.Item().Text($"Valor Total: {transactions.Sum(t => t.Value):N2} Kz").FontSize(12).Bold();

        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Data").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cooperativa").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qtd (Kg)").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Valor (Kz)").Bold();
            });

            foreach (var trans in transactions.Take(100))
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(trans.Date.ToString("dd/MM/yyyy"));
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(trans.Cooperative?.Name ?? $"Coop #{trans.CooperativeId}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(trans.AmountKg.ToString("N2"));
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text(trans.Value.ToString("N2"));
            }
        });
    }

    private async Task PreencherProducaoExcel(IXLWorksheet ws, DateTime inicio, DateTime fim, FiltrosRelatorio filtros)
    {
        var records = (await (_recordRepo as ICollectionRecordRepository).GetByPeriodAsync(inicio, fim)).ToList();

        ws.Cell(5, 1).Value = "Data";
        ws.Cell(5, 2).Value = "Ponto";
        ws.Cell(5, 3).Value = "Tipo";
        ws.Cell(5, 4).Value = "Quantidade (Kg)";

        int row = 6;
        foreach (var r in records)
        {
            ws.Cell(row, 1).Value = r.DateTime;
            ws.Cell(row, 2).Value = r.CollectionPoint?.Name ?? $"Ponto #{r.CollectionPointId}";
            ws.Cell(row, 3).Value = r.WasteType?.Name ?? "N/A";
            ws.Cell(row, 4).Value = r.AmountKg;
            row++;
        }
    }

    private async Task PreencherTransacoesExcel(IXLWorksheet ws, DateTime inicio, DateTime fim,
        FiltrosRelatorio filtros)
    {
        var transactions = (await _transactionRepo.GetByPeriodAsync(inicio, fim, filtros.CooperativaId)).ToList();

        ws.Cell(5, 1).Value = "Data";
        ws.Cell(5, 2).Value = "Cooperativa";
        ws.Cell(5, 3).Value = "Quantidade (Kg)";
        ws.Cell(5, 4).Value = "Valor (Kz)";

        int row = 6;
        foreach (var t in transactions)
        {
            ws.Cell(row, 1).Value = t.Date;
            ws.Cell(row, 2).Value = t.Cooperative?.Name ?? $"Coop #{t.CooperativeId}";
            ws.Cell(row, 3).Value = t.AmountKg;
            ws.Cell(row, 4).Value = (double)t.Value;
            row++;
        }
    }

    private void ComposeOperadoresSync(ColumnDescriptor col, List<CollectionRecord> records)
    {
        var grouped = records.GroupBy(r => r.User?.Name ?? "N/A")
            .Select(g => new { Nome = g.Key, Qtd = g.Count(), Peso = g.Sum(r => r.AmountKg) })
            .OrderByDescending(x => x.Peso);

        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Operador").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total Recolhas").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total (Kg)").Bold();
            });

            foreach (var g in grouped)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(g.Nome);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(g.Qtd.ToString());
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(g.Peso.ToString("N2"));
            }
        });
    }

    private void ComposeDesempenhoPontoSync(ColumnDescriptor col, List<CollectionRecord> records)
    {
        var grouped = records.GroupBy(r => r.CollectionPoint?.Name ?? "N/A")
            .Select(g => new { Local = g.Key, Qtd = g.Count(), Peso = g.Sum(r => r.AmountKg) })
            .OrderByDescending(x => x.Peso);

        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Ponto de Recolha").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Frequência").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total (Kg)").Bold();
            });

            foreach (var g in grouped)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(g.Local);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(g.Qtd.ToString());
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(g.Peso.ToString("N2"));
            }
        });
    }

    private async Task PreencherOperadoresExcel(IXLWorksheet ws, DateTime inicio, DateTime fim,
        FiltrosRelatorio filtros)
    {
        var records = (await (_recordRepo as ICollectionRecordRepository).GetByPeriodAsync(inicio, fim)).ToList();
        if (filtros.UsuarioId.HasValue) records = records.Where(r => r.UserId == filtros.UsuarioId.Value).ToList();

        var grouped = records.GroupBy(r => r.User?.Name ?? "N/A")
            .Select(g => new { Nome = g.Key, Qtd = g.Count(), Peso = g.Sum(r => r.AmountKg) });

        ws.Cell(5, 1).Value = "Operador";
        ws.Cell(5, 2).Value = "Total Recolhas";
        ws.Cell(5, 3).Value = "Total (Kg)";

        int row = 6;
        foreach (var g in grouped)
        {
            ws.Cell(row, 1).Value = g.Nome;
            ws.Cell(row, 2).Value = g.Qtd;
            ws.Cell(row, 3).Value = g.Peso;
            row++;
        }
    }

    private async Task PreencherDesempenhoPontoExcel(IXLWorksheet ws, DateTime inicio, DateTime fim,
        FiltrosRelatorio filtros)
    {
        var records = (await (_recordRepo as ICollectionRecordRepository).GetByPeriodAsync(inicio, fim)).ToList();

        var grouped = records.GroupBy(r => r.CollectionPoint?.Name ?? "N/A")
            .Select(g => new { Local = g.Key, Qtd = g.Count(), Peso = g.Sum(r => r.AmountKg) });

        ws.Cell(5, 1).Value = "Ponto de Recolha";
        ws.Cell(5, 2).Value = "Frequência";
        ws.Cell(5, 3).Value = "Total (Kg)";

        int row = 6;
        foreach (var g in grouped)
        {
            ws.Cell(row, 1).Value = g.Local;
            ws.Cell(row, 2).Value = g.Qtd;
            ws.Cell(row, 3).Value = g.Peso;
            row++;
        }
    }
}
