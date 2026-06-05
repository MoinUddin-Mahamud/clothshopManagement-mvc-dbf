USE master
GO
IF DB_ID('MyClothShopDB') IS NOT NULL
    DROP DATABASE MyClothShopDB
GO
CREATE DATABASE MyClothShopDB
GO
USE MyClothShopDB
GO

CREATE TABLE Users (
    UserId      INT IDENTITY PRIMARY KEY,
    UserName    NVARCHAR(100) UNIQUE NOT NULL,
    [Password]  NVARCHAR(200) NOT NULL,
    UserRole    NVARCHAR(50)  NOT NULL,
    IsActive    BIT DEFAULT 1
)
GO

INSERT INTO Users (UserName, [Password], UserRole, IsActive) VALUES
('admin',   '$2a$11$rBKV5e.QMHqJE7lK3ePSi.1Fz3e2K9YDM3WvG8kOvS6mK2eTu7YMu', 'Admin', 1),
('manager', '$2a$11$rBKV5e.QMHqJE7lK3ePSi.DmHdR1j8fQeYzX4lNuT9vB0kSEoWYGa', 'Admin', 1),
('staff1',  '$2a$11$rBKV5e.QMHqJE7lK3ePSi.Xk3Wf9Mv2LqPnA5bRsY7tDcUhVoE1Ky', 'Staff', 1),
('staff2',  '$2a$11$rBKV5e.QMHqJE7lK3ePSi.Xk3Wf9Mv2LqPnA5bRsY7tDcUhVoE1Ky', 'Staff', 1)
GO

CREATE TABLE ProductCategory (
    ProductCategoryId   INT IDENTITY PRIMARY KEY,
    CategoryName        NVARCHAR(100)  NOT NULL,
    CategoryDescription NVARCHAR(500)
)
GO

INSERT INTO ProductCategory VALUES
('Men Wear',    'Men Clothing Items'),
('Women Wear',  'Women Clothing Items'),
('Kids Wear',   'Kids Clothing Items'),
('Winter Wear', 'Winter Clothing Items'),
('Accessories', 'Fashion Accessories')
GO

CREATE TABLE Product (
    ProductId           INT IDENTITY PRIMARY KEY,
    ProductName         NVARCHAR(200)  NOT NULL,
    Size                NVARCHAR(50),
    Color               NVARCHAR(50),
    UnitPrice           DECIMAL(18,2)  NOT NULL,
    AvailableQuantity   INT            NOT NULL,
    IsActive            BIT DEFAULT 1,
    ProductImage        NVARCHAR(500),
    ProductCategoryId   INT NOT NULL
        FOREIGN KEY REFERENCES ProductCategory(ProductCategoryId)
)
GO

INSERT INTO Product VALUES
('Formal Shirt',    'M',    'White',  1800, 50, 1, NULL, 1),
('Casual Shirt',    'L',    'Blue',   1500, 40, 1, NULL, 1),
('T-Shirt',         'XL',   'Black',   900, 80, 1, NULL, 1),
('Ladies Saree',    'Free', 'Red',    4200, 20, 1, NULL, 2),
('Kurti',           'M',    'Green',  1200, 35, 1, NULL, 2),
('Kids Dress',      'S',    'Pink',   2000, 30, 1, NULL, 3),
('Kids T-Shirt',    'XS',   'Yellow',  600, 60, 1, NULL, 3),
('Winter Jacket',   'XL',   'Black',  6000, 15, 1, NULL, 4),
('Woolen Sweater',  'L',    'Navy',   3200, 25, 1, NULL, 4),
('Leather Belt',    'Free', 'Black',   900, 60, 1, NULL, 5),
('Scarf',           'Free', 'Maroon',  500,  8, 1, NULL, 5),
('Cap',             'Free', 'White',   400,  5, 1, NULL, 5)
GO

CREATE TABLE Customer (
    CustomerId      INT IDENTITY PRIMARY KEY,
    CustomerName    NVARCHAR(200) NOT NULL,
    ContactNumber   NVARCHAR(20)  NOT NULL UNIQUE,
    ContactAddress  NVARCHAR(500) NOT NULL,
    CreatedDate     DATETIME DEFAULT GETDATE()
)
GO

