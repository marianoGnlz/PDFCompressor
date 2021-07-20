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
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System;
using iText.Layout.Borders;
using System.Data.SqlClient;
using System.Data;

namespace PDFCompressor
{
    public class Compressor
    {
        public Compressor(string firma)
        {
            Firma = firma;
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

            _config = builder.Build();
        }

        private readonly IConfiguration _config;

        private readonly int separacionSuperior = 25;

        public string Firma { get; set; }
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

        public void Firmar(PdfDocument pdf)
        {
            PdfCanvas canvas = new PdfCanvas(pdf.GetLastPage());
            var widthPage = pdf.GetLastPage().GetPageSize().GetWidth();
            var heightPage = pdf.GetLastPage().GetPageSize().GetHeight();

            canvas.SaveState()
                .SetStrokeColor(ColorConstants.RED)
                .SetLineWidth(.2f)
                .Stroke()
                .RestoreState();

            Canvas canvasFirma = new Canvas(canvas, pdf.GetLastPage().GetPageSize());

            PdfFont normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            Text textoDeLaFirma = new Text(this.Firma)
                .SetFontSize(24)
                .SetFont(normal)
                ;
            Text textoDeProhibicion = new Text("\nQueda prohíbida su distribución.")
                .SetFontSize(12)
                .SetFont(normal)
                .SetItalic()
                ;

            Paragraph firma = new Paragraph()
                                .Add(textoDeLaFirma)
                                .Add(textoDeProhibicion)
                                .SetBorder(new SolidBorder(ColorConstants.RED,2))
                                .SetWidth(widthPage / 2)
                                ;

            canvasFirma
                .SetFontColor(ColorConstants.RED)
                .ShowTextAligned(firma, widthPage / 2, heightPage - separacionSuperior, TextAlignment.CENTER, VerticalAlignment.MIDDLE)
                .Close();

            canvas.Release();
        }

        public int ComprimirPaginas(IEnumerable<System.IO.FileInfo> fileQuery, string nombreArchivo, int? pageInit = null, int? pageFinal = null)
        {

            string pathNewPdf = @$"{_config.GetSection("files:locations:0:path").Value}/PDF{nombreArchivo}.pdf";

            using (SqlConnection connection = new SqlConnection(@$"{_config.GetSection("ConnectionStrings:DefaultConnection").Value}"))
            {
                connection.Open();
                string sql = "INSERT INTO Archivos(Nombre,Ruta,CreatedAt) VALUES(@param2,@param3,@param4)";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.Add("@param2", SqlDbType.VarChar, 50).Value = nombreArchivo;
                    cmd.Parameters.Add("@param3", SqlDbType.VarChar, 50).Value = pathNewPdf;
                    cmd.Parameters.Add("@param4", SqlDbType.DateTime, 50).Value = DateTime.Now;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }

            WriterProperties wp = new WriterProperties();
            wp.SetPdfVersion(PdfVersion.PDF_1_7);
            wp.SetCompressionLevel(9);
            wp.SetFullCompressionMode(true);
            PdfWriter writer = new PdfWriter(pathNewPdf, wp);
            PdfDocument pdf = new PdfDocument(writer);

            PdfDocumentInfo info = pdf.GetDocumentInfo();
            info.SetAuthor("Norte");
            info.SetKeywords("Diario, Norte");
            info.SetTitle($"PDF{nombreArchivo}");


            if (pageInit != null || pageFinal != null)
            {
                fileQuery = fileQuery.Skip((int)pageInit - 1).Take((int)pageFinal);
            }

            foreach (var file in fileQuery)
            {

                PdfReader reader = new PdfReader(file);
                PdfDocument pdfDocument = new PdfDocument(reader);

                pdfDocument.CopyPagesTo(1, 1, pdf);

                this.Firmar(pdf);

                reader.Close();
                pdfDocument.Close();
            }
            
            pdf.Close();
            writer.Close();
            return 0;
        }
    }
}
