using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LastFM.ApiClient.Models;

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
                    trackDescription = $"'{trackName}' by '{artistName}'";
                }
                else if (!string.IsNullOrEmpty(trackName))
                {
                    trackDescription = $"'{trackName}' ";
                }
                else if (!string.IsNullOrEmpty(artistName))
                {
                    trackDescription = $"An unknown track by '{artistName}'";
                }
                else
                {
                    trackDescription = $"An unknown track by an unknown artist";
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
