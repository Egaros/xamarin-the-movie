﻿using Plugin.Connectivity;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheMovie.Models;

namespace TheMovie.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {        
        // Variables to control of the pagination
        private int currentPage = 1;
        private int totalPage = 0;

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set { SetProperty(ref isConnected, value); }
        }

        private List<Genre> genres;        

        public ObservableCollection<Movie> Movies { get; set; }

        public DelegateCommand LoadUpcomingMoviesCommand { get; }
        public DelegateCommand SearchMoviesCommand { get; }        
        public DelegateCommand<Movie> ShowMovieDetailCommand { get; }
        public DelegateCommand<Movie> ItemAppearingCommand { get; }

        private readonly INavigationService navigationService;
        public MainPageViewModel(INavigationService navigationService)
        {
            Title = "TMDb - Upcoming Movies";
            this.navigationService = navigationService;
            Movies = new ObservableCollection<Movie>();

            LoadUpcomingMoviesCommand = new DelegateCommand(async () => await ExecuteLoadUpcomingMoviesCommand().ConfigureAwait(false));
            SearchMoviesCommand = new DelegateCommand(async () => await ExecuteSearchMoviesCommand().ConfigureAwait(false));
            ShowMovieDetailCommand = new DelegateCommand<Movie>(async (Movie movie) => await ExecuteShowMovieDetailCommand(movie).ConfigureAwait(false));
            ItemAppearingCommand = new DelegateCommand<Movie>(async (Movie movie) => await ExecuteItemAppearingCommand(movie).ConfigureAwait(false));

            LoadUpcomingMoviesCommand.Execute();
        }        

        private async Task ExecuteLoadUpcomingMoviesCommand()
        {
            IsConnected = CrossConnectivity.Current.IsConnected;

            if ((IsBusy) || (!IsConnected))
                return;

            IsBusy = true;

            try
            {
                Movies.Clear();                
                await LoadMoviesAsync(currentPage = 1, Enums.MovieCategory.Upcoming).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteSearchMoviesCommand()
        {            
            await navigationService.NavigateAsync("SearchMoviesPage").ConfigureAwait(false);
        }

        private async Task ExecuteShowMovieDetailCommand(Movie movie)
        {            
            var p = new NavigationParameters();
            p.Add(nameof(movie), movie);
            await navigationService.NavigateAsync("MovieDetailPage", p).ConfigureAwait(false);
        }

        private async Task ExecuteItemAppearingCommand(Movie movie)
        {
            int itemLoadNextItem = 2;
            int viewCellIndex = Movies.IndexOf(movie);
            if (Movies.Count - itemLoadNextItem <= viewCellIndex)
            {                
                await NextPageUpcomingMoviesAsync().ConfigureAwait(false);
            }
        }

        private async Task LoadMoviesAsync(int page, Enums.MovieCategory movieCategory)
        {
            try
            {
                genres = genres ?? await ApiService.GetGenresAsync().ConfigureAwait(false);
                var movies = await ApiService.GetMoviesByCategoryAsync(page, movieCategory).ConfigureAwait(false);
                if (movies != null)
                {
                    totalPage = movies.TotalPages;
                    foreach (var item in movies.Movies)
                    {
                        GenreListToString(genres, item);
                        Movies.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task NextPageUpcomingMoviesAsync()
        {
            currentPage++;
            if (currentPage <= totalPage)
            {                
                await LoadMoviesAsync(currentPage, Enums.MovieCategory.Upcoming).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Converter the genres of the movies to a string.
        /// </summary>
        /// <param name="genres"></param>
        /// <param name="item"></param>
        private void GenreListToString(List<Genre> genres, Movie item)
        {            
            StringBuilder genresNames = new StringBuilder();
            for (int i = 0; i < item.GenreIds.Length; i++)
            {
                var genreId = item.GenreIds[i];                
                var genreName = genres.FirstOrDefault(g => g.Id == genreId)?.Name;
                genreName += i < (item.GenreIds.Length - 1) ? ", " : "";
                genresNames.Append(genreName);
            }
            item.GenresNames = genresNames.ToString();
        }        
    }
}