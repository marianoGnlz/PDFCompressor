using Microsoft.Extensions.Configuration;
using System;

namespace PDFCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            string nombreArchivo = args[0];

            var compressor = new Compressor(args[1]);
            compressor.Search(nombreArchivo);
        }
    }
}
