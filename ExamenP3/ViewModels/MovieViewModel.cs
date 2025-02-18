﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamenP3.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
                // Configuración de la ruta de la base de datos en un directorio 
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
            // Verificación del campo de búsqueda
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Message = "Ingrese un nombre de película.";
                Console.WriteLine("Campo de búsqueda vacío.");
                return;
            }

            try
            {
                // Consumo de la API solicitada
                var url = $"https://freetestapi.com/api/v1/movies?search={SearchQuery}&limit=1"; //profe al final tuve un problema con la api , al principio daba la pelicula pero solo algunas caracteristicas pero luego la actualice y ahora no se consume correctamente :(
                using var httpClient = new HttpClient();

                // Obtención de la respuesta de la API
                var response = await httpClient.GetFromJsonAsync<ApiResponse>(url);
                Console.WriteLine($"Respuesta de la API: {response}");

                if (response?.Data?.Length > 0)
                {
                    var movieData = response.Data[0];

                    // Creación de un nuevo objeto con los resultados obtenidos
                    var movie = new Movie
                    {
                        Title = movieData.Title,
                        Genre = movieData.Genre?.FirstOrDefault() ?? "Desconocido",
                        LeadActor = movieData.Actors?.FirstOrDefault() ?? "Desconocido",
                        Awards = movieData.Awards ?? "No especificado",
                        Website = movieData.Website ?? "No disponible",
                        CustomName = "rgualoto"
                    };

                    // Guardar la información de la película dentro de la base de datos y actualizar la lista
                    _db.Insert(movie);
                    Movies.Add(movie);

                    Message = "Película guardada con éxito.";
                    Console.WriteLine($"Película guardada: {movie.Title}");
                }
                else
                {
                    Message = "No se encontró ninguna película.";
                    Console.WriteLine("No se encontró ninguna película en la API.");
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

    // Clase para deserializar la respuesta de la API
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
