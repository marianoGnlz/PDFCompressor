using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Colors;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace PDFCompressor
{
    public class Compressor
    {
        public Compressor()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

            _config = builder.Build();
        }

        private readonly IConfiguration _config;

        public int Search(string nombreArchivo)
        {
            var originFolder = _config.GetSection("files:locations:1:path").Value;
            System.IO.DirectoryInfo dir = new DirectoryInfo(originFolder);

            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> fileQuery =
                from file in fileList
                where file.Name.Contains(nombreArchivo)
                orderby file.Name
                select file;
            if (fileQuery.Any())
            {
                return ComprimirPaginas(fileQuery, nombreArchivo);
            } else
            {
                System.Console.WriteLine("Archivo no encontrado.");
                return 1;
            }
        }
        public int ComprimirPaginas(IEnumerable<System.IO.FileInfo> fileQuery, string nombreArchivo, int? pageInit = null, int? pageFinal = null)
        {
            FileStream newPdf = new FileStream($"{_config.GetSection("files:locations:0:path").Value}/PDF{nombreArchivo}.pdf", FileMode.Create); //@"D:\\GitHub\\Laburo\\Comprimidos\\*.pdf"
            WriterProperties wp = new WriterProperties();
            wp.SetPdfVersion(PdfVersion.PDF_2_0);
            wp.SetCompressionLevel(9);
            wp.SetFullCompressionMode(true);
            PdfWriter writer = new PdfWriter(newPdf, wp);
            PdfDocument pdf = new PdfDocument(writer);

            if (pageInit != null || pageFinal != null)
            {
                fileQuery = fileQuery.Skip((int)pageInit - 1).Take((int)pageFinal);
            }

            foreach (var file in fileQuery)
            {

                PdfReader reader = new PdfReader(file);
                PdfDocument pdfDocument = new PdfDocument(reader);

                pdfDocument.CopyPagesTo(1, 1, pdf);

                PdfCanvas canvas = new PdfCanvas(pdf.GetLastPage());
                var widthPage = pdf.GetLastPage().GetPageSize().GetWidth();

                canvas.SaveState()
                    .SetStrokeColor(ColorConstants.RED)
                    .SetLineWidth(.2f)
                    .Stroke()
                    .RestoreState();

                Canvas canvasFirma = new Canvas(canvas, pdf.GetLastPage().GetPageSize());

                Text firma1 = new Text("TEST1")
                    .SetFontSize(32)
                    ;

                Paragraph firma = new Paragraph().Add(firma1);

                canvasFirma
                    .SetFontColor(ColorConstants.RED)
                    .ShowTextAligned(firma, widthPage / 2, 0, TextAlignment.CENTER)
                    .Close();

                canvas.Release();

                reader.Close();
                pdfDocument.Close();
            }

            pdf.Close();
            writer.Close();
            newPdf.Close();
            return 0;
        }
    }
}
