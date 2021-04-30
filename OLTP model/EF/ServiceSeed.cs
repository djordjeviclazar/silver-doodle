using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SuperTouristAPI.Models;

namespace SuperTouristAPI.Data
{
    internal class ServiceSeed
    {
        public int TravelAgencyId { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public string LocationFrom { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string LocationTo { get; set; }

        //public decimal Price { get; set; }
        public string Price { get; set; }

        public string Currency { get; set; }

        public string About { get; set; }

        public int MaxNumberOfContracts { get; set; }

        public int NumberOfPeople { get; set; }

        //public bool IsArchived { get; set; }
        public int IsArchived { get; set; }

        //public bool IsLimited { get; set; }
        public int IsLimited { get; set; }

        public TimeSpan? HourFrom { get; set; }

        public TimeSpan? HourTo { get; set; }

        public static void MapSeedToService(DataContext context, ServiceSeed seed)
        {
            /*
            Service newService = new Service{
                            TravelAgency = context.TravelAgencies.FirstOrDefault(a => a.Id == seed.TravelAgencyId),
                            Name = seed.Name,
                            Type = context.ServiceTypes.FirstOrDefault(a => a.Id == seed.TypeId),
                            LocationFrom = seed.LocationFrom,
                            LocationTo = seed.LocationTo,
                            DateTo = seed.DateTo,
                            DateFrom = seed.DateFrom,
                            Price = seed.Price,
                            Currency = seed.Currency,
                            About = seed.About,
                            MaxNumberOfContracts = seed.MaxNumberOfContracts,
                            NumberOfPeople = seed.NumberOfPeople,
                            IsArchived = seed.IsArchived,
                            IsLimited = seed.IsLimited,
                            HourFrom = seed.HourFrom,
                            HourTo = seed.HourTo
                            //Images = 1
                        };
            
            context.Services.Add(newService);
            context.SaveChanges();
            */
        }
    }
}