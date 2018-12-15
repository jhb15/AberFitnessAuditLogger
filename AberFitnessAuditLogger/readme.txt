Add an environment variable:

Audit__GladosUrl

Register the auditlogger in your ConfigureServices

services.AddScoped<IAuditLogger, AuditLogger>();

Use Dependency Injection to inject an IAuditLogger when you need to log audit data.

Log data using .log() on the injected instance.