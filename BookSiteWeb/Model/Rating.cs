﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookSiteWeb.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int Rate { get; set; }
        public int? Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int? ChapterId { get; set; }
        public Chapter Chapter { get; set; }
    }
}