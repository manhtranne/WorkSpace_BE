using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Domain.Entities
{
    public class SearchQueryHistory
    {
        public int Id { get; set; }
        public int? UserId { get; set; }                // có thể null nếu khách vãng lai
        public string? Ward { get; set; }               // phường (text/mã)
        public DateOnly? Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Participants { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public string? AmenityIdsCsv { get; set; }      // "1,2,3"
        public string? QueryText { get; set; }          // nếu sau này có free text
        public int? ResultsCount { get; set; }
        public string? ClientIp { get; set; }
        public string? UserAgent { get; set; }

    }
}
