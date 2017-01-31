﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using anime_downloader.Classes;
using anime_downloader.Enums;
using anime_downloader.Services.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace anime_downloader.ViewModels
{
    public class MiscViewModel : ViewModelBase
    {
        private int _selectedIndex;

        private readonly ISettingsService _settings;

        private readonly IAnimeAggregateService _animeAggregate;

        public MiscViewModel(ISettingsService settings, IAnimeAggregateService animeAggregate)
        {
            _settings = settings;
            _animeAggregate = animeAggregate;
            SelectedIndex = 0;
            SubmitCommand = new RelayCommand(DoAction);
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { Set(() => SelectedIndex, ref _selectedIndex, value); }
        }

        public RelayCommand SubmitCommand { get; set; }

        public bool LoggedIntoMal => _settings.MyAnimeListConfig.Works;

        // 

        private async void DoAction()
        {
            MessengerInstance.Send(new WorkMessage {Working = true});

            // Mark fully watched as completed
            if (SelectedIndex == 1)
            {
                var names = new List<string>();
                foreach (var anime in _animeAggregate.AnimeService.FullyWatched())
                {
                    anime.Status = Status.Finished;
                    anime.Airing = false;
                    names.Add(anime.Title);
                }

                _settings.Save();
                var result = names.Count > 0 ? string.Join(", ", names) : "no shows";
                Methods.Alert($"Marked {result} as finished. ");
            }

            // Move duplicates to My Videos
            else if (SelectedIndex == 2)
            {
                var moveCount = await _animeAggregate.FileService.MoveDuplicatesAsync();
                Methods.Alert($"Moved {moveCount} files to duplicate folder.");
            }

            // Regather shows with no ep. total
            else if (SelectedIndex == 3)
            {
                var updated = new List<string>();

                var needsUpdating = _animeAggregate.AnimeService
                    .AiringAndWatching
                    .Where(a => a.MyAnimeList.HasId && a.MyAnimeList.TotalEpisodes == 0)
                    .ToList();

                foreach (var anime in needsUpdating)
                {
                    var results = await _animeAggregate.MalService.Find(HttpUtility.UrlEncode(anime.Title));
                    var closest = results.FirstOrDefault(r => r.Id.Equals(anime.MyAnimeList.Id));
                    if (closest != null && !anime.MyAnimeList.TotalEpisodes.Equals(closest.TotalEpisodes))
                    {
                        updated.Add(anime.Title);
                        anime.MyAnimeList.TotalEpisodes = closest.TotalEpisodes;
                    }
                }

                if (updated.Count > 0)
                {
                    var updateResult = string.Join(", ", updated);
                    Methods.Alert($"Updated total episodes for {updateResult}.");
                }

                else
                {
                    Methods.Alert($"No shows were updated for an attempted {needsUpdating.Count} shows.");
                }
            }

            // Set current episode to last found file's episode number
            else if (SelectedIndex == 4)
            {
                var changed = new List<string>();

                await Task.Run(() =>
                {
                    foreach (var anime in _animeAggregate.AnimeService.AiringAndWatching)
                    {
                        var lastEpisode = _animeAggregate.FileService.LastEpisode(anime);
                        if (lastEpisode != null && anime.Episode != lastEpisode.Episode)
                        {
                            anime.Episode = lastEpisode.Episode;
                            changed.Add(anime.Title);
                        }
                    }
                });

                if (changed.Count > 0)
                    Methods.Alert($"Updated episodes for: {string.Join(", ", changed)}");
                else
                    Methods.Alert("No re-indexes were needed.");
            }
            
            MessengerInstance.Send(new WorkMessage {Working = false});
        }

        // 
    }
}