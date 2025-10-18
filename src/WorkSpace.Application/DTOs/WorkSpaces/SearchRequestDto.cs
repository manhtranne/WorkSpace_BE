
using System;
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class SearchRequestDto
    {
        public string? LocationQuery { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTimeOfDay { get; set; }
        public TimeSpan? EndTimeOfDay { get; set; }
        public int? Capacity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? Amenities { get; set; }
        public string? Keyword { get; set; }

        public DateOnly GetSearchStartDateOnly() => StartDate.HasValue ? DateOnly.FromDateTime(StartDate.Value.Date) : DateOnly.FromDateTime(DateTime.Today);
        public DateOnly GetSearchEndDateOnly() => EndDate.HasValue ? DateOnly.FromDateTime(EndDate.Value.Date) : GetSearchStartDateOnly();
        public bool HasDateFilter() => StartDate.HasValue || EndDate.HasValue;
        public bool HasTimeOfDayFilter() => StartTimeOfDay.HasValue || EndTimeOfDay.HasValue; 
    }
}