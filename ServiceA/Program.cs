using Polly;
using Polly.Extensions.Http;
using System.Diagnostics;

namespace ResilienceDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Process.GetCurrentProcess().Id);
            var builder = WebApplication.CreateBuilder(args);

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

            var retryPolicy = Policy.Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(response => false)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                });

            var timeoutPolicy = Policy.TimeoutAsync(5, (context, timespan, action) =>
            {
                Console.WriteLine($"Timed out after {timespan.Seconds}");
                return Task.CompletedTask;
            });

            var bulkheadPolicy = Policy.BulkheadAsync(1);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient("")
                //.AddPolicyHandler(timeoutPolicy.AsAsyncPolicy<HttpResponseMessage>());
                //.AddPolicyHandler(bulkheadPolicy.AsAsyncPolicy<HttpResponseMessage>());
                .AddPolicyHandler(circuitBreakerPolicy);
                //.AddPolicyHandler(retryPolicy);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}