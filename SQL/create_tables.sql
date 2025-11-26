-- Create database (run in your server if needed)
-- CREATE DATABASE BookDB;
-- USE BookDB;

CREATE TABLE Books (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Position NVARCHAR(100) NULL, -- physical location or label
    Vector NVARCHAR(MAX) NOT NULL, -- JSON, e.g. '[0.12,0.85,0.33]'
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    DisplayName NVARCHAR(100) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Member',
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastLoginDate DATETIME NULL,
    IsActive BIT DEFAULT 1
);

-- Create indexes for better performance
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Position ON Books(Position);
CREATE INDEX IX_Books_CreatedDate ON Books(CreatedDate);

CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_CreatedDate ON Users(CreatedDate);
CREATE INDEX IX_Users_Role ON Users(Role);
