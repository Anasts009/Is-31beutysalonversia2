// Models/ServiceViewModel.cs
using System;

namespace BeautySalon.Models
{
    public class ServiceViewModel
    {
        public int ServiceId { get; set; }
        public string NameServices { get; set; } = "";
        public string? DescriptionService { get; set; }
        public decimal Price { get; set; }

        // В БД time — integer (минуты)
        public int Time { get; set; }

        // Отображение в формате ЧЧ:ММ
        public string TimeDisplay => $"{Time / 60:D2}:{Time % 60:D2}";
    }
}