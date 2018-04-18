# SearchUser
Web API developed using .Net Core 2 and others technologies to provide an endpoint to create, find and sign in users.

## The package contains:
Solution, projects and source files for this project on Visual Studio 2017 and C# ASP.Net Core 2.

## Settings

### Connection string
Edit appSettings.json > ConnectionStrings > DefaultConnection to configure default connection string to MS SqlServer Database.
Development environment is using InMemoryDatabase.

### Environment
Set environment variable ASPNETCORE_ENVIRONMENT=Production to setup environment for application.

### Launching
Edit Properties\launchSettings.json for launch settings

## Endpoints

### /api/signup
Execute signup on application.
* No authentication required.
* Request method: POST
* Body: json object
```json
{
    "name": "Matheus",
    "email": "matheusmaximo@gmail.com",
    "password": "Passw0rd!",
    "telephones": [
        {
            "number": "+353123456789"
        },
        {
            "number": "+353987654321"
        }
    ]
}
```
* Response: json object with user data
```json
{
    "id": "USERID",
    "createdOn": "2018-04-18T13:03:05.2947582+01:00",
    "lastUpdatedOn": "2018-04-18T13:03:07.0270848+01:00",
    "lastLoginOn": "2018-04-18T13:03:06.5798319+01:00",
    "token": "TOKEN"
}
```

### /api/signin
Execute signin on application.
* No authentication required.
* Request method: POST
* Body: json object
```json
{
    "email": "matheusmaximo@gmail.com",
    "password": "Passw0rd!"
}
```
* Response: json object with user data
```json
{
    "id": "USERID",
    "createdOn": "2018-04-18T13:03:05.2947582+01:00",
    "lastUpdatedOn": "2018-04-18T13:03:07.0270848+01:00",
    "lastLoginOn": "2018-04-18T13:03:06.5798319+01:00",
    "token": "TOKEN"
}
```

### /api/finduser
Search user on database
* Requires authentication Bearer. Token given on /api/signup OR /api/signin
* Request method: GET
* Url parameter: USERID (I.E. /api/finduser/79bfe381-050d-4cd4-9cd7-64b3a68d8faf )
* Body: no needs body
* Response: json object with user data
```json
{
    "id": "USERID",
    "createdOn": "2018-04-18T13:03:05.2947582+01:00",
    "lastUpdatedOn": "2018-04-18T13:03:07.0270848+01:00",
    "lastLoginOn": "2018-04-18T13:03:06.5798319+01:00",
    "token": null
}
```

### /documentation
Project documentation. Swagger like.
* No authentication required.
* Request method: GET
* Response: HTML
