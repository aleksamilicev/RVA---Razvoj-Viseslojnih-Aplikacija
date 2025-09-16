using RVA.Shared.Models;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Linq;

namespace RVA.Client.Helpers
{
    public static class DataSeeder
    {
        public static void SeedRaftingData(string dataDirectory, int count = 10)
        {
            var random = new Random();
            var locations = GenerateLocations(random, 5);
            var clothings = GenerateClothings(random, 10);
            var equipments = GenerateEquipments(random, 10);

            var raftings = new List<Rafting>();
            for (int i = 1; i <= count; i++)
            {
                var startLocation = locations[random.Next(locations.Count)];
                var endLocation = locations[random.Next(locations.Count)];
                var rafting = new Rafting
                {
                    Id = i,
                    Name = $"Rafting {i}",
                    Description = "Exciting rafting adventure",
                    StartTime = DateTime.Now.AddDays(random.Next(1, 30)),
                    Duration = TimeSpan.FromHours(random.Next(2, 6)),
                    Distance = random.Next(5, 20),
                    ParticipantCount = random.Next(1, 12),
                    MaxParticipants = 12,
                    EndTime = DateTime.Now.AddDays(random.Next(1, 30)).AddHours(random.Next(2, 6)),
                    CurrentIntensity = (Intensity)random.Next(0, Enum.GetValues(typeof(Intensity)).Length),
                    CurrentSpeedKmh = random.NextDouble() * 20,
                    Capacity = 12,
                    CurrentState = (RaftingState)random.Next(0, Enum.GetValues(typeof(RaftingState)).Length),
                    GuideId = random.Next(1, 5),
                    PricePerPerson = (decimal)(random.Next(50, 500)),
                    WeatherConditions = "Sunny",
                    StartLocationId = startLocation.Id,
                    StartLocation = startLocation,
                    EndLocationId = endLocation.Id,
                    EndLocation = endLocation,
                    UsedClothing = clothings.OrderBy(x => random.Next()).Take(2).ToList(),
                    UsedEquipment = equipments.OrderBy(x => random.Next()).Take(2).ToList()
                };
                raftings.Add(rafting);
            }

            // Kreiraj direktorijum ako ne postoji
            Directory.CreateDirectory(dataDirectory);

            // JSON
            var jsonPath = Path.Combine(dataDirectory, "rafting_data.json");
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(raftings, new JsonSerializerOptions { WriteIndented = true }));

            // XML
            var xmlPath = Path.Combine(dataDirectory, "rafting_data.xml");
            var serializer = new XmlSerializer(typeof(List<Rafting>));
            using (var writer = new StreamWriter(xmlPath))
            {
                serializer.Serialize(writer, raftings);
            }

            // CSV
            var csvPath = Path.Combine(dataDirectory, "rafting_data.csv");
            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            }))
            {
                // Flatten data for CSV
                csv.WriteRecords(raftings.Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    StartTime = r.StartTime.ToString("o"),
                    r.Duration,
                    r.Distance,
                    r.ParticipantCount,
                    r.MaxParticipants,
                    EndTime = r.EndTime.ToString("o"),
                    r.CurrentIntensity,
                    r.CurrentSpeedKmh,
                    r.Capacity,
                    r.CurrentState,
                    r.GuideId,
                    r.PricePerPerson,
                    r.WeatherConditions,
                    StartLocation = r.StartLocation.Name,
                    EndLocation = r.EndLocation.Name,
                    ClothingIds = string.Join(";", r.UsedClothing.Select(c => c.Id)),
                    EquipmentIds = string.Join(";", r.UsedEquipment.Select(e => e.Id))
                }));
            }
        }

        private static List<Location> GenerateLocations(Random random, int count)
        {
            var list = new List<Location>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new Location
                {
                    Id = i,
                    Name = $"Location {i}",
                    River = $"River {i}",
                    Latitude = 40 + random.NextDouble() * 10,
                    Longitude = 20 + random.NextDouble() * 10,
                    HasParking = random.Next(0, 2) == 1,
                    HasFacilities = random.Next(0, 2) == 1,
                    Description = $"Beautiful place {i}",
                    CreatedDate = DateTime.Now
                });
            }
            return list;
        }

        private static List<Clothing> GenerateClothings(Random random, int count)
        {
            var list = new List<Clothing>();
            var types = Enum.GetValues(typeof(ClothingType));
            var materials = Enum.GetValues(typeof(Material));
            for (int i = 1; i <= count; i++)
            {
                list.Add(new Clothing
                {
                    Id = i,
                    Name = $"Clothing {i}",
                    Type = (ClothingType)types.GetValue(random.Next(types.Length)),
                    Material = (Material)materials.GetValue(random.Next(materials.Length)),
                    Size = random.Next(36, 50),
                    Color = "Red",
                    Brand = "Generic",
                    IsWaterproof = random.Next(0, 2) == 1,
                    LastCleaned = DateTime.Now.AddDays(-random.Next(1, 30)),
                    Condition = "Good",
                    IsAvailable = true
                });
            }
            return list;
        }

        private static List<Equipment> GenerateEquipments(Random random, int count)
        {
            var list = new List<Equipment>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new Equipment
                {
                    Id = i,
                    Name = $"Equipment {i}",
                    Type = "Standard",
                    SerialNumber = $"SN-{random.Next(1000, 9999)}",
                    Condition = "Good",
                    IsAvailable = true,
                    LastMaintenance = DateTime.Now.AddDays(-random.Next(1, 100))
                });
            }
            return list;
        }
    }
}
