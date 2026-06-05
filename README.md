# One Fashion — Cloth Shop Management System

> A complete back-office management system for a retail clothing shop, built with ASP.NET MVC 5, Entity Framework 6 (DB-First), and SQL Server.

---

## Overview

One Fashion is a role-based web application that allows clothing shop staff and admins to manage products, customers, and orders from a single dashboard. It features secure login, a live sales dashboard, master-detail order management, and a filterable order report with CSV export.

---

## Tech Stack

| | |
|---|---|
| **Backend** | ASP.NET MVC 5, .NET Framework 4.8 |
| **ORM** | Entity Framework 6 — DB-First |
| **Database** | SQL Server / LocalDB |
| **Security** | Session-based auth, BCrypt password hashing |
| **Frontend** | Bootstrap 4.5, Chart.js, Font Awesome 5, Select2, jQuery |

---

## Key Features

| Module | Highlights |
|---|---|
| **Authentication** | Login/Logout, BCrypt hashing, role-based access (Admin/Staff), custom session filter |
| **Dashboard** | Live stats, monthly sales chart, low stock alerts, recent orders |
| **Categories** | CRUD with AJAX modals — no page reload |
| **Products** | CRUD, image upload with magic-byte validation, stock tracking, pagination |
| **Customers** | CRUD, order history profile, phone uniqueness check, pagination |
| **Orders** | Master-Detail structure, AJAX product selection, auto stock deduction, TransactionScope rollback |
| **Reports** | Date and customer filters, SQL View powered, CSV export |
| **User Management** | Admin creates accounts, resets passwords, activates/deactivates users |

---

## Project Structure

```
ClothShopManagement/
|-- Controllers/
|   |-- AccountController.cs          Login, Logout, Change Password
|   |-- DashboardController.cs        Live stats and chart data
|   |-- ProductController.cs          Product CRUD and image upload
|   |-- ProductCategoryController.cs  Category CRUD and AJAX modals
|   |-- CustomerController.cs         Customer CRUD and order history
|   |-- OrderController.cs            Master-Detail order management
|   |-- ReportController.cs           Order report and CSV export
|   |-- UserController.cs             Admin user management
|
|-- Filters/
|   |-- SessionAuthorizeAttribute.cs  Custom auth filter
|
|-- Models/
|   |-- ViewModel/                    Presentation layer models
|   |-- [EF generated models]         DB-First entity classes
|
|-- Views/
|   |-- Account, Dashboard, Product, ProductCategory
|   |-- Customer, Order, Report, User, Shared
|
|-- App_Data/
|   |-- SQLQuery1.sql                 Full DB setup script
```

---

## Database Design

| Object | Type | Description |
|---|---|---|
| Users | Table | Credentials and roles |
| ProductCategory | Table | Product categories |
| Product | Table | Stock and pricing |
| Customer | Table | Customer records |
| Orders | Table | Order master |
| OrderDetails | Table | Order line items |
| vw_OrderReport | View | Report data source |
| sp_AddOrder | Stored Procedure | Insert order |
| fn_TotalAmount | Scalar Function | Qty x Price |

---

## Role Permissions

| Feature | Admin | Staff |
|---|---|---|
| Dashboard | Yes | Yes |
| Products — View, Add, Edit, Delete | Yes | Yes |
| Customers — View, Add, Edit | Yes | Yes |
| Customers — Delete | Yes | No |
| Orders — Place, Edit, Delete | Yes | Yes |
| Reports and CSV Export | Yes | Yes |
| Change Own Password | Yes | Yes |
| User Management | Yes | No |

---

## Author

**Moin Uddin**
