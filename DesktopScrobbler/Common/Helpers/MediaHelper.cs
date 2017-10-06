using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LastFM.ApiClient.Models;
using LastFM.Common.Localization;

namespace LastFM.Common.Helpers
{
    public static class MediaHelper
    {
        public static string GetTrackDescription(MediaItem mediaItem, int descriptionMaxLength=0)
        {
            string trackDescription = string.Empty;

            string trackName = mediaItem?.TrackName;
            string artistName = mediaItem?.ArtistName;

            if (descriptionMaxLength == 0)
            {
                if (!string.IsNullOrEmpty(trackName) && !string.IsNullOrEmpty(artistName))
                {
                    trackDescription = string.Format(LocalizationStrings.MediaHelper_TrackDescription_Complete, trackName, artistName);
                }
                else if (!string.IsNullOrEmpty(trackName))
                {
                    trackDescription =  string.Format(LocalizationStrings.MediaHelper_TrackDescription_TrackOnly, trackName);
                }
                else if (!string.IsNullOrEmpty(artistName))
                {
                    trackDescription = string.Format(LocalizationStrings.MediaHelper_TrackDescription_ArtistOnly, artistName);
                }
                else
                {
                    trackDescription = LocalizationStrings.MediaHelper_TrackDescription_Unknown;
                }
            }

            if (descriptionMaxLength > 0 && trackDescription.Length > descriptionMaxLength)
            {
                trackDescription = $"{trackDescription.Substring(0, descriptionMaxLength)} ...";
            }

            return trackDescription;
        }
    }
}
