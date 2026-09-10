// Provides JWT bearer authentication support.
using Microsoft.AspNetCore.Authentication.JwtBearer;
// Provides OpenAPI model types used to describe OAuth in Swagger.
using Microsoft.OpenApi.Models;

// Creates the application builder and loads configuration and hosting services.
var builder = WebApplication.CreateBuilder(args);

// Registers attribute-routed API controllers.
builder.Services.AddControllers();
// Registers API metadata needed by Swagger.
builder.Services.AddEndpointsApiExplorer();

// Registers JWT bearer authentication as the default authentication scheme.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Uses the OAuth provider's authority to discover signing keys and validate tokens.
        options.Authority = builder.Configuration["OAuth:Authority"];

        // Requires tokens to contain the configured API audience.
        options.Audience = builder.Configuration["OAuth:Audience"];

        // Allows HTTP metadata only during local development; production requires HTTPS.
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });
// Registers Swagger document generation and the interactive Swagger UI configuration.
builder.Services.AddSwaggerGen(options =>
{
    // Adds an OAuth 2.0 security scheme to Swagger's Authorize button.
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        // Identifies this security scheme as OAuth 2.0.
        Type = SecuritySchemeType.OAuth2,
        // Configures the authorization-code flow used by Swagger UI.
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                // URL where the user signs in and grants authorization.
                AuthorizationUrl = new Uri(builder.Configuration["OAuth:AuthorizationUrl"]!),
                // URL where Swagger exchanges the authorization code for a token.
                TokenUrl = new Uri(builder.Configuration["OAuth:TokenUrl"]!),
                // Defines the OAuth scope requested during sign-in.
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "Sign in and read your identity"
                }
            }
        }
    });
    // Marks API operations as requiring the OAuth security scheme in Swagger.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            // References the OAuth scheme defined above.
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    // Identifies this reference as a security scheme reference.
                    Type = ReferenceType.SecurityScheme,
                    // Matches the security definition name "oauth2".
                    Id = "oauth2"
                }
            },
            // Applies the scheme without requiring a specific scope for every operation.
            Array.Empty<string>()
        }
    });
});

// Builds the configured web application.
var app = builder.Build();

// Exposes Swagger only while running in the Development environment.
if (app.Environment.IsDevelopment())
{
    // Serves the generated OpenAPI JSON document.
    app.UseSwagger();
    // Serves the interactive Swagger UI.
    app.UseSwaggerUI(options =>
    {
        // Identifies the Entra application registration used by Swagger UI.
        options.OAuthClientId(builder.Configuration["OAuth:ClientId"]!);
        // Sends a code verifier so Entra can validate the authorization-code exchange.
        options.OAuthUsePkce();
    });
}

// Reads and validates bearer tokens from incoming requests.
app.UseAuthentication();
// Enforces [Authorize] attributes after authentication has identified the caller.
app.UseAuthorization();

// Maps controller routes such as /WeatherForecast and /api/oauth-test/me.
app.MapControllers();

// Starts listening for HTTP requests.
app.Run();
