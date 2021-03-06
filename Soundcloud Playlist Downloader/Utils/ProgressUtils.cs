﻿using Soundcloud_Playlist_Downloader.JsonObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Soundcloud_Playlist_Downloader.Utils
{
    public class ProgressUtils
    {
        public bool IsActive { get; set; }
        public int SongsToDownload;
        public int SongsDownloaded;
        public bool Completed { get; set; }
        public bool Exiting { get; set; }
        public bool IsError { get; set; }
        public ConcurrentQueue<Exception> Exceptions { get; set; }
        public static readonly int MaximumExceptionsCount = 10;
        public int CurrentAmountOfExceptions { get; set; }

        private ConcurrentDictionary<string, string> TrackProgress = new ConcurrentDictionary<string, string>();

        public ProgressUtils()
        {
            Completed = true;
            Exiting = false;
            IsError = false;
            IsActive = false;
            SongsDownloaded = 0;
            SongsToDownload = 0;
            TrackProgress = new ConcurrentDictionary<string, string>();
            Exceptions = new ConcurrentQueue<Exception>();
            CurrentAmountOfExceptions = 0;
        }

        public void AddOrUpdateFailedTrack(Track track, Exception e)
        {
            var exc = new Exception($"Exception while downloading track '{track.Title}' from artist '{track.Artist}'", e);
            this.CurrentAmountOfExceptions++;
            this.Exceptions.Enqueue(exc);
            this.TrackProgress.AddOrUpdate(track.id.ToString(), track.Title, (key, oldValue) => "[x] " + track.Title);
        }

        public void AddOrUpdateInProgressTrack(Track track)
        {
            this.TrackProgress.AddOrUpdate(track.id.ToString(), "[~] " + track.Title, (key, oldValue) => track.Title);
        }

        public void AddOrUpdateNotDownloadableTrack(Track track)
        {
            this.TrackProgress.AddOrUpdate(track.id.ToString(), track.Title, (key, oldValue) => "[o] " + track.Title);
        }

        public void AddOrUpdateSuccessFullTrack(Track track)
        {
            this.TrackProgress.AddOrUpdate(track.id.ToString(), track.Title, (key, oldValue) => "[✓] " + track.Title);
        }

        public ICollection<string> GetTrackProgressValues()
        {
            return this.TrackProgress.Values;
        }

        public void ThrowAllExceptionsWithMessage(string message)
        {
            ThrowAllExceptionsWithRootException(new Exception(message));
        }

        public void ThrowAllExceptionsWithRootException(Exception rootException)
        {
            if (CurrentAmountOfExceptions > 0)
                throw new AggregateException(rootException.Message, this.Exceptions);
            throw rootException;
        }

        public void ResetProgress()
        {
            SongsDownloaded = 0;
            SongsToDownload = 0;
            TrackProgress = new ConcurrentDictionary<string, string>();
            Exceptions = new ConcurrentQueue<Exception>();
            IsActive = true;
            IsError = false;
            CurrentAmountOfExceptions = 0;
        }
    }


}
