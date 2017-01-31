﻿using anime_downloader.Enums;
using anime_downloader.Services.Interfaces;
using anime_downloader.ViewModels.Components;
using GalaSoft.MvvmLight;

namespace anime_downloader.ViewModels
{
    public class ManageViewModel : ViewModelBase
    {
        private FileListViewModel _unwatched;

        private FileListViewModel _watched;
        
        // 

        public ManageViewModel(ISettingsService settings, IAnimeAggregateService animeAggregate)
        {
            Unwatched = new FileListViewModel(animeAggregate.FileService, animeAggregate.AnimeService,
                animeAggregate.PlaylistService)
            {
                Title = "Unwatched",
                ImageResourcePath = "../Resources/Images/right.png",
                EpisodeType = EpisodeStatus.Unwatched,
                StartPath = settings.PathConfig.Unwatched,
                MovePath = settings.PathConfig.Watched
            };

            Watched = new FileListViewModel(animeAggregate.FileService, animeAggregate.AnimeService,
                animeAggregate.PlaylistService)
            {
                Title = "Watched",
                ImageResourcePath = "../Resources/Images/left.png",
                EpisodeType = EpisodeStatus.Watched,
                StartPath = settings.PathConfig.Watched,
                MovePath = settings.PathConfig.Unwatched,
                HideLabel = true
            };
        }

        // 
        
        public FileListViewModel Unwatched
        {
            get { return _unwatched; }
            set { Set(() => Unwatched, ref _unwatched, value); }
        }

        public FileListViewModel Watched
        {
            get { return _watched; }
            set { Set(() => Watched, ref _watched, value); }
        }
    }
}