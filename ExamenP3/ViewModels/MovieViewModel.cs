﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            // Configurar base de datos SQLite
            _db = new SQLiteConnection("ExamenP3_Movies.db");
            _db.CreateTable<Movie>();
            Movies = new ObservableCollection<Movie>(_db.Table<Movie>().ToList());
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
                // Construir la URL para la API
                var url = $"https://freetestapi.com/api/v1/movies?search={SearchQuery}&limit=1";
                using var httpClient = new HttpClient();

                // Obtener datos desde la API
                var response = await httpClient.GetFromJsonAsync<ApiResponse>(url);

                if (response?.Data?.Length > 0)
                {
                    // Extraer la primera película de la respuesta
                    var movieData = response.Data[0];
                    var movie = new Movie
                    {
                        Title = movieData.Title,
                        Genre = movieData.Genre?.FirstOrDefault() ?? "Desconocido",
                        LeadActor = movieData.Actors?.FirstOrDefault() ?? "Desconocido",
                        Awards = movieData.Awards ?? "No especificado",
                        Website = movieData.Website ?? "No disponible",
                        CustomName = "rgualoto"
                    };

                    // Guardar en la base de datos SQLite
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
                Message = "Error al buscar la película. Verifique su conexión a internet.";
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            // Limpiar el campo de búsqueda
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
