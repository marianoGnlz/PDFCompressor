using Microsoft.Extensions.Configuration;
using System;

namespace PDFCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            var compressor = new Compressor();
            string nombreArchivo = args[0];
            compressor.Search(nombreArchivo);
        }
    }
}
