University Counselling Services — ASP.NET Web Application

1. Project Overview

The University Counselling Services Web Application is a web-based system designed to improve the management of counselling services at a university. The application aims to assist university wellness staff in managing student information, counselling appointments, counsellor schedules, and counselling statistics through a centralised system.

The system is intended to reduce the challenges experienced when managing counselling services using paper-based records, Microsoft Outlook calendars, and Excel spreadsheets. By organising information in one system, the application aims to improve appointment tracking, record management, and the generation of counselling reports.

2. Problem Statement

The university’s Wellness Centre currently relies on manual processes, including Microsoft Outlook calendars and paper-based records, to manage counselling appointment requests. Student information and counselling statistics are also maintained using spreadsheets, making it difficult to manage information efficiently.

These processes can result in missed appointment requests, double bookings, misplaced student records, and students arriving for appointments without the appropriate counsellor being available. Furthermore, counsellors are responsible for generating their own statistics, which can become difficult to maintain alongside their daily responsibilities.

The University Counselling Services Web Application aims to address these challenges by providing a centralised platform for managing counselling appointments, student information, and reporting.

3. Project Aim

The aim of the project is to develop a secure and user-friendly web application that improves the management and administration of university counselling services.

4. Project Objectives

The objectives of the application are to:

* Improve the management of counselling appointment requests and schedules.
* Reduce missed appointment requests and double bookings.
* Maintain organised student information and counselling records.
* Assist wellness staff in tracking and managing appointments.
* Support the generation of counselling statistics and reports.
* Improve the organisation and accessibility of relevant information.
* Protect confidential student information through appropriate security measures.
* Advertise counselling programmes and wellness initiatives to increase awareness.

5. Scope of the Application

The application focuses on the administrative management of university counselling services.

The intended scope includes:

* Managing student information.
* Managing counselling appointment requests.
* Scheduling and tracking appointments.
* Managing counsellor information and appointment allocations.
* Maintaining counselling-related records.
* Generating counselling statistics and reports.
* Providing information about counselling programmes and wellness initiatives.

The implementation of individual functions depends on the features developed within the application.

6. Main System Users

Administrator

The administrator is responsible for overseeing the application and managing the information and processes assigned to the administrative role.

Wellness Staff

Wellness staff use the application to manage counselling appointment requests, organise schedules, maintain student information, and access relevant counselling records.

Counsellors or Psychologists

Counsellors or psychologists are responsible for attending to students, managing their assigned appointments, and maintaining relevant counselling information and statistics, subject to their access permissions.

Students

Students are the individuals receiving counselling services. Student-related functions should be supported according to the requirements and access permissions implemented in the system.

7. Functional Requirements

The application is intended to support the following functional requirements.

7.1 Student Information Management

The system should support the management of student information required for counselling services. Student records should be organised and accessible only to authorised users.

7.2 Appointment Management

The system should allow authorised wellness staff to manage appointment requests, assign appointment dates, and track appointment statuses.

7.3 Appointment Scheduling

The system should help staff organise counselling schedules and reduce scheduling conflicts, including double bookings.

7.4 Counsellor Management

The system should support the management of counsellor information and the allocation of appointments to the appropriate counsellors.

7.5 Reports and Statistics

The system should support the collection and generation of counselling statistics and reports to assist wellness staff with administrative and reporting responsibilities.

7.6 Programme Announcements

The system should support the sharing of information about counselling programmes, wellness activities, and available support services.

7.7 Record Security

The system should protect confidential student information through appropriate authentication, authorisation, and secure data-handling practices.

8. Non-Functional Requirements

Security

The application should restrict access to confidential information and ensure that users can access only the functions and records permitted by their roles.

Usability

The application should provide a clear and user-friendly interface that allows authorised users to perform their tasks efficiently.

Reliability

The application should support consistent management of appointment information and student records.

Performance

The application should respond within a reasonable time when users access information, manage appointments, and generate reports.

Maintainability

The application should be organised into appropriate components to make the code easier to understand, test, maintain, and extend.

Data Integrity

The application should help maintain accurate and consistent information, particularly when managing student records, appointments, and counsellor schedules.

9. Technologies Used

The application is developed using the following technologies, where applicable to the project implementation:

* ASP.NET Core MVC: Web application framework and architectural pattern.
* C#: Main programming language used for application logic.
* HTML: Structures the content of web pages.
* CSS: Controls the appearance and layout of the user interface.
* JavaScript: Supports client-side interactions where required.
* Database: Stores application information, depending on the database configured for the project.
* Entity Framework Core: Supports database access and data management if implemented.
* Visual Studio: Development environment used to build and run the application.
* Git and GitHub: Version control and source code repository management.

10. Application Architecture

The application follows the ASP.NET Core MVC pattern where implemented.

Models

