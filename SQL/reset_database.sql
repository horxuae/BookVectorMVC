-- 切換到新資料庫
USE MVC_UserDB;
GO

-- 創建 Books 資料表
CREATE TABLE Books (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Position NVARCHAR(100) NULL, -- 實體位置或標籤
    Vector NVARCHAR(MAX) NOT NULL, -- JSON 格式的向量資料
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);
GO

-- 創建 Users 資料表
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
GO

-- 創建索引以提升查詢效能
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Position ON Books(Position);
CREATE INDEX IX_Books_CreatedDate ON Books(CreatedDate);
GO

-- 創建 Users 資料表的索引和約束
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_CreatedDate ON Users(CreatedDate);
CREATE INDEX IX_Users_Role ON Users(Role);
GO

-- 創建觸發器自動更新 UpdatedDate
CREATE TRIGGER TR_Books_UpdateDate
ON Books
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Books 
    SET UpdatedDate = GETDATE()
    FROM Books b
    INNER JOIN inserted i ON b.BookId = i.BookId;
END
GO

-- 資料庫建立完成，可以開始新增書籍資料

-- 顯示初始化結果
SELECT '資料庫初始化完成！' as 狀態;
SELECT COUNT(*) as '初始書籍數量' FROM Books;
SELECT * FROM Books;