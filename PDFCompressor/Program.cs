using Microsoft.Extensions.Configuration;
using System;

namespace PDFCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            string nombreArchivo = args[0];

            var compressor = new Compressor("Este archivo es una copia gratuita");
            compressor.Search(nombreArchivo);
        }
    }
}
