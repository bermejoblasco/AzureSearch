
namespace AzureSearch
{
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    class AzureSearch
    {
        static void Main(string[] args)
        {
            try
            {
                var indexName = "example";
                SearchServiceClient azureSearchService = new SearchServiceClient(ConfigurationManager.AppSettings["AzureSearchServiceName"], new SearchCredentials(ConfigurationManager.AppSettings["AzureSearchKey"]));
                SearchIndexClient indexClient = azureSearchService.Indexes.GetClient(indexName);
                var listBooks = new BookModel().GetBooks();

                if (azureSearchService.Indexes.Exists(indexName))
                {
                    azureSearchService.Indexes.Delete(indexName);
                }

                //Create Index Model
                Index indexModel = new Index()
                {
                    Name = indexName,
                    Fields = new[]
                    {
                        new Field("ISBN", DataType.String) { IsKey = true, IsRetrievable = true, IsFacetable = false },
                        new Field("Titulo", DataType.String) {IsRetrievable = true, IsSearchable = true, IsFacetable = false },
                        new Field("Autores", DataType.Collection(DataType.String)) {IsSearchable = true, IsRetrievable = true, IsFilterable = true, IsFacetable = false },
                        new Field("FechaPublicacion", DataType.DateTimeOffset) { IsFilterable = true, IsRetrievable = false, IsSortable = true, IsFacetable = false },
                        new Field("Categoria", DataType.String) { IsFacetable = true, IsFilterable= true, IsRetrievable = true }

                    }
                };
                //Create Index in AzureSearch
                var resultIndex = azureSearchService.Indexes.Create(indexModel);
                //Add documents in our Index
                azureSearchService.Indexes.GetClient(indexName).Documents.Index(IndexBatch.MergeOrUpload<BookModel>(listBooks));
                //Search by word
                Console.WriteLine("{0}", "Searching documents 'Cloud'...\n");
                Search(indexClient, searchText: "Cloud");
                //Search all and filter
                Console.WriteLine("\n{0}", "Filter documents by Autores 'Eugenio Betts'...\n");
                Search(indexClient, searchText: "*", filter: "Autores / any(t: t eq 'Eugenio Betts')");

                //Search all and order
                Console.WriteLine("\n{0}", "order by FechaPublicacion\n");
                Search(indexClient, searchText: "*", order: new List<string>() { "FechaPublicacion" });

                ////Search all and facet
                Console.WriteLine("\n{0}", "Facet by Categoria \n");
                Search(indexClient, searchText: "*", facets: new List<string>() { "Categoria" });

                Console.WriteLine("Process Ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            finally
            {
                Console.WriteLine("Press any key");
                Console.ReadKey();
            }
        }

        private static void Search(SearchIndexClient indexClient, string searchText, string filter = null, List<string> order = null, List<string> facets = null)
        {
            // Execute search based on search text and optional filter
            var sp = new SearchParameters();

            //Add Filter
            if (!String.IsNullOrEmpty(filter))
            {
                sp.Filter = filter;
            }

            //Order
            if(order!=null && order.Count>0)
            {
                sp.OrderBy = order;
            }
            
            //facets
            if (facets != null && facets.Count > 0)
            {
                sp.Facets = facets;
            }


            //Search
            DocumentSearchResult<BookModel> response = indexClient.Documents.SearchAsync<BookModel>(searchText, sp).Result;
            foreach (SearchResult<BookModel> result in response.Results)
            {
                Console.WriteLine(result.Document);
            }
            if (response.Facets != null)
            {
                foreach (var facet in response.Facets)
                {
                    Console.WriteLine("Facet Name: " + facet.Key);
                    foreach(var value in facet.Value)
                    {
                        Console.WriteLine("Value :" + value.Value + " - Count: " + value.Count);
                    }
                }
            }
        }
    }
}
