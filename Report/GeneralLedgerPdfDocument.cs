using Accounting.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Report
{
    
        public class GeneralLedgerPdfDocument : IDocument
        {
            private readonly List<GeneralLedgerLaporanDTO> _laporanData;
            private readonly DateTime _tanggalLaporan;
        private string sNamaLaporan;
        public string NamaLaporan
        {
            set { sNamaLaporan = value; }
            get { return sNamaLaporan; }
        }
            public GeneralLedgerPdfDocument(List<GeneralLedgerLaporanDTO> laporanData, DateTime tanggalLaporan)
            {
                _laporanData = laporanData ?? new List<GeneralLedgerLaporanDTO>();
                _tanggalLaporan = tanggalLaporan;

                // Atur lisensi QuestPDF (untuk Community / Free License)
                QuestPDF.Settings.License = LicenseType.Community;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    // HAPUS baris berikut: page.PageNumbering();
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Lato));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }
        private void ComposeHeader(IContainer container)
        {
                container.Column(col =>
                {
                    col.Item().Text("APLIKASI AKUNTANSI").Bold().FontSize(12).FontColor(Colors.Grey.Medium);
                    col.Item().Text("NERACA").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Per Tanggal: {_tanggalLaporan:dd MMMM yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
        }
        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(5).Table(table =>
            {
                // Definisi Kolom: Kode Akun, Nama Akun, Before Amount, Current Amount
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(90); // Account Code
                    columns.RelativeColumn(3);        // Account Name
                    columns.RelativeColumn(1.5f);       // Before Amount
                    columns.RelativeColumn(1.5f);       // Current Amount
                });

                // Header Tabel
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Kode Akun").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Nama Akun").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight().Text("Saldo").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight().Text("Saldo Awal").Bold().FontColor(Colors.White);
                });

                // Baris Data
                bool alternate = false;

                foreach (var item in _laporanData)
                {
                    bool isTotalRow = !string.IsNullOrEmpty(item.AccountCode) && item.AccountCode.Contains("999");
                    bool isRoot = item.Root == 0 || isTotalRow;
                    string backgroundColor = isTotalRow ? Colors.Grey.Lighten3 : (alternate ? Colors.Grey.Lighten4 : Colors.White);
                    float indentLeft = isTotalRow ? 0 : (item.Root - 1) * 15;
                    float fontSize = isTotalRow ? 10 : 9;

                    // Fungsi helper untuk mengatur styling cell
                    IContainer ApplyCellStyle(IContainer cellContainer)
                    {
                        var styled = cellContainer.Background(backgroundColor);

                        if (isTotalRow)
                        {
                            // Padding atas & bawah lebih besar agar teks tidak terlalu mepet dengan garis
                            return styled.PaddingVertical(15).PaddingHorizontal(0)
                                         .BorderTop(0.5f).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2); // Garis lebih tipis (0.5f)
                        }
                        else
                        {
                            return styled.Padding(5)
                                         .BorderBottom(1).BorderColor(Colors.White);
                        }
                    }

                    // 1. Kolom Kode Akun
                    table.Cell().Element(ApplyCellStyle).Text(text => {
                        if (!isTotalRow)
                        {
                            var span = text.Span(item.AccountCode ?? "");
                            span.FontSize(fontSize);
                            if (isRoot) span.Bold();
                        }
                    });

                    // 2. Kolom Nama Akun
                    table.Cell().Element(ApplyCellStyle).PaddingLeft(indentLeft).Text(text => {
                        var span = text.Span(item.AccountName ?? "");
                        span.FontSize(fontSize);
                        if (isRoot || isTotalRow) span.Bold();
                    });

                    // 4. Kolom Current Amount
                    table.Cell().Element(ApplyCellStyle).AlignRight().Text(text => {
                        var span = text.Span(item.CurrentAmount.ToString("N2"));
                        span.FontSize(fontSize);
                        if (isRoot || isTotalRow) span.Bold();
                    });
                    // 3. Kolom Before Amount
                    table.Cell().Element(ApplyCellStyle).AlignRight().Text(text => {
                        var span = text.Span(item.BeforeAmount.ToString("N2"));
                        span.FontSize(fontSize);
                        if (isRoot || isTotalRow) span.Bold();
                    });

                    if (!isTotalRow)
                    {
                        alternate = !alternate;
                    }
                }
            });
        }
        private void ComposeFooter(IContainer container)
            {
                container.AlignCenter().Text(text =>
                {
                    text.Span("Halaman ");
                    text.CurrentPageNumber();
                    text.Span(" dari ");
                    text.TotalPages();
                });
            }
        }
}
