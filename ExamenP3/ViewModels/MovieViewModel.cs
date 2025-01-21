using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreSpotlight;
using ExamenP3.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace ExamenP3.ViewModels
{
    public partial class MovieViewModel : ObservableObject
    {
        private readonly SQLiteConnection _db;

        [ObservableProperty]
        private string _searchQuery;

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private ObservableCollection<Movie> _movies;

        public MovieViewModel()
        {
            _db = new SQLiteConnection("ExamenP3_Movies.db");
            _db.CreateTable<Movie>();
            Movies = new ObservableCollection<Movie>(_db.Table<Movie>().ToList());
        }

        [RelayCommand]
        public async Task SearchMovie()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Message = "Ingrese un nombre de película.";
                return;
            }

            try
            {
                var url = $"https://freetestapi.com/api/v1/movies?search={SearchQuery}&limit=1";
                using var httpClient = new HttpClient();
                var response = await httpClient.GetFromJsonAsync<ApiResponse>(url);

                if (response?.Data?.Length > 0)
                {
                    var movieData = response.Data[0];
                    var movie = new Movie
                    {
                        Title = movieData.Title,
                        Genre = movieData.Genre[0],
                        LeadActor = movieData.Actors[0],
                        Awards = movieData.Awards,
                        Website = movieData.Website,
                        CustomName = "SCordova"
                    };

                    _db.Insert(movie);
                    Movies.Add(movie);
                    Message = "Película guardada con éxito.";
                }
                else
                {
                    Message = "No se encontró ninguna película.";
                }
            }
            catch
            {
                Message = "Error al buscar la película.";
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchQuery = string.Empty;
        }
    }

    public class ApiResponse
    {
        public MovieData[] Data { get; set; }
    }

    public class MovieData
    {
        public string Title { get; set; }
        public string[] Genre { get; set; }
        public string[] Actors { get; set; }
        public string Awards { get; set; }
        public string Website { get; set; }
    }
}
