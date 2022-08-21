﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BinhDinhFoodWeb.Models
{
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }
        [Required]
        public String BlogName { get; set; }
        [DisplayName("Nội dung")]
        public string BlogContent { get; set; }
        [DisplayName("Ngày thêm")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BlogDateCreated { get; set; } = DateTime.Now;

    }
}