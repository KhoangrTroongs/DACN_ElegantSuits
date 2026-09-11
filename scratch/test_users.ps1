$body = @{
    email = 'admin@example.com'
    password = 'Admin@123'
} | ConvertTo-Json

$login = Invoke-RestMethod -Uri 'http://localhost:5097/api/Auth/login' -Method Post -Body $body -ContentType 'application/json'
$headers = @{
    Authorization = "Bearer $($login.Data.Token)"
}

$users = Invoke-RestMethod -Uri 'http://localhost:5097/api/Users' -Headers $headers
Write-Host "Total users from /api/Users: $($users.Data.Count)"
$users.Data | Format-Table Id, Email, FullName, IsActive, PhoneNumber
