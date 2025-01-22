using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamenP3.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.IO;
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
            try
            {
                // Configurar la ruta de la base de datos en un directorio seguro
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ExamenP3_Movies.db");
                Console.WriteLine($"Ruta de la base de datos: {dbPath}");

                // Inicializar la conexión SQLite
                _db = new SQLiteConnection(dbPath);
                _db.CreateTable<Movie>();

                // Cargar datos existentes de la base de datos
                Movies = new ObservableCollection<Movie>(_db.Table<Movie>().ToList());
                Message = "Base de datos inicializada correctamente.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
                Message = "Error al inicializar la base de datos.";
            }
        }

        [RelayCommand]
        public async Task SearchMovie()
        {
            // Verificar si el campo de búsqueda está vacío
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Message = "Ingrese un nombre de película.";
                return;
            }

            try
            {
                // Construir la URL de búsqueda con el parámetro de consulta
                var url = $"https://freetestapi.com/api/v1/movies?search={SearchQuery}&limit=1";
                using var httpClient = new HttpClient();

                // Obtener la respuesta de la API
                var response = await httpClient.GetFromJsonAsync<ApiResponse>(url);

                if (response?.Data?.Length > 0)
                {
                    var movieData = response.Data[0];

                    // Crear un nuevo objeto Movie con los datos obtenidos
                    var movie = new Movie
                    {
                        Title = movieData.Title,
                        Genre = movieData.Genre?.FirstOrDefault() ?? "Desconocido",
                        LeadActor = movieData.Actors?.FirstOrDefault() ?? "Desconocido",
                        Awards = movieData.Awards ?? "No especificado",
                        Website = movieData.Website ?? "No disponible",
                        CustomName = "rgualoto"
                    };

                    // Guardar la película en la base de datos y actualizar la lista
                    _db.Insert(movie);
                    Movies.Add(movie);

                    Message = "Película guardada con éxito.";
                }
                else
                {
                    Message = "No se encontró ninguna película.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar la película: {ex.Message}");
                Message = "Error al buscar la película. Verifique su conexión a internet.";
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchQuery = string.Empty;
            Message = string.Empty;
        }
    }

    // Clases para deserializar la respuesta de la API
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
