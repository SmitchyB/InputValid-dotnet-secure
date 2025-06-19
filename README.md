# InputValid-dotnet-secure - .NET 8 Vulnerable Build (Improper Input Validation)

This repository houses a specific application build that is part of a larger comparative study, "Evaluating the Effectiveness of Secure Coding Practices Across Python, MERN, and .NET 8." The experiment systematically assesses how secure coding techniques mitigate critical web application vulnerabilities—specifically improper input validation, insecure secrets management, and insecure error handling—across these three diverse development stacks. Through the development of paired vulnerable and secure application versions, this study aims to provide empirical evidence on the practical effectiveness of various security controls and the impact of architectural differences on developer effort and overall security posture.

## Purpose
This particular build contains the **Vulnerable Build** of the C# .NET 8 application, specifically designed to demonstrate **Improper Input Validation**.

## Vulnerability Focus
This application build explicitly demonstrates:
* **Improper Input Validation:** The application fails to adequately check, filter, or sanitize user-supplied input, making it vulnerable to various attacks that leverage malformed or malicious data.

## Description of Vulnerability in this Build
In this version, the `/signup` endpoint processes user input without performing robust server-side validation. This means that:
* Required fields can be left empty.
* Strings of excessive length can be submitted.
* Invalid data formats (e.g., malformed emails, phone numbers) are accepted.
* Inputs potentially containing malicious characters (e.g., script tags for XSS, SQL injection payloads) are processed directly.
This lack of validation creates avenues for attackers to exploit the application, potentially leading to data corruption, unauthorized access, or other system compromises.

## Setup and Running the Application

### Prerequisites
* **.NET 8 SDK:** Specifically version `8.0.411` (as enforced by the `global.json` file in this project's root).
* Node.js and npm/yarn (if testing with the React frontend, which runs on `http://localhost:3000`).

### Steps
1.  **Clone the repository:**
    ```bash
    git clone <your-repo-url>
    # Navigate to the specific build folder, e.g.:
    cd InputValid-dotnet-secure/dotnet/vulnerable-input-validation
    ```
2.  **Verify .NET SDK version (optional, but good practice):**
    ```bash
    dotnet --info
    ```
    Ensure it shows `Version: 8.0.411` under ".NET SDKs installed" and "SDK: Version: 8.0.411" for the host. If not, ensure `global.json` is correctly placed in this project's root directory.
3.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```
4.  **Build the application:**
    ```bash
    dotnet build
    ```
5.  **Run the application:**
    ```bash
    dotnet run
    ```
    The application will typically start on `http://localhost:5000`.

## API Endpoints

### `POST /signup`
* **Purpose:** Handles user registration requests. In this vulnerable build, it processes input without proper validation.
* **Method:** `POST`
* **Content-Type:** `application/json`
* **Request Body Example (JSON):**
    ```json
    {
      "username": "",  // Will be accepted without validation error
      "email": "malicious<script>alert(1)</script>", // Will be accepted
      "phoneNumber": "abc", // Will be accepted
      "password": "1", // Will be accepted
      "confirmPassword": "1" // Will be accepted
    }
    ```
* **Expected Behavior:**
    * **Invalid Inputs (e.g., empty fields, malicious characters, invalid formats):** Returns `200 OK` with a success message, implying the data was "processed" without server-side validation rejecting it.
        * Backend console will show logs indicating the raw, invalid data was received and accepted.

## Static Analysis Tooling
This specific build is designed to be analyzed by Static Analysis Security Testing (SAST) tools such as Semgrep and .NET Roslyn Analyzers to measure their detection capabilities for the specific **input validation vulnerabilities** present in this build.