Models represent the application’s data structures and may contain validation rules for the information managed by the system.

Views

Views provide the user interface displayed in the web browser. They present information and forms used to interact with the application.

Controllers

Controllers handle incoming requests, coordinate application logic, interact with models or data services, and return the appropriate views or responses.

Database

The database stores the information required by the application, such as student details, appointment information, and other relevant records, according to the implemented database design.

11. Project Structure

A typical ASP.NET Core MVC project may have the following structure:

ASP.NET-Web-Application/
│
├── Controllers/
│   ├── HomeController.cs
│   └── [Other Controllers].cs
│
├── Models/
│   └── [Application Models].cs
│
├── Views/
│   ├── Home/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
│
├── Properties/
│   └── launchSettings.json
│
├── appsettings.json
├── Program.cs
├── [ProjectName].csproj
└── README.md

The structure above is an example. The actual folder and file names should reflect the contents of the repository.

12. System Requirements

To develop and run the application, the following software may be required:

* Windows, or another operating system supported by the chosen .NET version.
* Visual Studio with the ASP.NET and web development workload, or another compatible development environment.
* The .NET SDK version targeted by the project.
* A compatible database server or database engine, if the application uses one.
* Git, if the source code is managed through GitHub.

13. Installation and Setup

Step 1: Clone the Repository

Open a terminal and run:

git clone YOUR_REPOSITORY_URL

Replace YOUR_REPOSITORY_URL with the actual URL of the GitHub repository.

Step 2: Open the Project

Navigate to the directory containing the ASP.NET application.

Open the solution file (.sln) in Visual Studio. If the project does not contain a solution file, open its .csproj file instead.

Step 3: Install or Verify the .NET SDK

Ensure that the installed .NET SDK is compatible with the version specified in the project file.

To check the installed SDK versions, run:

dotnet --list-sdks

Step 4: Restore Dependencies

Open a terminal in the directory containing the project file and run:

dotnet restore

This restores the NuGet packages required by the application.

Step 5: Configure the Database

If the application uses a database, configure the appropriate connection string in the relevant configuration file, such as appsettings.json.

Do not commit passwords, private connection details, or other sensitive credentials to a public repository.

Step 6: Apply Database Migrations

If the project uses Entity Framework Core migrations, ensure the required tools and dependencies are installed, then apply the migrations using the appropriate project configuration.

For example:

dotnet ef database update

This command should be used only when the project is configured for Entity Framework Core migrations.

Step 7: Build the Application

Run the following command from the project directory:

dotnet build

Resolve any build errors before attempting to run the application.

Step 8: Run the Application

Start the application with:

dotnet run

Open the local URL displayed in the terminal in a web browser.

Alternatively, use Visual Studio to select the appropriate launch profile and run the project.

14. Configuration

The application’s configuration may be maintained through files such as:

* appsettings.json
* appsettings.Development.json
* Properties/launchSettings.json

These files can contain application settings, development configuration, and local launch profiles.

Sensitive credentials should be stored using appropriate development secrets or environment variables rather than committed to source control.

15. Testing

The application should be tested to verify that its implemented functions operate correctly.

Testing may include:

* Verifying that student information can be managed correctly.
* Checking that appointment requests and appointment statuses are handled appropriately.
* Testing appointment scheduling and conflict prevention.
* Checking that authorised users can access permitted information.
* Verifying that unauthorised users cannot access restricted records.
* Testing report and statistics generation where implemented.
* Checking that validation errors are handled appropriately.
* Testing database connectivity and data persistence.

Only features that have been implemented should be reported as completed or tested.

16. Security and Privacy

Counselling records may contain confidential personal information. The application should therefore use appropriate security measures to protect student privacy.

These measures may include authentication, role-based access control, secure database access, input validation, and secure management of credentials.

Access to sensitive counselling records should be limited to authorised users with a legitimate need to view the information.

17. Known Limitations

The application’s actual limitations depend on its current implementation.

Features that have not yet been implemented, tested, or connected to the database should not be considered fully functional merely because they appear in the project documentation.

18. Future Improvements

Potential improvements may include:

* Enhancing appointment scheduling and conflict detection.
* Improving reporting and statistics generation.
* Strengthening access controls and audit logging.
* Improving the user interface and accessibility.
* Expanding testing and error handling.
* Improving data validation and database management.

These are potential improvements and should not be interpreted as existing features unless they have been implemented.

19. Project Status

This application is part of a university Information Technology project focused on improving the management of counselling services through a web-based system.

20. Contributors

(Group D) F Mahlumba (Student ID: 219015487) IM Molefi (Student ID: 223048296) TC Selepe (Student ID: 224004059) N Dlali (Student ID: 220039413) BBL Ntsutle (Student ID: 224085810) T Mofokeng (Student ID: 224079447)
