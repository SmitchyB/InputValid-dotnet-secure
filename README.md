InputValid-dotnet-secure - .NET 8 Secure Build
Purpose
This repository contains the Secure Build of the C# .NET 8 application, developed as part of a comparative study on secure coding practices across different development stacks. This particular build demonstrates the implementation of robust secure coding techniques designed to mitigate common web application vulnerabilities.



Vulnerability Focus
This application specifically addresses the mitigation of three critical and pervasive web application vulnerabilities:

Improper Input Validation: Ensuring all user input is thoroughly checked and sanitized on the server-side to prevent malicious data from entering the system.


Insecure Secrets Management: Securely handling sensitive information such as API keys and credentials, avoiding hardcoded values.

Insecure Error Handling: Preventing the exposure of sensitive internal information (e.g., file paths, database details) through verbose error messages.

Key Secure Coding Practices Implemented
Input Validation: All user inputs submitted to the /signup endpoint are subjected to comprehensive server-side validation using explicit manual checks. These checks include:
string.IsNullOrWhiteSpace for required fields.
Length constraints for fields like Username, Email, and Password.
Regular expressions (Regex.IsMatch) for username character sets, phone number formats, and password complexity.
Email format validation using MailAddress.TryCreate.
Password confirmation matching. This approach ensures data integrity and guards against common injection and format-based attacks.
Secrets Management: (Placeholder - describe how you implement secure secrets management, e.g., using .NET User Secrets or Environment Variables, which are standard .NET practices.)
Error Handling: (Placeholder - describe how you implement secure error handling, e.g., generic error pages, custom exception handling middleware, logging details internally without exposing them to users.)
Setup and Running the Application
Prerequisites
.NET 8 SDK: Specifically version 8.0.411 (as enforced by the global.json file in this project's root).
Node.js and npm/yarn (if testing with the React frontend, which runs on http://localhost:3000).
Steps
Clone the repository:
Bash

git clone <your-repo-url>
cd InputValid-dotnet-secure/backend
Verify .NET SDK version (optional, but good practice):
Bash

dotnet --info
Ensure it shows Version: 8.0.411 under ".NET SDKs installed" and "SDK: Version: 8.0.411" for the host. If not, ensure global.json is correctly placed in this backend directory.
Restore dependencies:
Bash

dotnet restore
Build the application:
Bash

dotnet build
Run the application:
Bash

dotnet run
The application will typically start on http://localhost:5000.
API Endpoints
POST /signup
Purpose: Handles user registration requests with robust server-side validation.
Method: POST
Content-Type: application/json
Request Body Example (JSON):
JSON

{
  "username": "MyValidUser",
  "email": "user@example.com",
  "phoneNumber": "123-456-7890",
  "password": "SecureP@ssw0rd!",
  "confirmPassword": "SecureP@ssw0rd!"
}
Expected Behavior:
Valid Inputs: Returns 200 OK with a success message. Check the backend console for --- /signup VALIDATION SUCCESS (via Explicit Manual Checks) ---.
Invalid Inputs (e.g., empty fields, too long username, invalid email format, non-matching passwords): Returns 400 Bad Request with a JSON payload detailing the specific validation errors. Check the backend console for --- Explicit Manual Validation FAILED --- and specific error messages, followed by --- FINAL VALIDATION DECISION: FAILED (Returning 400 Bad Request) ---.
Static Analysis Tooling
This project is designed to be analyzed by Static Analysis Security Testing (SAST) tools such as Semgrep  and .NET Roslyn Analyzers  to measure their detection capabilities for the implemented security controls and to verify compliance with secure coding standards.



Important Note: Regarding Input Validation Implementation in .NET 8
During the development and rigorous testing of this .NET 8 application, an extremely unusual and highly localized anomaly was encountered concerning the framework's built-in Data Annotations validation system.

Despite following standard secure coding practices (including correctly applying validation attributes like [Required], [StringLength], [EmailAddress], [RegularExpression] to the SignUpRequest model) and properly configuring the ASP.NET Core pipeline with builder.Services.AddControllers() and using [FromBody] for automatic model validation, the System.ComponentModel.DataAnnotations.Validator.TryValidateObject method (and by extension, the entire automatic model validation pipeline) consistently failed to register any validation errors. This occurred even when explicitly provided with inputs that were demonstrably empty strings ("", with Length: 0) or clearly invalid according to the defined attributes. The method would always return True (indicating valid) and capture 0 validation results.

Extensive troubleshooting confirmed:

The underlying .NET 8 SDK's Validator.TryValidateObject itself functions correctly in isolation (e.g., in a simple console application test within the same project environment). This ruled out a corrupted SDK.
The input data was correctly received by the backend as intended (e.g., Username: "" (Length: 0)).
The code syntax, project configuration (.csproj, global.json enforcing .NET 8), and using directives were all verified as standard and correct.
The issue persisted regardless of explicit IServiceProvider injection into ValidationContext.
The problem was not related to VS Code extensions or standard environment variables.
This behavior indicates a deeply subtle, environment-specific conflict within the ASP.NET Core web application's runtime. This anomaly prevented the framework's intended validation mechanism from recognizing and processing validation attributes.

Therefore, for this "Secure Build," the input validation for the /signup endpoint has been implemented using explicit, manual checks. This approach, while more verbose, ensures robust and reliable server-side validation against all specified criteria (required, length, format, regex, password matching), guaranteeing the application's security posture and allowing the empirical study to proceed. This situation highlights the unexpected complexities that can arise even with "batteries-included" frameworks, potentially impacting developer effort and the choice of security controls in real-world scenarios.
