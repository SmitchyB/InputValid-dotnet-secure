var builder = WebApplication.CreateBuilder(args); // Create a new web application builder

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Model Validation ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("--- AUTOMATIC MVC VALIDATION TRIGGERED (Fallback) ---");
            foreach (var state in context.ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    logger.LogWarning($"- Member: '{state.Key}', Error: '{error.ErrorMessage}'");
                }
            }
            logger.LogWarning("-----------------------------------------------------");
            return new BadRequestObjectResult(problemDetails);
        };
    });


// --- CORS Configuration ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000") // React dev server default port
                       .AllowAnyHeader()
                       .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- CORS Middleware ---
app.UseCors("AllowFrontend");


// =======================================================
// SECURE Sign-Up Endpoint (Full EXPLICIT MANUAL VALIDATION WITH COMPARATIVE LOGGING)
// =======================================================
app.MapPost("/signup", ([FromBody] SignUpRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation("--- RECEIVED SIGN-UP DATA FOR /signup ENDPOINT ---");
    logger.LogInformation($"Username: \"{request.Username}\" (Is Null: {request.Username is null}, Length: {request.Username?.Length ?? -1})");
    logger.LogInformation($"Email: \"{request.Email}\" (Is Null: {request.Email is null}, Length: {request.Email?.Length ?? -1})");
    logger.LogInformation($"Phone Number: \"{request.PhoneNumber}\" (Is Null: {request.PhoneNumber is null}, Length: {request.PhoneNumber?.Length ?? -1})");
    logger.LogInformation($"Password: \"{request.Password}\" (Is Null: {request.Password is null}, Length: {request.Password?.Length ?? -1})");
    logger.LogInformation($"Confirm Password: \"{request.ConfirmPassword}\" (Is Null: {request.ConfirmPassword is null}, Length: {request.ConfirmPassword?.Length ?? -1})");
    logger.LogInformation("--------------------------------------------------");

    var combinedErrors = new Dictionary<string, List<string>>(); // Collects errors from both methods

    // --- (A) VALIDATION ATTEMPT USING Data Annotations (via Validator.TryValidateObject) ---
    logger.LogInformation("--- Attempting Data Annotations Validation (via Validator.TryValidateObject) ---");
    var dataAnnotationsResults = new List<ValidationResult>();
    var dataAnnotationsContext = new ValidationContext(request, serviceProvider: app.Services, items: null);
    bool isDataAnnotationsValid = Validator.TryValidateObject(request, dataAnnotationsContext, dataAnnotationsResults, validateAllProperties: true);

    logger.LogInformation($"Result (DataAnnotations): isValid = {isDataAnnotationsValid}");
    logger.LogInformation($"Number of DataAnnotations ValidationResults captured: {dataAnnotationsResults.Count}");

    if (!isDataAnnotationsValid)
    {
        logger.LogWarning("--- Data Annotations VALIDATION FAILED (as expected, but not seen in your env) ---");
        foreach (var validationResult in dataAnnotationsResults)
        {
            var memberNames = validationResult.MemberNames.Any() ? validationResult.MemberNames : new List<string> { "General" };
            foreach (var memberName in memberNames)
            {
                if (!combinedErrors.ContainsKey(memberName)) { combinedErrors[memberName] = new List<string>(); }
                combinedErrors[memberName].Add(validationResult.ErrorMessage ?? "Invalid input by DataAnnotations.");
                logger.LogWarning($"- DataAnnotations Error for Member: '{memberName}' - Message: '{validationResult.ErrorMessage}'");
            }
        }
    }
    else
    {
        logger.LogInformation("--- Data Annotations VALIDATION SUCCESS (as seen in your env, even for invalid inputs) ---");
    }

    // --- (B) PERFORMING EXPLICIT MANUAL VALIDATION CHECKS ---
    logger.LogInformation("--- Performing Explicit Manual Validation Checks ---");
    var manualErrors = new Dictionary<string, List<string>>();

    // Helper to add error message for a specific field in manual validation
    void AddManualError(string fieldName, string message)
    {
        if (!manualErrors.ContainsKey(fieldName)) { manualErrors[fieldName] = new List<string>(); }
        manualErrors[fieldName].Add(message);
        // Also add to combinedErrors for the final response, ensuring no duplicates if already added by DataAnnotations (less likely for your case)
        if (!combinedErrors.ContainsKey(fieldName)) { combinedErrors[fieldName] = new List<string>(); }
        if (!combinedErrors[fieldName].Contains(message)) { combinedErrors[fieldName].Add(message); } // Avoids duplicate messages in final response
    }

    // Username Validation
    if (string.IsNullOrWhiteSpace(request.Username))
        AddManualError("Username", "Username is required.");
    else if (request.Username.Length < 3 || request.Username.Length > 20)
        AddManualError("Username", "Username must be between 3 and 20 characters.");
    else if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_-]+$"))
        AddManualError("Username", "Username contains invalid characters (only alphanumeric, _, - allowed)." );

    // Email Validation
    if (string.IsNullOrWhiteSpace(request.Email))
        AddManualError("Email", "Email is required.");
    else
    {
        try { new MailAddress(request.Email); }
        catch (FormatException) { AddManualError("Email", "Please enter a valid email address."); }

        if (request.Email.Length > 255)
            AddManualError("Email", "Email address is too long.");
    }

    // Phone Number Validation
    if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        AddManualError("PhoneNumber", "Phone number is required.");
    else
    {
        if (!Regex.IsMatch(request.PhoneNumber, @"^\+?\d{1,3}?[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$"))
            AddManualError("PhoneNumber", "Please enter a valid phone number format (e.g., 123-456-7890)." );
        if (request.PhoneNumber.Length < 10 || request.PhoneNumber.Length > 20)
            AddManualError("PhoneNumber", "Phone number length is invalid.");
    }

    // Password Validation
    if (string.IsNullOrWhiteSpace(request.Password))
        AddManualError("Password", "Password is required.");
    else
    {
        if (request.Password.Length < 8 || request.Password.Length > 128)
            AddManualError("Password", "Password must be at least 8 characters long.");
        if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$", RegexOptions.None, TimeSpan.FromSeconds(1)))
            AddManualError("Password", "Password must contain at least one uppercase, one lowercase, one number, and one special character.");
    }

    // Confirm Password Validation
    if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        AddManualError("ConfirmPassword", "Confirm Password is required.");
    else if (request.Password != request.ConfirmPassword)
        AddManualError("ConfirmPassword", "Passwords do not match.");

    // Log the results of manual validation
    if (manualErrors.Any())
    {
        logger.LogWarning("--- Explicit Manual Validation FAILED ---");
        foreach (var errorEntry in manualErrors)
        {
            foreach (var msg in errorEntry.Value) { logger.LogWarning($"- Manual Error for Member: '{errorEntry.Key}' - Message: '{msg}'"); }
        }
    }
    else { logger.LogInformation("--- Explicit Manual Validation SUCCESS ---"); }


    // --- Final Decision based on Combined Errors ---
    if (combinedErrors.Any())
    {
        logger.LogWarning("--- FINAL VALIDATION DECISION: FAILED (Returning 400 Bad Request) ---");
        foreach (var errorEntry in combinedErrors)
        {
            foreach (var msg in errorEntry.Value) { logger.LogWarning($"- Combined Error: {errorEntry.Key}: {msg}"); }
        }
        logger.LogWarning("--------------------------------------------------");
        return Results.BadRequest(new { errors = combinedErrors });
    }
    else
    {
        logger.LogInformation("--- FINAL VALIDATION DECISION: SUCCESS (Returning 200 OK) ---");
        logger.LogInformation("Received data is VALID.");
        logger.LogInformation("------------------------------------------");
        return Results.Ok(new { message = "Sign-up data successfully validated and received!" });
    }
})
.WithName("SignUpSecure")
.WithOpenApi();

