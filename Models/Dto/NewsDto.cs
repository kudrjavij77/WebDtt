using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Dto
{
    public class NewsDto
    {
        public string Body { get; set; }
        public string Title { get; set; }
        public DateTime CreateDate { get; set; }

        public NewsDto(News news)
        {
            if (news == null) return;
            Title = news.Title;
            CreateDate = news.CreateDate;
            Body = news.Body.Length > 300 ? news.Body.Substring(0, 300) : news.Body;
        }
    }
}