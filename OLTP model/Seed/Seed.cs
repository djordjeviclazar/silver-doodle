using System.Collections.Generic;
using System.Linq;
using SuperTouristAPI.Models;
using Newtonsoft.Json;
using SuperTouristAPI.Helpers;
using SuperTouristAPI.Dtos;
using System;
using SuperTouristAPI.Cache;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace SuperTouristAPI.Data
{
    public class Seed
    {
        public static void SeedUsers(DataContext context, SuperTouristCache cache)
        {
            if (!context.Users.Any())
            {
                //var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var userData = System.IO.File.ReadAllText("E:/SuperTourist/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                foreach (var user in users)
                {
                    GeneratePassword("password", out byte[] passwordHash, out byte[] passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                    if (user.Role == Role.Agency)
                    {
                        var agency = new TravelAgency();
                        agency.User = user;
                        context.TravelAgencies.Add(agency);
                    }
                    else if (user.Role == Role.Tourist)
                    {
                        var tourist = new Tourist();
                        tourist.User = user;
                        context.Tourists.Add(tourist);
                    }
                }
                context.SaveChanges();
            }
            /*if (!context.Services.Any())
            {
                var serviceDatesData = System.IO.File.ReadAllText("Data/ServiceDateSeed.json");
                var serviceDates = JsonConvert.DeserializeObject<List<ServiceDateSeedDTO>>(serviceDatesData);

                var serviceData = System.IO.File.ReadAllText("Data/ServiceSeedData.json");
                var services = JsonConvert.DeserializeObject<List<ServiceSeedDTO>>(serviceData);

                foreach (var service in services)
                {
                    
                }



                foreach (var service in services)
                {
                    var agency = context.TravelAgencies.FirstOrDefault(x => x.Id == service.AgencyId);
                    var serviceModel = new Service
                    {
                        Name = service.Name,
                        TravelAgency = agency,
                        LocationFrom = service.LocationFrom,
                        LocationTo = service.LocationTo,
                        DateFrom = null,
                        DateTo = null,
                        Price = service.Price,
                        About = service.About,
                        MaxNumberOfContracts = service.MaxNumberOfContracts,
                        NumberOfPeople = service.NumberOfPeople,
                        IsLimited = service.IsLimited,
                        HourFrom = service.HourFrom ?? TimeSpan.Zero,
                        HourTo = service.HourTo ?? TimeSpan.Zero
                    };
                    context.Services.Add(serviceModel);
                }

                context.SaveChanges();

                int[] arr = new int[services.Count];

                

                foreach (var date in serviceDates)
                {
                    var service = context.Services.FirstOrDefault(x => x.Id == date.Service);
                    if (arr[service.Id - 1] == 1)
                    {
                        var agency = context.TravelAgencies.FirstOrDefault(x => x.Id == service.TravelAgency.Id);
                        var serviceModel = new Service
                        {
                            Name = service.Name,
                            TravelAgency = agency,
                            LocationFrom = service.LocationFrom,
                            LocationTo = service.LocationTo,
                            DateFrom = date.DateFrom,
                            DateTo = date.DateTo,
                            Price = service.Price,
                            About = service.About,
                            MaxNumberOfContracts = service.MaxNumberOfContracts,
                            NumberOfPeople = service.NumberOfPeople,
                            IsLimited = service.IsLimited,
                            HourFrom = service.HourFrom ?? TimeSpan.Zero,
                            HourTo = service.HourTo ?? TimeSpan.Zero
                        };
                        context.Services.Add(serviceModel);
                    }
                    else
                    {
                        service.DateFrom = date.DateFrom;
                        service.DateTo = date.DateTo;
                        context.Update(service);
                        arr[service.Id - 1]++;
                    }

                    context.SaveChanges();
                }

            }

            if (!context.ServiceTypes.Any())
            {
                var serviceTypesData = System.IO.File.ReadAllText("Data/ServiceTypeSeed.json");
                var servicesTypes = JsonConvert.DeserializeObject<List<ServiceTypeSeed>>(serviceTypesData);

                foreach (var servicesType in servicesTypes)
                {
                    var myService = context.Services.FirstOrDefault(s => s.Id == servicesType.Service);
                    var serviceTypeModel = new ServiceType
                    {
                       // Service = myService,
                        Type = servicesType.Type
                    };
                    context.ServiceTypes.Add(serviceTypeModel);
                }
            }

            if (!context.Contracts.Any())
            {
                var contractsData = System.IO.File.ReadAllText("Data/ContractSeedData.json");
                var contracts = JsonConvert.DeserializeObject<List<ContractSeedDTO>>(contractsData);

                foreach (var contract in contracts)
                {
                    var contractService = context.Services.FirstOrDefault(s => s.Id == contract.Service);
                    var contractTourist = context.Tourists.FirstOrDefault(s => s.Id == contract.Tourist);
                    var contractModel = new Contract
                    {
                        Tourist = contractTourist,
                        Service = contractService,
                        DateFrom = contract.DateFrom,
                        DateTo = contract.DateTo,
                        HourFrom = contract.HourFrom ?? default(TimeSpan),
                        HourTo = contract.HourTo ?? default(TimeSpan),
                        NumberOfTourists = contract.NumberOfTourists,
                        Price = contract.Price,
                        IsPaid = contract.IsPaid,
                        IsArchived = contract.IsArchived,
                        ContractCanceled = contract.ContractCanceled,
                        CancelRequested = contract.CancelRequested
                    };
         
                   context.Contracts.Add(contractModel);
                }

                context.SaveChanges();
            }

            GenerateCache(context, cache);
*/
        }

        private static void GenerateCache(DataContext context, SuperTouristCache cache)
        {
            var servicesForCache = context.Services.ToList();
            var contracts = context.Contracts.Include(x => x.Service).ToList();

            foreach (var service in servicesForCache)
            {
                cache.Add(
                    new ServiceCacheModel
                    {
                        Id = service.Id,
                        MaxNumberOfPeople = service.MaxNumberOfContracts * service.NumberOfPeople,
                        IsLimited = service.IsLimited,
                        RegistratedNumberOfPeople = contracts.Where(x => x.Service.Id == service.Id).Select(c => c.NumberOfTourists).Sum()
                    }
                );
            }
        }

        private static void GeneratePassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public static void GenerateServicesJson(DataContext context)
        {
            List<ServiceSeed> genServices = new List<ServiceSeed>();
            List<ContractSeed> genContracts = new List<ContractSeed>();
            List<ReviewSeed> genReviews = new List<ReviewSeed>();

            // Assume: 30 agencies, 100 tourists
            int agencyNum = 30, touristNum = 100;
            Random random = new Random();
            DateTime datePom;
            int[] shuffleArray = new int[touristNum];
            for (int i = 0; i < shuffleArray.Length; i++)
            {
                shuffleArray[i] = i + 1;
            }

            string[] locations = {"Nis, Serbia", "Belgrade, Serbia", "Barcelona, Spain", "New York, US", "Split, Croatia", "Kavala, Greece", "Naples, Italy", "Valencia, Spain", "Boon Lay, Singapoure", "Rome, Italy"};

            int duration = 0, contractNum = 0, serviceSum = 0;
            for (int i = 1; i <= agencyNum; i++)
            {
                // create service for every agency
                int serviceNum = random.Next(5, 20);
                for (int k = 1; k <= serviceNum; k++)
                {
                    int type = 1;
                    int price = 5;

                    type = random.Next(0, 1000);
                    if(type < 200) 
                    { 
                        price = random.Next(5, 10);
                        type = 1; 
                    }
                    else
                    {
                        if (type < 300) 
                        { 
                            price = random.Next(5, 10);
                            type = 2; 
                        }
                        else
                        {
                            if (type < 400) 
                            { 
                                price = random.Next(10, 30);
                                type = 3; 
                            }
                            else
                            {
                                if (type < 500) 
                                { 
                                    price = random.Next(10, 30);
                                    type = 4; 
                                }
                                else
                                {
                                    price = random.Next(30, 100);
                                    type = (type % 6) + 5;
                                }
                            }
                        }
                    } 
                

                    int numberOfOrganizedTimes = random.Next(1, 8), monthsSubstracted = 0;
                    int seasonsInYear = random.Next(3);
                    int offset = random.Next((9 - numberOfOrganizedTimes) * 366);
                    datePom = DateTime.Now;
                    datePom = datePom.AddDays(-offset);
                    // months susbtract:
                        switch(seasonsInYear)
                        {
                            case 0: 
                                monthsSubstracted = 3;
                                numberOfOrganizedTimes *= random.Next(5); 
                                break; 
                            case 1: 
                                monthsSubstracted = 6;
                                numberOfOrganizedTimes = random.Next(3); 
                                break; 
                            default:  
                                monthsSubstracted = 12; 
                                break; 
                        }

                    
                    duration = random.Next(7, 90);
                    int maxNumberOfContracts  = random.Next(5, 30), numberOfPeople = random.Next(1, 5);

                    string locationFrom = locations[random.Next(locations.Length)];
                    string locationTo = locations[random.Next(locations.Length)];
                    string newSeviceName = "gen";
                    for(int z = 0; z < numberOfOrganizedTimes; z++)
                    {
                        DateTime newServiceDateFrom = datePom;
                        DateTime newServiceDateTo = datePom.AddDays(duration);

                        ServiceSeed newService = new ServiceSeed{
                            TravelAgencyId = i,
                            Name = newSeviceName,
                            TypeId = type,
                            LocationFrom = locationFrom,
                            LocationTo = locationTo,
                            DateTo = newServiceDateTo,
                            DateFrom = newServiceDateFrom,
                            Price = price.ToString(),
                            Currency = "eur",
                            About = "gen",
                            MaxNumberOfContracts = maxNumberOfContracts,
                            NumberOfPeople = numberOfPeople,
                            IsArchived = 0,
                            IsLimited = 0,
                            HourFrom = new TimeSpan(),
                            HourTo = new TimeSpan()
                            //Images = 1
                        };
                        genServices.Add(newService);
                        serviceSum++;
                        //ServiceSeed.MapSeedToService(context, newService);

                        // generate contracts:
                        int numberOfContacts = random.Next(-20, newService.MaxNumberOfContracts);
                        
                        shuffleArray = shuffle(shuffleArray);
                        for (int g = 1; g <= numberOfContacts; g++)
                        {
                            int contractBeginOffset = random.Next(duration - 1);
                            int contractEndOffset = random.Next(1, (duration - contractBeginOffset));

                            DateTime contractDateFrom = ((DateTime)datePom).AddDays(contractBeginOffset);
                            DateTime contractDateTo = ((DateTime)contractDateFrom).AddDays(contractEndOffset);
                            if(contractDateFrom > contractDateTo)
                            {
                                File.WriteAllText("E:/SuperTourist/ErrorInfo.txt", "error");
                                return;
                            }
                            DateTime dateOfCreation = new DateTime();
                            try
                            {
                                dateOfCreation = contractDateFrom.AddDays(-(random.Next(366) + 1));
                            }
                            catch(Exception e)
                            {
                                File.WriteAllText("E:/SuperTourist/ErrorInfo.txt", contractDateFrom.ToLongDateString());
                            }
                            int numberOfTourists = (random.NextDouble() > 0.8 ? random.Next(2, 5) : 1) * newService.NumberOfPeople;
                            DateTime dateToNew = contractDateTo > newServiceDateTo ? newServiceDateTo : contractDateTo;

                            int paid = random.Next(100) > 2 ? 1 : 0;
                            int cancelReq = random.Next(100) < 8 ? 1 : 0;
                            int cancel = (random.Next(100) < 4 || paid == 0) ? 1 : 0;
                            ContractSeed newContract = new ContractSeed{
                                TouristId = shuffleArray[g - 1],
                                ServiceId = serviceSum,
                                DateFrom = contractDateFrom, //.ToString("yyyy-MM-dd HH:mm:ss")
                                DateTo = dateToNew,
                                DateCreated = dateOfCreation,
                                HourFrom = new TimeSpan(),
                                HourTo = new TimeSpan(),
                                NumberOfTourists = numberOfTourists,
                                PassportNumber = "a",
                                Price = (numberOfTourists * price * (decimal)((dateToNew - contractDateFrom).TotalDays)).ToString(),
                                IsPaid = paid,
                                IsArchived = 0,
                                CancelRequested = cancelReq,
                                ContractCanceled = cancel
                            };
                            genContracts.Add(newContract);

                            if(newContract.TouristId > 100 || decimal.Parse(newContract.Price) > 1000000)
                            {
                                File.WriteAllText("E:/SuperTourist/ErrorInfo.txt", "error");
                                return;
                            }
                            
                            contractNum++;
                            if (random.Next(100) < 25)
                            {
                                // create review

                                ReviewSeed newReview = new ReviewSeed{
                                    Tourist = newContract.TouristId,
                                    Agency = i,
                                    Contract = contractNum,
                                    TextContent = "a",
                                    Grade = random.Next(1, 5),
                                    DateTimeAdded = dateToNew.AddDays(random.Next(1, 7)),
                                    ReviewImageUrl = "a"
                                };

                                genReviews.Add(newReview);
                            }
                        }

                        // datePom = datePom.AddMonths(-monthsSubstracted);
                        try
                        {
                        datePom = datePom.AddMonths(-monthsSubstracted);
                        }
                        catch(Exception e)
                        {
                            File.WriteAllText("E:/SuperTourist/ErrorInfo.txt", datePom.ToLongDateString());
                        }
                    }
                }
            }

            // write in JSON
            JsonSerializer serializer = new JsonSerializer();
            using(StreamWriter stream = File.CreateText("E:/SuperTourist/ServiceSeed.json"))
            {
                serializer.Serialize(stream, genServices);
            }

            using(StreamWriter stream = File.CreateText("E:/SuperTourist/ContractSeed.json"))
            {
                serializer.Serialize(stream, genContracts);
            }

            using(StreamWriter stream = File.CreateText("E:/SuperTourist/ReviewSeed.json"))
            {
                serializer.Serialize(stream, genReviews);
            }
        }

        private static int[] shuffle(int[] shuffleArray)
        {
            int n = shuffleArray.Length;
            int changeIndex, pom;
            Random random = new Random();

            for (int i = 0; i < n; i++)
            {
                changeIndex = random.Next(i + 1);

                pom = shuffleArray[i];
                shuffleArray[i] = shuffleArray[changeIndex];
                shuffleArray[changeIndex] = pom;
            }

            return shuffleArray;
        }
    }
}
