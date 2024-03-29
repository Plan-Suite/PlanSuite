﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("blog_posts")]
    public class BlogPost
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("author_id")]
        public Guid AuthorId { get; set; }

        [Column("date_posted")]
        public DateTime DatePosted { get; set; }

        [Column("date_modified")]
        public DateTime? DateModified { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("summary")]
        public string Summary { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("slug")]
        public string Slug { get; set; }

        [Column("comments_disabled")]
        public bool CommentsDisabled { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("keywords")]
        public string Keywords { get; set; }
    }
}