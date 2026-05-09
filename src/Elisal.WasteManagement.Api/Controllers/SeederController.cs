using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Elisal.WasteManagement.Application.Services;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")] // Descomente em produção para restringir
public class SeederController : ControllerBase
{
    private readonly ElisalDbContext _context;

    public SeederController(ElisalDbContext context)
    {
        _context = context;
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunSeeder()
    {
        try
        {
            // 1. Limpar dados existentes (mantendo Admin e Tipos de Resíduos)
            await CleanDatabaseAsync();

            // 2. Configurar locale do Bogus para Português
            Randomizer.Seed = new Random(2026);
            var faker = new Faker("pt_PT"); // Pt_PT é o mais próximo de Angola no Bogus

            // 3. Gerar Utilizadores
            var (users, userPasswords) = await SeedUsersAsync(faker);

            // 4. Gerar Pontos de Recolha (Foco em Viana)
            var collectionPoints = await SeedCollectionPointsAsync(faker);

            // 5. Gerar Centros de Triagem
            var sortingCenters = await SeedSortingCentersAsync(faker);

            // 6. Gerar Rotas e associar pontos
            var routes = await SeedRoutesAsync(faker, collectionPoints, users.Where(u => u.Role == UserRole.Driver).ToList());

            // 7. Gerar Registos Retroativos (Últimos 60 dias)
            await SeedOperationsAsync(faker, routes, sortingCenters, collectionPoints);

            return Ok(new
            {
                Message = "Base de dados populada com sucesso! Dados gerados para os últimos 60 dias.",
                GeneratedUsers = userPasswords
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    private async Task CleanDatabaseAsync()
    {
        // Delete order is important due to foreign keys
        _context.RoutePointExecutionStatuses.RemoveRange(_context.RoutePointExecutionStatuses);
        _context.RouteExecutions.RemoveRange(_context.RouteExecutions);
        _context.RoutePoints.RemoveRange(_context.RoutePoints);
        _context.Routes.RemoveRange(_context.Routes);
        
        _context.WasteReceptions.RemoveRange(_context.WasteReceptions);
        _context.SortingBatchOutputs.RemoveRange(_context.SortingBatchOutputs);
        _context.SortingBatches.RemoveRange(_context.SortingBatches);
        _context.SortingCenters.RemoveRange(_context.SortingCenters);

        _context.CollectionRecordWasteTypes.RemoveRange(_context.CollectionRecordWasteTypes);
        _context.CollectionRecords.RemoveRange(_context.CollectionRecords);
        _context.CollectionPoints.RemoveRange(_context.CollectionPoints);

        var nonAdminUsers = await _context.Users.Where(u => u.Role != UserRole.Admin).ToListAsync();
        _context.Users.RemoveRange(nonAdminUsers);

        await _context.SaveChangesAsync();
    }

    private async Task<(List<User>, List<object>)> SeedUsersAsync(Faker faker)
    {
        var users = new List<User>();
        var userPasswords = new List<object>();

        var nomesAngolanos = new[] { "João", "José", "Manuel", "António", "Domingos", "Paulo", "Pedro", "Carlos", "Fernando", "Francisco", "Ndongala", "Ngola", "Kiluanje", "Agostinho" };
        var apelidosAngolanos = new[] { "Silva", "Santos", "Costa", "Oliveira", "Martins", "Fernandes", "Gomes", "Lopes", "Nunes", "Mateus", "Kassoma", "Neto", "Dos Santos", "Macau" };

        for (int i = 0; i < 10; i++)
        {
            var nome = faker.PickRandom(nomesAngolanos);
            var apelido = faker.PickRandom(apelidosAngolanos);
            var fullName = $"{nome} {apelido}";
            var email = $"{nome.ToLower()}.{apelido.ToLower()}@elisal.co.ao";
            var senhaRaw = "Senha@123";
            var role = i < 3 ? UserRole.Manager : UserRole.Driver;

            var user = new User
            {
                Name = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(senhaRaw, 12),
                Role = role,
                IsActive = true,
                CreatedAt = faker.Date.Past(1)
            };

            users.Add(user);
            userPasswords.Add(new { Name = fullName, Email = email, Role = role.ToString(), Password = senhaRaw });
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        return (users, userPasswords);
    }

    private async Task<List<CollectionPoint>> SeedCollectionPointsAsync(Faker faker)
    {
        var points = new List<CollectionPoint>();
        
        // Viana tem maior destaque
        var bairrosViana = new[] { "Zango 1", "Zango 2", "Zango 3", "Vila Sede", "Kapalanga", "Luanda Sul", "Estalagem", "Km 30" };
        var bairrosLuanda = new[] { "Talatona", "Mutamba", "Cacuaco", "Kilamba", "Cazenga", "Samba", "Maculusso" };

        for (int i = 0; i < 20; i++)
        {
            // 70% Viana, 30% outros
            bool isViana = faker.Random.Int(1, 100) <= 70;
            var bairro = isViana ? faker.PickRandom(bairrosViana) : faker.PickRandom(bairrosLuanda);
            var municipio = isViana ? "Viana" : "Luanda";

            var point = new CollectionPoint
            {
                Name = $"Ecoponto - {bairro} {(i + 1)}",
                Latitude = faker.Address.Latitude(-8.8, -8.9),
                Longitude = faker.Address.Longitude(13.2, 13.5), // Coordenadas aproximadas de Luanda
                Address = $"Rua Principal, {bairro}, {municipio}",
                Capacity = faker.Random.Int(100, 1000),
                CurrentOccupancy = faker.Random.Int(0, 100),
                Municipality = municipio
            };
            points.Add(point);
        }

        await _context.CollectionPoints.AddRangeAsync(points);
        await _context.SaveChangesAsync();

        return points;
    }

    private async Task<List<SortingCenter>> SeedSortingCentersAsync(Faker faker)
    {
        var centers = new List<SortingCenter>();
        var nomesCentros = new[] { "Centro de Triagem de Viana", "Eco-Centro Cacuaco", "Central de Processamento Kilamba" };

        foreach (var nome in nomesCentros)
        {
            centers.Add(new SortingCenter
            {
                Name = nome,
                Municipality = nome.Contains("Viana") ? "Viana" : "Luanda",
                CapacityTonsPerDay = faker.Random.Double(50, 200),
                Address = "Zona Industrial"
            });
        }

        await _context.SortingCenters.AddRangeAsync(centers);
        await _context.SaveChangesAsync();

        return centers;
    }

    private async Task<List<Elisal.WasteManagement.Domain.Entities.Route>> SeedRoutesAsync(Faker faker, List<CollectionPoint> allPoints, List<User> drivers)
    {
        var routes = new List<Elisal.WasteManagement.Domain.Entities.Route>();

        if (!drivers.Any()) return routes;

        for (int i = 0; i < 5; i++)
        {
            var driver = faker.PickRandom(drivers);
            var route = new Elisal.WasteManagement.Domain.Entities.Route
            {
                Name = $"Rota {(i + 1)} - {(i % 2 == 0 ? "Viana Sul" : "Luanda Centro")}",
                AssignedDriverId = driver.Id,
                WeekDay = faker.PickRandom(new[] { "Segunda", "Terça", "Quarta", "Quinta", "Sexta" }),
                StartTime = new TimeSpan(faker.Random.Int(6, 14), 0, 0)
            };
            
            routes.Add(route);
        }

        await _context.Routes.AddRangeAsync(routes);
        await _context.SaveChangesAsync();

        // Adicionar RoutePoints
        foreach (var route in routes)
        {
            // Atribuir 3 a 6 pontos por rota
            var numPoints = faker.Random.Int(3, 6);
            var routePointsForRoute = faker.PickRandom(allPoints, numPoints).ToList();
            
            for (int order = 0; order < routePointsForRoute.Count; order++)
            {
                var rp = new RoutePoint
                {
                    RouteId = route.Id,
                    CollectionPointId = routePointsForRoute[order].Id,
                    SequenceOrder = order + 1
                };
                _context.RoutePoints.Add(rp);
            }
        }
        await _context.SaveChangesAsync();

        return routes;
    }

    private async Task SeedOperationsAsync(Faker faker, List<Elisal.WasteManagement.Domain.Entities.Route> routes, List<SortingCenter> sortingCenters, List<CollectionPoint> collectionPoints)
    {
        // Precisamos de WasteTypes para associar
        var wasteTypes = await _context.WasteTypes.ToListAsync();
        if (!wasteTypes.Any())
        {
            // Seed básico de WasteTypes caso não existam
            wasteTypes = new List<WasteType>
            {
                new WasteType { Name = "Plástico", Description = "Reciclável", IsRecyclable = true, ColorCode = "Yellow" },
                new WasteType { Name = "Papel/Cartão", Description = "Reciclável", IsRecyclable = true, ColorCode = "Blue" },
                new WasteType { Name = "Vidro", Description = "Reciclável", IsRecyclable = true, ColorCode = "Green" },
                new WasteType { Name = "Indiferenciado", Description = "Orgânico", IsRecyclable = false, ColorCode = "Black" }
            };
            await _context.WasteTypes.AddRangeAsync(wasteTypes);
            await _context.SaveChangesAsync();
        }

        var startDate = DateTime.UtcNow.AddDays(-60); // Últimos 60 dias

        for (int i = 0; i < 60; i++) // Uma execução por dia aproximadamente
        {
            var date = startDate.AddDays(i).AddHours(faker.Random.Int(6, 14)); // Execuções de manhã/tarde

            if (routes.Any())
            {
                var route = faker.PickRandom(routes);
                
                var execution = new RouteExecution
                {
                    RouteId = route.Id,
                    DriverId = route.AssignedDriverId ?? 1,
                    StartTime = date,
                    EndTime = date.AddMinutes(120),
                    Status = RouteExecutionStatus.Completed
                };
                _context.RouteExecutions.Add(execution);
                await _context.SaveChangesAsync();

                // Registar recolhas para esta execução
                var routePoints = await _context.RoutePoints.Where(rp => rp.RouteId == route.Id).ToListAsync();
                foreach (var rp in routePoints)
                {
                    var record = new CollectionRecord
                    {
                        CollectionPointId = rp.CollectionPointId,
                        DateTime = date.AddMinutes(rp.SequenceOrder * 30),
                        UserId = route.AssignedDriverId ?? 1,
                        AmountKg = faker.Random.Double(100, 500)
                    };
                    _context.CollectionRecords.Add(record);
                }
            }

            // Recepções nos Centros de Triagem
            if (sortingCenters.Any() && faker.Random.Bool(0.7f)) // 70% de chance de ter uma recepção naquele dia
            {
                var center = faker.PickRandom(sortingCenters);
                var route = faker.PickRandom(routes);
                
                var grossWeight = faker.Random.Double(500, 2000);
                var tareWeight = faker.Random.Double(100, 400);

                var reception = new WasteReception
                {
                    SortingCenterId = center.Id,
                    DateTime = date.AddHours(4),
                    GrossWeightKg = grossWeight,
                    TareWeightKg = tareWeight,
                    NetWeightKgStored = grossWeight - tareWeight,
                    ReceivedByUserId = 1, // Admin (ou outro)
                    Notes = "Recolha de rotina de " + (faker.Random.Bool() ? "Viana" : "Luanda")
                };
                _context.WasteReceptions.Add(reception);
            }
        }

        await _context.SaveChangesAsync();
    }
}
