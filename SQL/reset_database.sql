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
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    UpdatedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- 創建索引以提升查詢效能
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Position ON Books(Position);
CREATE INDEX IX_Books_CreatedDate ON Books(CreatedDate);
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