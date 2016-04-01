namespace AzureSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BookModel
    {
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public List<string> Autores { get; set; }
        public DateTimeOffset FechaPublicacion { get; set; }
        public string Categoria { get; set; }

        public List<BookModel> GetBooks()
        {
            var listBooks = new List<BookModel>();
            listBooks.Add(new BookModel()
            {
                ISBN = "9781430224792",
                Titulo = "Windows Azure Platform (Expert's Voice in .NET)",
                Categoria = "Comic",
                Autores = new List<string>() {"Redkar", "Tejasw" },
                FechaPublicacion = DateTimeOffset.Now.AddDays(-2)
            });

            listBooks.Add(new BookModel()
            {
                ISBN = "9780470506387",
                Titulo = "Cloud Computing with the Windows Azure Platform",
                Categoria = "Terror",
                Autores = new List<string>() { "Roger Jennings"},
                FechaPublicacion = DateTimeOffset.Now
            });

            listBooks.Add(new BookModel()
            {
                ISBN = "9780889222861",
                Titulo = "Azure Blues",
                Categoria = "Terror",
                Autores = new List<string>() { "Gilbert", "Gerry", "Rogery Landing"},                
                FechaPublicacion = DateTimeOffset.Now.AddMonths(-3)
            });

            listBooks.Add(new BookModel()
            {
                ISBN = "9780735649675",
                Titulo = "Moving Applications to the Cloud on the Microsoft Azure(TM) Platform",
                Categoria = "Fiction",
                Autores = new List<string>() { "Pace", "Eugenio Betts", "Dominic Densmore", "Scott; Dunn", "Ryan Narumoto", "Masashi Woloski", "Matias" },
                FechaPublicacion = DateTimeOffset.Now
            });

            return listBooks;
        }

        public override string ToString()
        {
            return "ISBN: " + this.ISBN + " - titulo: " + this.Titulo + " - Autores: " + String.Join(", ", this.Autores);     
        }
    }
}
