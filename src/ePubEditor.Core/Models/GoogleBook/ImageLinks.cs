﻿using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class ImageLinks
    {
        [JsonPropertyName("smallThumbnail")]
        public string SmallThumbnail { get; set; }

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; }
    }


}
