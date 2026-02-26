# BackendPOS

Backend POS API untuk manajemen produk, order, pembayaran, dan laporan penjualan.

## Struktur Solution

- `BackendPOS.Api`: entry point ASP.NET Core, controller, auth, swagger, middleware.
- `BackendPOS.Application`: DTO, validator, dan kontrak service.
- `BackendPOS.Domain`: entity dan enum domain.
- `BackendPOS.Infrastructure`: EF Core DbContext, migration, dan implementasi service.

## Tech Stack

- .NET 8 + ASP.NET Core
- EF Core + Npgsql (PostgreSQL)
- JWT Bearer Authentication
- FluentValidation
- Swagger / OpenAPI

## Prasyarat

- .NET SDK 8
- PostgreSQL

## Konfigurasi

File utama: `BackendPOS.Api/appsettings.Development.json`

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5433;Database=backendposdb;Username=postgres;Password=postgres"
},
"Jwt": {
  "Issuer": "BackendPOS",
  "Audience": "BackendPOS",
  "Key": "your-secret-key",
  "ExpiresMinutes": 120
}
```

Catatan: kode membaca `Jwt:ExpiresMinutes` (bukan `ExpiredMinutes`).

## Database dan Migrations

Menjalankan migration:

```powershell
dotnet ef database update --project BackendPOS.Infrastructure --startup-project BackendPOS.Api
```

Membuat migration baru:

```powershell
dotnet ef migrations add <Name> --project BackendPOS.Infrastructure --startup-project BackendPOS.Api
```

File migration berada di `BackendPOS.Infrastructure/Migrations`.

## Menjalankan Aplikasi

```powershell
dotnet run --project BackendPOS.Api
```

Swagger tersedia di:
- `http://localhost:5246/swagger`
- `https://localhost:7117/swagger`

## Autentikasi

Login untuk mendapatkan JWT:

```
POST /api/auth/login
```

Body contoh:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

## Endpoint Utama

Produk:
- `GET /api/products` (Authorize)
- `POST /api/products` (Authorize, role Admin)

Order:
- `POST /api/orders` (Authorize, role Admin/Cashier)
- `GET /api/orders/{id}` (Authorize, role Admin/Cashier)
- `GET /api/orders?status=&from=&to=` (Authorize, role Admin/Cashier)
- `POST /api/orders/{id}/pay` (Authorize, role Admin/Cashier)
- `POST /api/orders/{id}/cancel` (Authorize, role Admin/Cashier)

Report:
- `GET /api/reports/sales/daily?date=YYYY-MM-DD` (Authorize, role Admin)

## Contoh Payload

Create product:

```json
{
  "name": "Es Teh",
  "sku": "TEH-001",
  "price": 5000,
  "stock": 100,
  "isActive": true
}
```

Create order:

```json
{
  "items": [
    { "productId": "GUID-PRODUCT-1", "qty": 2 },
    { "productId": "GUID-PRODUCT-2", "qty": 1 }
  ]
}
```

Pay order:

```json
{
  "method": "Cash",
  "amount": 20000,
  "reference": null
}
```

## Alur Logika Inti

- Create order membuat status `Draft`, menghitung total dari snapshot harga produk.
- Pay order:
  - Validasi status dan jumlah bayar.
  - Kurangi stok produk via transaksi database.
  - Simpan payment dan set `PaidAtUtc`, status menjadi `Paid`.
- Cancel order hanya untuk status `Draft`.
- Report daily sales menjumlahkan total order `Paid` pada rentang tanggal.

## Catatan dan TODO

- Autentikasi masih hardcoded pada `AuthController`.
- Validator produk memakai DTO, sementara endpoint create produk menerima entity langsung.
- Migration `20260212154051_AddPaymentsAndOrderStatus` masih kosong.
- Mapping response order item belum mengisi `Subtotal`.
