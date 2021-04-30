using System;
using SuperTouristAPI.Models;

namespace SuperTouristAPI.Data
{
    internal class ContractSeed
    {
        public int TouristId { get; set; }

        public int ServiceId { get; set; }

        public DateTime DateFrom { get; set; } //HasSpecificTime 00:00!=00:00

        public DateTime DateTo { get; set; }

        public DateTime DateCreated { get; set; }

        public TimeSpan HourFrom { get; set; }

        public TimeSpan HourTo { get; set; }

        public int NumberOfTourists { get; set; }
        
        public string PassportNumber { get; set; }
        /*
        public decimal Price { get; set; }

        public bool IsPaid { get; set; }

        public bool IsArchived { get; set; }

        public bool ContractCanceled { get; set; }

        public bool CancelRequested { get; set; }
        */

        public string Price { get; set; }

        public int IsPaid { get; set; }

        public int IsArchived { get; set; }

        public int ContractCanceled { get; set; }

        public int CancelRequested { get; set; }
    }
}