INSERT INTO Customer VALUES
('Rahim Uddin',     '01711111111', 'Mirpur, Dhaka',     GETDATE()),
('Karim Mia',       '01822222222', 'Uttara, Dhaka',     GETDATE()),
('Sabbir Hossain',  '01933333333', 'Dhanmondi, Dhaka',  GETDATE()),
('Fatema Begum',    '01644444444', 'Gulshan, Dhaka',    GETDATE()),
('Nasrin Akter',    '01555555555', 'Banani, Dhaka',     GETDATE())
GO

CREATE TABLE Orders (
    OrderId     INT IDENTITY PRIMARY KEY,
    CustomerId  INT NOT NULL
        FOREIGN KEY REFERENCES Customer(CustomerId),
    OrderDate   DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL
)
GO

CREATE TABLE OrderDetails (
    OrderDetailsId    INT IDENTITY PRIMARY KEY,
    OrderId           INT NOT NULL
        FOREIGN KEY REFERENCES Orders(OrderId) ON DELETE CASCADE,
    ProductCategoryId INT NOT NULL
        FOREIGN KEY REFERENCES ProductCategory(ProductCategoryId),
    ProductId         INT NOT NULL
        FOREIGN KEY REFERENCES Product(ProductId),
    OrderQuantity     INT           NOT NULL,
    OrderUnit         NVARCHAR(50)  NOT NULL,
    UnitPrice         DECIMAL(18,2) NOT NULL,
    Amount            DECIMAL(18,2) NOT NULL
)
GO

INSERT INTO Orders VALUES (1, DATEADD(day,-3, GETDATE()), 3300)
INSERT INTO Orders VALUES (2, DATEADD(day,-2, GETDATE()), 4200)
INSERT INTO Orders VALUES (3, DATEADD(day,-1, GETDATE()), 6000)
INSERT INTO Orders VALUES (1, GETDATE(), 1800)
GO

INSERT INTO OrderDetails VALUES (1,1,1,2,'Pcs',1800,3600)
INSERT INTO OrderDetails VALUES (2,2,4,1,'Pcs',4200,4200)
INSERT INTO OrderDetails VALUES (3,4,8,1,'Pcs',6000,6000)
INSERT INTO OrderDetails VALUES (4,1,1,1,'Pcs',1800,1800)
GO

UPDATE Product SET AvailableQuantity = AvailableQuantity - 3 WHERE ProductId = 1
UPDATE Product SET AvailableQuantity = AvailableQuantity - 1 WHERE ProductId = 4
UPDATE Product SET AvailableQuantity = AvailableQuantity - 1 WHERE ProductId = 8
GO

CREATE FUNCTION dbo.fn_TotalAmount(@Qty INT, @Price DECIMAL(18,2))
RETURNS DECIMAL(18,2)
AS
BEGIN
    RETURN @Qty * @Price
END
GO

CREATE PROCEDURE sp_AddOrder
    @CustomerId  INT,
    @TotalAmount DECIMAL(18,2)
AS
BEGIN
    INSERT INTO Orders (CustomerId, TotalAmount)
    VALUES (@CustomerId, @TotalAmount)
    SELECT SCOPE_IDENTITY() AS NewOrderId
END
GO

CREATE VIEW vw_OrderReport
AS
SELECT
    od.OrderDetailsId,
    o.OrderId,
    c.CustomerName,
    o.OrderDate,
    p.ProductName,
    od.OrderQuantity,
    od.UnitPrice,
    od.Amount
FROM Orders o
JOIN Customer     c  ON o.CustomerId  = c.CustomerId
JOIN OrderDetails od ON o.OrderId     = od.OrderId
JOIN Product      p  ON od.ProductId  = p.ProductId
GO

SELECT * FROM Users
SELECT * FROM ProductCategory
SELECT * FROM Product
SELECT * FROM Customer
SELECT * FROM Orders
SELECT * FROM OrderDetails
SELECT * FROM vw_OrderReport
GO
