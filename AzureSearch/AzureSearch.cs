
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

                //Define SuggesterList
                List<Suggester> suggesterList = new List<Suggester>();
                suggesterList.Add(new Suggester() { Name = "suggester", SearchMode = SuggesterSearchMode.AnalyzingInfixMatching, SourceFields = new List<string>() { "Titulo", "Autores" } });
                //Define ScoringList
                List<ScoringProfile> scoringsList = new List<ScoringProfile>();
                Dictionary<string, double> textWieghtsDictionary = new Dictionary<string, double>();
                textWieghtsDictionary.Add("Autores", 1.5);
                scoringsList.Add(new ScoringProfile()
                {
                    Name = "ScoringTest",
                    TextWeights = new TextWeights(textWieghtsDictionary)
                }
                );
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

                    },
                    //Add Suggesters
                    Suggesters = suggesterList,
                    //Add Scorings
                    ScoringProfiles = scoringsList
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

                 //Suggest
                 Console.WriteLine("\n{0}", "Suggest by Ro without fuzzy \n");
                SuggestionSearch(indexClient, suggesterText: "Ro", suggester: "suggester", fuzzy: false);

                //Suggest
                Console.WriteLine("\n{0}", "Suggest by Ro with fuzzy \n");
                SuggestionSearch(indexClient, suggesterText: "Ro", suggester: "suggester", fuzzy: true);

                //Scoring
                Console.WriteLine("{0}", "Searching documents 'Cloud'...\n");
                Search(indexClient, searchText: "Cloud", scoringName: "ScoringTest");                

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

        private static void Search(SearchIndexClient indexClient, string searchText, string filter = null, List<string> order = null, List<string> facets = null, string scoringName = null)
        {
            // Execute search based on search text and optional filter
            var sp = new SearchParameters();

            //Add Filter
            if (!String.IsNullOrEmpty(filter))
            {
                sp.Filter = filter;
            }

            //Order
            if (order != null && order.Count > 0)
            {
                sp.OrderBy = order;
            }

            //facets
            if (facets != null && facets.Count > 0)
            {
                sp.Facets = facets;
            }

            if (!string.IsNullOrEmpty(scoringName))
            {
                Console.WriteLine("Apply scoring: " + scoringName);
                sp.ScoringProfile = scoringName;
            }

            //Search
            DocumentSearchResult<BookModel> response = indexClient.Documents.SearchAsync<BookModel>(searchText, sp).Result;
            foreach (SearchResult<BookModel> result in response.Results)
            {
                Console.WriteLine(result.Document + " - Score: " + result.Score);
            }
            if (response.Facets != null)
            {
                foreach (var facet in response.Facets)
                {
                    Console.WriteLine("Facet Name: " + facet.Key);
                    foreach (var value in facet.Value)
                    {
                        Console.WriteLine("Value :" + value.Value + " - Count: " + value.Count);
                    }
                }
            }
        }
        private static void SuggestionSearch(SearchIndexClient indexClient, string suggesterText, string suggester, bool fuzzy)
        {
            SuggestParameters suggestParameters = new SuggestParameters()
            {
                Top = 10,
                UseFuzzyMatching = fuzzy,
                SearchFields = new List<string> { "Autores" }
            };

            DocumentSuggestResult response = indexClient.Documents.Suggest(suggesterText, suggester, suggestParameters, searchRequestOptions: new SearchRequestOptions(Guid.NewGuid()));
            foreach (var suggestion in response.Results)
            {
                Console.WriteLine("Suggestion: " + suggestion.Text);
            }
        }
    }
}