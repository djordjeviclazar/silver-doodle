using System;

namespace SuperTouristAPI.Data
{
    internal class ReviewSeed
    {
        public int Tourist { get; set; }

        public int Agency { get; set; }

        public int Contract { get; set; }

        public string TextContent { get; set; }

        public int Grade { get; set; }

        public DateTime DateTimeAdded { get; set; }

        public string ReviewImageUrl { get; set; }
    }
}