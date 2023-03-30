# Overview
CustomIdentity is an RESTful API built on the ASP.NET Core framework and utilizes a simplified Domain-Driven Design approach. Api enables user account management, including features such as registration, activation, login, logout, account updates, and account deletion. The API incorporates JWT tokens and OAuth 2.0 authorization for secure authentication and access control.
# Requirements
- (OS)Windows
- .NET 6 SDK (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- IDE (eg. Visual Studio 2022, JetBrains Rider)
- Swagger UI (included in vs 2022)
- Web server: Kestrel (included in vs 2022)
- PgAdmin (PostgreSQL - for database management and administration)
# Libraries
![image](https://user-images.githubusercontent.com/106349197/226959493-90e263c0-0888-43db-859a-87b1fd99f324.png)
# Setup
To use the application, assuming you have downloaded and installed the tools mentioned in the Requirements and Libraries sections, you should create your own database using pgAdmin. Next, update the connection string in the CustomIdentity.Domain project in the CustomIdentityDb.cs file (line 30) and in the CustomIdentity.API project in the Program.cs file (line 61) to match the database you created. The next step is to run the ```Update-database``` command from the **Package Manager Console**, which will create the table structure in your database according to the application requirements and the migration file (CustomIdentity.Domain/Migrations/{MyMigration.cs}). After completing these steps, execute the query in the insertDB.txt file (Solution Items/insertDB.txt) from the pgAdmin console (query tools). Now you should be able to use the application.
# Database
## CustomIdentity ERD:
![image](https://user-images.githubusercontent.com/106349197/227795515-d1ee6757-fc03-4d8f-b4d2-f6caf88c0840.png)
## Tables:
### User
User data is distributed across three tables, with the User table serving as the reference for them.
### UserProfile
The UserProfile table stores basic user information such as first name, last name, age, etc.
### UserCredential
The UserCredentials table stores sensitive user data such as passwords and account status, as well as references to the user's associated authentication methods and tokens.
### UserPermissions
The UserPermissions table is used to store references to roles assigned to a user.
### Tokens
The Tokens table stores user: registration token and Refresh Token
### UserAuthMethod
Table UserAuthMethod stores information about available user authentication methods.
### UserRoles
The UserRoles table is a join table for the many-to-many relationship between UserPermissions and WebAppRole - this table stores information about roles assigned to a specific user (specifically, to a UserPermissions instance).
### WebAppRole
The WebAppRole table stores roles available to users.
# Endpoints
### 1. SignUp
- Role: **USER**
- URL:  ```/Auth/SignUp/```
- Method: ```POST```
- Request Body: 
```json
{
  "login": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "password": "string",
  "passwordCheck": "string",
  "phoneNumber": "string"
}
```
- Function: Register a new user account.
### 2. VerifyRegistrationToken
- Role: **USER**
- URL:  ```/Auth/VerifyRegistrationToken/{emailVerificationToken}/```
- Method: ```GET```
- URL Parameters: `{emailVerificationToken}` 
- Function: Verifies the JWT registration token sent in the activation email to activate the account.
### 3. SignIn
- Role: **USER**
- URL: ```/Auth/SignIn/```
- Method: ```POST```
- Request Body:
```json
{
  "login": "string",
  "password": "string"
}
```
- Function: Log in a user and return authentication tokens (refresh token - cookie, authorization token - header).
### 4. RefreshRegistrationToken
- Role: **USER**
- URL: ```/Auth/RefreshRegistrationToken/{userEmail}/```
- Method: ```Post```
- URL Parameters: `{userEmail}`
- Function: Refreshes the registration token for the user associated with the provided email and sends it to their email address.
### 5. Logout
- Role: **USER**
- URL: ```/Auth/Logout/```
- Method: ```Post```
- Function: Logs out the user by removing the refresh token from the cookie.
### 6. UpdateLoginAndPassword
- Role: **USER**
- URL: ```/Auth/UpdateLoginAndPassword/```
- Method: ```POST```
- Request Body: 
```json
{
  "changedLogin": "string",
  "changedPassword": "string",
  "confirmChangedPassword": "string"
}
```
- Function: Allows the user to change their account login and password.
### 7. DeleteAccount
- Role: **USER**
- URL: ```/Auth/DeleteAccount/```
- Method: ```POST```
- Function: Marks the account as deleted.
### 8. ConfirmAccountDeletion
- Role: **ADMIN**
- URL: ```/Auth/ConfirmAccountDeletion/```
- Method: ```GET```
- Function: Displays all accounts marked as deleted.
### 9. ConfirmAccountDeletion
- Role: **ADMIN**
- URL: ```/Auth/ConfirmAccountDeletion/```
- Method: ```POST```
- Request Body: 
```json
{
  "accountId": 0
}
```
- Function: Accepts the account deletion and removes it from the database.
# Application functionalities
## 1. Registration
- API enables user registration, during which the user is asked to provide personal data that will be necessary for authentication and activation of their account.
## 2. Account activation
- After a successful registration process, the user can activate their account using a link sent to the email address alprovided by them. This will be necessary when attempting to log in to the user's account.
## 3. Resending activation link
- When the user loses their email with the activation link, they have the option to generate a new one and send it to their email - this process requires the user to provide their email address.
## 4. Login
- After successful activation of the account, the user has the ability to log in, during which process they are sent JWT tokens - an authorization token, which will be used by the user to authorize access to a specific functionality of the application, and a refresh token, which will be required for re-authentication of the user and generation of a new authorization token when it expires.
## 5. Logout
- A user has the ability to log out, during which process the refresh token cookie is removed from the user's browser, and the authorization token is removed from the request header.
## 6. Update user login and password
- User has the ability to update their account login and password.
## 7. Delete Account (Mark for deletion)
- "User can delete their account, however, the account is only marked as to-be-deleted and the deletion must be approved by a system administrator.
## 8. Displaying deleted accounts. (administrative function)
- Administrator has the ability to view accounts that are marked for deletion.
## 9. Account deletion approval (administrative function)
- An administrator can approve the deletion of an account by providing its ID - using the function results in a permanent deletion of the user account.










