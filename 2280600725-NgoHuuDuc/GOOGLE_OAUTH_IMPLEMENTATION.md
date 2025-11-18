# Google OAuth Login Implementation Guide

## 📋 Overview
This document describes the complete implementation of Google OAuth authentication for the Elegant Suits application.

## 🎯 Features Implemented

### 1. **Google OAuth Authentication**
- Users can log in using their Google account
- Automatic account creation for new Google users
- Linking Google accounts to existing email accounts
- Secure token-based authentication

### 2. **Database Integration**
- User information is automatically saved to the database
- OAuth provider details are stored (LoginProvider, ProviderKey)
- Profile pictures from Google are saved

### 3. **Email Notifications**
- Welcome email sent to new users who register via Google
- Login notification email for existing users
- Professional HTML email templates

## 🔧 Configuration

### Google OAuth Credentials
The following credentials are configured in `appsettings.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "338699126878-2lpqaf50h0rfuvamgl762f46s6gjetip.apps.googleusercontent.com",
    "ClientSecret": "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"
  }
}
```

### Email Settings
Email notifications are configured in `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderName": "Elegant Suits",
  "SenderEmail": "noreply@elegantsuits.com",
  "Username": "",
  "Password": "",
  "EnableSsl": true
}
```

**Note:** To enable email notifications, you need to configure the `Username` and `Password` fields with valid SMTP credentials.

## 📁 Files Modified/Created

### New Files Created:
1. **Services/Interfaces/IEmailService.cs** - Email service interface
2. **Services/EmailService.cs** - Email service implementation
3. **add_oauth_columns.sql** - SQL script to add OAuth columns

### Modified Files:
1. **Models/User.cs** - Added OAuth properties
2. **Program.cs** - Configured Google OAuth authentication
3. **Controllers/AccountController.cs** - Added OAuth login methods
4. **Views/Account/Login.cshtml** - Added Google login button
5. **appsettings.json** - Added OAuth and email configuration

## 🗄️ Database Changes

### New Columns in AspNetUsers Table:
- `LoginProvider` (nvarchar(max), nullable) - OAuth provider name (e.g., "Google")
- `ProviderKey` (nvarchar(max), nullable) - Unique identifier from OAuth provider
- `IsOAuthUser` (bit, default: 0) - Flag indicating if user uses OAuth

### To Apply Database Changes:
Run the SQL script in SQL Server Management Studio or use the following command:

```bash
sqlcmd -S VERON -d WEBQLSP -i "2280600725-NgoHuuDuc/add_oauth_columns.sql"
```

Or execute the script manually in your SQL Server Management Studio.

##[object Object]How It Works

### User Flow:

1. **New Google User:**
   - User clicks "Đăng nhập với Google" button
   - Redirected to Google login page
   - After successful authentication, redirected back to application
   - New account created automatically with Google profile information
   - User assigned "User" role
   - Welcome email sent to user
   - User logged in and redirected to home page

2. **Existing User with Same Email:**
   - Google account is linked to existing account
   - OAuth properties updated
   - Login notification email sent
   - User logged in successfully

3. **Returning Google User:**
   - User clicks "Đăng nhập với Google" button
   - Automatically logged in using existing credentials
   - Login notification email sent

## 📧 Email Notifications

### Welcome Email (New Users):
- Subject: "Đăng nhập thành công với Google - Elegant Suits"
- Contains: User information, login timestamp, security notice
- Professional HTML template with branding

### Login Email (Existing Users):
- Subject: "Đăng nhập thành công với Google - Elegant Suits"
- Contains: Login confirmation, timestamp, security alert

## 🔒 Security Features

1. **Secure Authentication:**
   - Uses OAuth 2.0 protocol
   - Tokens are securely managed by ASP.NET Core Identity
   - External sign-in scheme configured

2. **Data Protection:**
   - User passwords not required for OAuth users
   - Email verification automatic via Google
   - Profile data validated before storage

3. **Account Linking:**
   - Prevents duplicate accounts with same email
   - Secure linking of OAuth providers to existing accounts

## 🎨 UI Changes

### Login Page Updates:
- Added professional Google login button with official Google branding
- "HOẶC" divider between traditional and OAuth login
- Responsive design that works on all devices
- Google icon SVG included for authentic look

## 🧪 Testing

### To Test Google OAuth:
1. Run the application
2. Navigate to `/Account/Login`
3. Click "Đăng nhập với Google" button
4. Sign in with a Google account
5. Verify:
   - User is created in database
   - OAuth fields are populated
   - Email notification is sent (if configured)
   - User is logged in successfully

### Test Scenarios:
- ✅ New user registration via Google
- ✅ Existing user login via Google
- ✅ Linking Google to existing email account
- ✅ Email notifications
- ✅ Profile picture import from Google
- ✅ Role assignment (User role)

## 📝 Important Notes

1. **Email Configuration:**
   - Email notifications will only work if SMTP credentials are configured
   - If not configured, emails are logged but not sent
   - Application continues to work without email configuration

2. **Google OAuth Setup:**
   - Ensure the redirect URI is configured in Google Cloud Console
   - Redirect URI format: `https://yourdomain.com/signin-google`
   - For local testing: `https://localhost:5001/signin-google`

3. **Database:**
   - Run the SQL script before testing
   - Backup database before applying changes

## 🔄 Future Enhancements

Potential improvements for future versions:
- Add Facebook OAuth
- Add Microsoft OAuth
- Two-factor authentication
- Email verification for traditional registration
- Password reset functionality
- Account management page for OAuth users

## 📞 Support

For issues or questions, please contact the development team.

---

**Implementation Date:** November 18, 2025  
**Version:** 1.0  
**Status:** ✅ Complete and Ready for Testing

