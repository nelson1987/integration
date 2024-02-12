using AutoMapper;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace CourseTDD.Api;

public class UserController : Controller
{
    private readonly IMongoCollection<User> _users;
    private readonly IRepository<User> _userRepository;
    private readonly IProducer<string, string> _producer;

    public UserController(IMongoCollection<User> users, IRepository<User> userRepository, IProducer<string, string> producer)
    {
        _users = users;
        _userRepository = userRepository;
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var user = new User
        {
            Name = command.Name,
            Email = command.Email
        };

        await _userRepository.AddAsync(user);

        var message = new UserCreatedEvent
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        var serializedMessage = System.Text.Json.JsonSerializer.Serialize(message);

        await _producer.ProduceAsync("user-created", serializedMessage);

        return Ok();
    }
}

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class CreateUserCommand
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class UserCreatedEvent
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public interface IRepository<T>
{
    Task AddAsync(T entity);
}

public class Repository<T> : IRepository<T>
{
    private readonly DbContext _context;

    public Repository(DbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }
}

public class KafkaProducer : IProducer<string, string>
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public Handle Handle => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public void AbortTransaction(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void AbortTransaction()
    {
        throw new NotImplementedException();
    }

    public int AddBrokers(string brokers)
    {
        throw new NotImplementedException();
    }

    public void BeginTransaction()
    {
        throw new NotImplementedException();
    }

    public void CommitTransaction(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void CommitTransaction()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public int Flush(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Flush(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void InitTransactions(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public int Poll(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Produce(string topic, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryHandler = null)
    {
        throw new NotImplementedException();
    }

    public void Produce(TopicPartition topicPartition, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryHandler = null)
    {
        throw new NotImplementedException();
    }

    public async Task ProduceAsync(string topic, string message)
    {
        await _producer.ProduceAsync(topic, new Message<string, string>(message));
    }

    public Task<DeliveryResult<string, string>> ProduceAsync(string topic, Message<string, string> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeliveryResult<string, string>> ProduceAsync(TopicPartition topicPartition, Message<string, string> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void SendOffsetsToTransaction(IEnumerable<TopicPartitionOffset> offsets, IConsumerGroupMetadata groupMetadata, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void SetSaslCredentials(string username, string password)
    {
        throw new NotImplementedException();
    }
}

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<User, UserCreatedEvent>();
    }
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });

        services.AddMongoClient(Configuration.GetConnectionString("MongoDb"));
        services.AddDbContext<MyContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PostgreSql")));
        services.AddKafkaProducer<string, string>(Configuration.GetSection("Kafka"));
        services.AddAutoMapper(typeof(Startup));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
