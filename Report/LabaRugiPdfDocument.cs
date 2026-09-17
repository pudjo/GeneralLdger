using Accounting.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace Accounting.Report
{
    
        public class LabaRugiPdfDocument : IDocument
        {
            private readonly List<GeneralLedgerLaporanDTO> _laporanData;
            private readonly DateTime _tanggalAwal;
        private readonly DateTime _tanggalAkhir;
        private string sNamaLaporan;
        public string NamaLaporan
        {
            set { sNamaLaporan = value; }
            get { return sNamaLaporan; }
        }
            public LabaRugiPdfDocument(List<GeneralLedgerLaporanDTO> laporanData, DateTime tanggalAwal, DateTime tanggalAkhir)
            {
                _laporanData = laporanData ?? new List<GeneralLedgerLaporanDTO>();
             _tanggalAwal= tanggalAwal;
            _tanggalAkhir= tanggalAkhir;
        

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
                    col.Item().Text("LABA RUGI").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Per Tanggal: {_tanggalAwal:dd MMMM yyyy} s/d {_tanggalAkhir:dd MMMM yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
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
                    columns.RelativeColumn(3);          // Account Name
                    columns.RelativeColumn(1.5f);       // Before Amount / Amount
                });

                // Header Tabel
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Kode Akun").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Nama Akun").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight().Text("Jumlah").Bold().FontColor(Colors.White);

                });

                // Baris Data
                bool alternate = false;
                foreach (var item in _laporanData)
                {
                    // Deteksi baris khusus (mengandung '9999') untuk formatting tebal/garis/jarak lebih besar
                    bool isTotalRow = !string.IsNullOrEmpty(item.AccountCode) && item.AccountCode.Contains("9999");
                    bool isRoot = item.Root == 0 || isTotalRow;
                    string backgroundColor = isTotalRow ? Colors.Grey.Lighten3 : (alternate ? Colors.Grey.Lighten4 : Colors.White);
                    float indentLeft = isTotalRow ? 0 : (item.Root - 1) * 15;
                    float fontSize = isTotalRow ? 10 : 9;

                    // Helper styling per cell
                    IContainer ApplyCellStyle(IContainer c)
                    {
                        var styled = c.Background(backgroundColor);

                        if (isTotalRow)
                        {
                            // Jarak vertikal lebih besar dan garis atas & bawah tipis
                            return styled.PaddingVertical(12).PaddingHorizontal(0)
                                         .BorderTop(0.75f).BorderBottom(0.75f).BorderColor(Colors.Grey.Lighten2);
                        }
                        else
                        {
                            return styled.Padding(5)
                                         .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    }

                    // Kode Akun
                    table.Cell().Element(ApplyCellStyle).Text(text =>
                    {
                        var span = text.Span(item.AccountCode ?? "");
                        span.FontSize(fontSize);
                        if (isRoot) span.Bold();
                        if (isTotalRow) span.Underline();
                    });

                    // Nama Akun
                    table.Cell().Element(ApplyCellStyle).PaddingLeft(indentLeft).Text(text =>
                    {
                        var span = text.Span(item.AccountName ?? "");
                        span.FontSize(fontSize);
                        if (isRoot || isTotalRow) span.Bold();
                        if (isTotalRow) span.Underline();
                    });

                    // Jumlah / Current Amount
                    table.Cell().Element(ApplyCellStyle).AlignRight().Text(text =>
                    {
                        var span = text.Span(item.CurrentAmount.ToString("N2"));
                        span.FontSize(fontSize);
                        if (isRoot || isTotalRow) span.Bold();
                        if (isTotalRow) span.Underline();
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
