using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using UP_Murtazin.Pages;
using UP_Murtazin.Models;

namespace UP_Murtazin.Tests
{
    public class MachineTests
    {
        // Тест 1: Проверка создания модели автомата
        [Fact]
        public void CreateMachineModel_ValidData_PropertiesSetCorrectly()
        {
            var machine = new MachineDisplayModel
            {
                Id = 903823,
                Name = "БД «Московский»",
                Model = "Sisco Cristallo 400",
                CompanyName = "ООО Торговые Автоматы",
                SerialNumber = "1824100025",
                Location = "Суммарно 127 у входа",
                InstallDate = "12.05.2018",
                IsBlocked = false
            };

            int actualId = machine.Id;
            string actualName = machine.Name;
            string actualModel = machine.Model;
            string actualCompany = machine.CompanyName;
            string actualSerial = machine.SerialNumber;
            string actualLocation = machine.Location;
            string actualDate = machine.InstallDate;
            bool actualBlocked = machine.IsBlocked;

            Assert.Equal(903823, actualId);
            Assert.Equal("БД «Московский»", actualName);
            Assert.Equal("Sisco Cristallo 400", actualModel);
            Assert.Equal("ООО Торговые Автоматы", actualCompany);
            Assert.Equal("1824100025", actualSerial);
            Assert.Equal("Суммарно 127 у входа", actualLocation);
            Assert.Equal("12.05.2018", actualDate);
            Assert.False(actualBlocked);
        }

        // Тест 2: Проверка блокировки и разблокировки автомата
        [Fact]
        public void MachineBlocking_BlockAndUnblock_IsBlockedChangesCorrectly()
        {
            // Arrange
            var machine = new MachineDisplayModel
            {
                Id = 903828,
                Name = "ПТ «Магнит»",
                IsBlocked = false
            };

            // Act - блокируем
            machine.IsBlocked = true;
            bool afterBlock = machine.IsBlocked;

            // Act - разблокируем
            machine.IsBlocked = false;
            bool afterUnblock = machine.IsBlocked;

            // Assert
            Assert.True(afterBlock);
            Assert.False(afterUnblock);
        }

        // Тест 3: Проверка фильтрации списка автоматов по названию
        [Fact]
        public void FilterMachinesByName_ReturnsOnlyMatchingMachines()
        {
            // Arrange
            var machines = new List<MachineDisplayModel>
            {
                new MachineDisplayModel { Id = 1, Name = "БД Московский" },
                new MachineDisplayModel { Id = 2, Name = "ПТ Магнит" },
                new MachineDisplayModel { Id = 3, Name = "Завод Тайфун" },
                new MachineDisplayModel { Id = 4, Name = "Рынок Центральный" }
            };

            string filterText = "магнит";

            // Act
            var filtered = machines.Where(m =>
                m.Name.ToLower().Contains(filterText.ToLower())).ToList();

            // Assert
            Assert.Single(filtered);
            Assert.Equal(2, filtered.First().Id);
            Assert.Equal("ПТ Магнит", filtered.First().Name);
        }

        // Тест 4: Проверка пагинации (разбивка на страницы)
        [Fact]
        public void Pagination_SkipAndTake_ReturnsCorrectPage()
        {
            // Arrange - создаем 25 автоматов
            var machines = new List<MachineDisplayModel>();
            for (int i = 1; i <= 25; i++)
            {
                machines.Add(new MachineDisplayModel
                {
                    Id = i,
                    Name = $"Автомат {i}"
                });
            }

            int pageSize = 10;
            int currentPage = 2;

            // Act - берем вторую страницу (записи с 11 по 20)
            var pageData = machines
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Assert
            Assert.Equal(10, pageData.Count);
            Assert.Equal(11, pageData.First().Id);
            Assert.Equal(20, pageData.Last().Id);
        }

        // Тест 5: Проверка расчета процента работающих автоматов
        [Fact]
        public void CalculateWorkingPercent_ReturnsCorrectPercentage()
        {
            // Arrange
            var machines = new List<MachineDisplayModel>
            {
                new MachineDisplayModel { Id = 1, IsBlocked = false },
                new MachineDisplayModel { Id = 2, IsBlocked = false },
                new MachineDisplayModel { Id = 3, IsBlocked = true },
                new MachineDisplayModel { Id = 4, IsBlocked = false },
                new MachineDisplayModel { Id = 5, IsBlocked = true }
            };

            int total = machines.Count;
            int working = machines.Count(m => !m.IsBlocked);
            double percent = (double)working / total * 100;

            // Act
            int expectedWorking = 3;
            double expectedPercent = 60.0;

            // Assert
            Assert.Equal(3, working);
            Assert.Equal(60.0, percent);
            Assert.Equal(expectedWorking, working);
            Assert.Equal(expectedPercent, percent);
        }
    }
}