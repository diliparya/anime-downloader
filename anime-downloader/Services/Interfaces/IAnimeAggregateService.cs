﻿namespace anime_downloader.Services.Interfaces
{
    /// <summary>
    ///     A collection of the main services used throughout the application.
    /// </summary>
    public interface IAnimeAggregateService
    {
        IAnimeService AnimeService { get; set; }
        IFileService FileService { get; set; }
        IDownloadService DownloadService { get; set; }
        IMyAnimeListService MalService { get; set; }
        IPlaylistService PlaylistService { get; set; }
    }
}