// Run the application
app.Run();


// =======================================================
// Type Definitions (Classes and Records)
// These types are used above in the top-level statements.
// =======================================================

public class PasswordsMatchAttribute(string comparisonProperty) : ValidationAttribute
{
    private readonly string _comparisonProperty = comparisonProperty;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ErrorMessage ??= "Passwords do not match.";
        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
        if (property is null)
        {
            throw new ArgumentException($"Property '{_comparisonProperty}' not found on type '{validationContext.ObjectType.Name}'.");
        }
        var comparisonValue = property.GetValue(validationContext.ObjectInstance);
        if (!Equals(value, comparisonValue))
        {
            return new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
        }
        return ValidationResult.Success;
    }
}

public record SignUpRequest(
    // Attributes are kept for documentation/clarity, but explicit checks handle validation
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username contains invalid characters (only alphanumeric, _, - allowed).")]
    string Username,

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(255, ErrorMessage = "Email address is too long.")]
    string Email,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\+?\d{1,3}?[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$", ErrorMessage = "Please enter a valid phone number format (e.g., 123-456-7890).")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number length is invalid.")]
    string PhoneNumber,

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$", ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number, and one special character.")]
    string Password,

    [Required(ErrorMessage = "Confirm Password is required.")]
    [PasswordsMatch("Password", ErrorMessage = "Passwords do not match.")]
    string ConfirmPassword
);