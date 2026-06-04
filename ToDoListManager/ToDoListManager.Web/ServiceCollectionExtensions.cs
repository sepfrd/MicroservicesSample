using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;
using ToDoListManager.Business.Businesses;
using ToDoListManager.Business.Contracts;
using ToDoListManager.Common.Dtos;
using ToDoListManager.Common.Helpers;
using ToDoListManager.Common.Validations;
using ToDoListManager.DataAccess;
using ToDoListManager.DataAccess.Context;
using ToDoListManager.DataAccess.Contracts;
using ToDoListManager.ExternalService.RabbitMQService;
using ToDoListManager.ExternalService.RabbitMQService.Contracts;
using ToDoListManager.Model.Entities;

namespace ToDoListManager.Web;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection InjectApi(this IServiceCollection services) =>
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            })
            .Services;

    internal static IServiceCollection InjectCors(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

    internal static IServiceCollection InjectUnitOfWork(this IServiceCollection services) =>
        services.AddScoped<IUnitOfWork, UnitOfWork>();

    internal static IServiceCollection InjectContext(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return services.AddDbContext<ToDoListManagerDbContext>(options => options.UseInMemoryDatabase("ToDoListManager"));
        }

        var sqlServerConfigurationDto = configuration.GetSection("SqlServerConfiguration").Get<SqlServerConfigurationDto>()!;

        var connectionString = string.Format(sqlServerConfigurationDto.ConnectionString, sqlServerConfigurationDto.UserId, sqlServerConfigurationDto.Password);

        return services.AddDbContext<ToDoListManagerDbContext>(options =>
        {
            options
                .UseSqlServer(connectionString)
                .UseSeeding((dbContext, _) => dbContext.SeedDatabase())
                .UseAsyncSeeding((dbContext, _, _) => Task.FromResult(dbContext.SeedDatabase()));

            if (!environment.IsProduction())
            {
                options.EnableSensitiveDataLogging();
            }
        });
    }

    internal static IServiceCollection InjectSerilog(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSerilog(x => x.ReadFrom.Configuration(configuration));

    internal static IServiceCollection InjectAuthentication(this IServiceCollection services) =>
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.Headers.Location = context.RedirectUri;
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.Headers.Location = context.RedirectUri;
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            })
            .Services
            .AddAuthorization();

    internal static IServiceCollection InjectBusinesses(this IServiceCollection services) =>
        services
            .AddScoped<IAuthBusiness, AuthBusiness>()
            .AddScoped<ICategoryBusiness, CategoryBusiness>()
            .AddScoped<IToDoItemBusiness, ToDoItemBusiness>()
            .AddScoped<IToDoListBusiness, ToDoListBusiness>()
            .AddScoped<IUserBusiness, UserBusiness>();

    internal static IServiceCollection InjectFluentValidation(this IServiceCollection services) =>
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<PersonValidator>();

    internal static async Task<IServiceCollection> InjectRabbitMqAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConfigurationDto = configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfigurationDto>()!;

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfigurationDto.Host,
            Port = rabbitMqConfigurationDto.Port
        };

        var rabbitMqConnection = await factory.CreateConnectionAsync();

        await using var rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync();

        await rabbitMqChannel.ExchangeDeclareAsync(rabbitMqConfigurationDto.TriggerExchangeName, type: ExchangeType.Fanout, true);
        await rabbitMqChannel.QueueDeclareAsync(rabbitMqConfigurationDto.TriggerQueueName, true, false, false);

        await rabbitMqChannel.QueueBindAsync(
            rabbitMqConfigurationDto.TriggerQueueName,
            rabbitMqConfigurationDto.TriggerExchangeName,
            rabbitMqConfigurationDto.TriggerQueueName);

        services
            .AddSingleton(rabbitMqConnection)
            .AddScoped<IMessageBusClient, MessageBusClient>();

        return services;
    }

    internal static IServiceCollection InjectGrpc(this IServiceCollection services) =>
        services.AddGrpc(configure => configure.EnableDetailedErrors = true).Services;

    private static DbContext SeedDatabase(this DbContext dbContext)
    {
        if (dbContext.Set<User>().Any())
        {
            return dbContext;
        }

        const int fakePeopleCount = 1_000;
        const int fakeUsersCount = 1_000;
        const int fakeCategoriesCount = 1_000;
        const int fakeToDoListsCount = 1_000;
        const int fakeToDoItemsCount = 1_000;

        var fakePeople = FakeDataHelper.GetFakePeople(fakePeopleCount);

        dbContext.Set<Person>().AddRange(fakePeople);

        dbContext.SaveChanges();

        var fakeUsers = FakeDataHelper.GetFakeUsers(fakePeople, fakeUsersCount);

        dbContext.Set<User>().AddRange(fakeUsers);

        dbContext.SaveChanges();

        var fakeCategories = FakeDataHelper.GetFakeCategories(fakeUsers, fakeCategoriesCount);

        dbContext.Set<Category>().AddRange(fakeCategories);

        dbContext.SaveChanges();

        var fakeToDoLists = FakeDataHelper.GetFakeToDoLists(fakeUsers, fakeToDoListsCount);

        dbContext.Set<ToDoList>().AddRange(fakeToDoLists);

        dbContext.SaveChanges();

        var fakeToDoItems = FakeDataHelper.GetFakeToDoItems(fakeToDoLists, fakeCategories, fakeToDoItemsCount);

        dbContext.Set<ToDoItem>().AddRange(fakeToDoItems);

        dbContext.SaveChanges();

        return dbContext;
    }
}