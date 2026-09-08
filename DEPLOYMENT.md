# HotelLuxury production deployment

## Hosting requirements

- Windows hosting or VPS with the ASP.NET Core 8 Hosting Bundle
- SQL Server with a dedicated database/user
- HTTPS certificate and a writable persistent folder for uploads and Data Protection keys
- Kavenegar API key and sender line if SMS is enabled

## Required environment variables

Do not put these values in `appsettings*.json` or source control.

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True
AllowedHosts=hotel.example.com;www.hotel.example.com
DataProtection__KeysPath=D:\HostingData\HotelLuxury\keys
Storage__UploadsPath=D:\HostingData\HotelLuxury\uploads
SmsSettings__Provider=Kavenegar
SmsSettings__ApiKey=YOUR_REAL_API_KEY
SmsSettings__Sender=YOUR_SENDER_LINE
AdminSeed__Email=owner@example.com
AdminSeed__Password=USE_A_LONG_RANDOM_PASSWORD
```

`AdminSeed` is only needed for the first start. Remove both variables after the first admin is created.

## Database migration

Run migrations as an explicit deployment step, not while the website is serving traffic:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Production'
$env:ConnectionStrings__DefaultConnection='...'
dotnet ef database update --project Hotel.Infrastructure --startup-project Hotel.Web
```

Back up the production database before every migration.

## Publish

```powershell
dotnet publish Hotel.Web\Hotel.Web.csproj -c Release -o .\publish --no-restore
```

Deploy the contents of `publish`, configure the environment variables in the hosting control panel or IIS, and grant the application pool identity write permission only to the configured `keys` and `uploads` folders.

## Verification after deployment

1. Open `/health` and confirm HTTP 200.
2. Verify `/`, `/Home/Rooms`, `/Reservation/Track`, and `/Admin/Account/Login`.
3. Create one test reservation and confirm the Kavenegar log in the admin panel.
4. Upload a test gallery image and restart the app to confirm it persists.
5. Remove the initial `AdminSeed` environment variables.

## Reverse proxy

If the host terminates HTTPS before Kestrel, set `ReverseProxy__Enabled=true` and add only the proxy IP addresses through `ReverseProxy__KnownProxies__0`, `ReverseProxy__KnownProxies__1`, and so on.
