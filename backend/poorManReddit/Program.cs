using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

using poorManReddit;
using poorManReddit.DataModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow requests from specific origins
// Add Services: Add services to the container, including Swagger (just above on line 9) for API documentation and CORS for cross-origin requests.
// CORS Configuration: Configure CORS to allow requests from http://127.0.0.1 with any header and method.

builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowAllOrigins",
    builder =>
    {
        builder.WithOrigins("http://127.0.0.1")
        .AllowCredentials()
        .AllowAnyHeader()
        .SetIsOriginAllowed((host) => true)
        .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline for development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS using the defined policy
// Above we defined a CORS policy named "AllowAllOrigins" that allows requests from http://127.0.0.1
// We are now using this policy to enable CORS in the application.
app.UseCors("AllowAllOrigins");


app.MapPost("/newUser", (PublicUsers user) => {
    try{
        using(UsersContext db = new UsersContext())
        {
            db.users.Add(user.GetPrivateUser());
            db.SaveChanges();
            db.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
        }
    }
    catch(Exception e)
    {
        return Results.BadRequest(e.Message);
    }
    return Results.Ok("User created successfully");
}).WithName("Create New User").WithOpenApi();

app.MapPost("/login", (PublicUsers user) => {

    using(var db = new UsersContext()){
        var pu = db.users.FirstOrDefault(u => u.Username == user.userName);
        if (pu != null){
            return Results.Ok(pu);
        }
        else{
            return Results.Unauthorized();
        }
    }

}).WithName("Login User").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() {AuthenticationSchemes="BasicAuthentication"});
// Define an endpoint to get weather forecast data
app.MapGet("/Reset", () =>
{
    using(DbContext reddit = new redditContext())
    {
        reddit.Database.EnsureDeleted();
        reddit.Database.EnsureCreated();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        using (StreamReader r = new StreamReader("dummyData.json"))  
        {  
            string? json = r.ReadToEnd();  
            List<Topic>? source = JsonSerializer.Deserialize<List<Topic>>(json, options); 
            foreach(var item in source)
            {
                reddit.Add<Topic>(item);  
            }
            
        }  
        reddit.SaveChanges();

        using (StreamReader r = new StreamReader("commentData.json"))  
        {  
            string? json = r.ReadToEnd();  
            List<Comments>? source = JsonSerializer.Deserialize<List<Comments>>(json, options); 
            foreach(var item in source)
            {
                reddit.Add<Comments>(item);  
            }

        }  
        reddit.SaveChanges();
        reddit.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
        
    }

})
.WithName("Reset Data")
.WithOpenApi();

// Define an endpoint to get topics from a JSON file
app.MapGet("/getTopics", () =>
{
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    //using (StreamReader r = new StreamReader("dummyData.json"))  
    using(redditContext reddit = new redditContext())
    {  
        List<Topic>? source = reddit.Topics.ToList();//OrderBy(t => t.Id).ToList();//JsonSerializer.Deserialize<List<Topic>>(json, options); 
        return source;
    }  
}).WithOpenApi().WithName("Get Topics");

// Define an endpoint to get comments from a JSON file
app.MapGet("/getComments", () => {
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    using(redditContext reddit = new redditContext())//using (StreamReader r = new StreamReader("commentData.json"))  
    {  
        
        List<Comments>? source = reddit.Comments.OrderBy(t => t.Comment_id).ToList();
        return source;
    }  
}).WithOpenApi().WithName("Get Comments");

// Define a simple endpoint to return a "Hello World" message
app.MapGet("/hellWorld", () =>
{
    return "Hello World from my API";
}).WithOpenApi().WithName("Hello World");

app.MapPost("/addTopic", (Topic topic) =>
{
    using(redditContext reddit = new redditContext())
    {
        reddit.Add<Topic>(topic);
        reddit.SaveChanges();
        return topic;
    }
}).WithOpenApi().WithName("Add Topic");

app.MapPost("/addComment", (Comments comment) =>
{
    using(redditContext reddit = new redditContext())
    {
        reddit.Add<Comments>(comment);
        reddit.SaveChanges();
        return comment;
    }
}).WithOpenApi().WithName("Add Comment");

app.MapPut("/updateTopic", (Topic topic) =>
{
    using(redditContext reddit = new redditContext())
    {
        Topic? update = reddit.Topics.Where(t => t.Topic_id == topic.Topic_id).FirstOrDefault();
        if(update != null){
            update.Rating = topic.Rating;
            reddit.Update<Topic>(update);
            reddit.SaveChanges();
        }
        return topic;
    }
}).WithOpenApi().WithName("Update Topic");

app.MapPut("/updateComment", (Comments comment) =>
{
    using(redditContext reddit = new redditContext())
    {
        Comments? c = reddit.Comments.Where(t => t.Comment_id == comment.Comment_id).FirstOrDefault();
        if(c != null)
        {
            c.Rating = comment.Rating;
            reddit.Update<Comments>(c);
            reddit.SaveChanges();
        }
        return c;
    }
}).WithOpenApi().WithName("Update Comment");

// Run the application
app.Run();

// Record type to represent weather forecast data
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    // Property to calculate temperature in Fahrenheit
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}