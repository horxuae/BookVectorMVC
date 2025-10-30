-- Create database (run in your server if needed)
-- CREATE DATABASE BookDB;
-- USE BookDB;

CREATE TABLE Books (
    BookId INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Position NVARCHAR(100) NULL, -- physical location or label
    Vector NVARCHAR(MAX) NOT NULL -- JSON, e.g. '[0.12,0.85,0.33]'
);
