﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Models
{
    public class TalkModel
    {
        [Required]
        public int TalkId { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required]
        [StringLength(400, MinimumLength = 20)]
        public string Abstract { get; set; }
        [Range(100, 500)]
        public int Level { get; set; }

        public SpeakerModel Speaker { get; set; }
    }
}
