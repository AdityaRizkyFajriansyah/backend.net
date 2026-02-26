# Progress BackendPOS

## Status Saat Ini

Tahap MVP awal. Fondasi arsitektur dan fitur inti order sudah ada, namun masih butuh penguatan di autentikasi, manajemen produk/stock, reporting, dan quality (testing, logging) untuk menjadi POS yang matang.

## Yang Sudah Selesai

- Struktur solusi 4 layer (Api, Application, Domain, Infrastructure).
- Model domain utama: Product, Order, OrderItem, Payment, enum status/metode.
- EF Core DbContext + migrasi awal sampai pembayaran dan PaidAtUtc.
- Endpoints dasar:
  - Auth login (JWT hardcoded sementara).
  - Produk: list dan create.
  - Order: create, get by id, list filter, pay, cancel.
  - Report: daily sales.
- Business logic order:
  - Create order dengan snapshot harga.
  - Pay order dengan transaksi DB dan pengurangan stock.
  - Cancel hanya untuk Draft.
- Validasi input memakai FluentValidation (orders dan products).

## Yang Masih Kurang / Gap ke POS yang Bagus

- Auth/Users:
  - Login masih hardcoded, belum ada user/role di database.
  - Claim role belum konsisten.
- Produk:
  - CRUD belum lengkap (update, delete, detail).
  - Validasi produk belum terpakai karena endpoint menerima entity langsung.
- Inventory:
  - Tidak ada fitur restock, stock adjustment, stock movement log.
- Payments:
  - Belum ada refund, partial payment, atau multiple payment per order.
- Reporting:
  - Baru daily sales; belum ada range, top products, cashier summary.
- Quality:
  - Belum ada tests, logging terstruktur, dan error handling konsisten.
- Data consistency:
  - Ada migration kosong.
  - Potensi mismatch nama tabel `Products` vs `products` di SQL update.
  - JWT config key masih beda (`ExpiresMinutes` vs `ExpiredMinutes`).

## Milestone

- [x] Fondasi project + domain model
- [x] Order flow end-to-end (create, pay, cancel)
- [x] Daily sales report sederhana
- [ ] Auth berbasis DB + role management
- [ ] Produk CRUD lengkap + inventory management
- [ ] Reporting komprehensif
- [ ] Testing + observability

## Rekomendasi Tahap Berikutnya

1) Perbaiki autentikasi (user/role dari DB) dan konsistensi JWT.
2) Lengkapi CRUD produk + validasi DTO.
3) Tambah inventory flow (restock, stock adjustment, audit log).
4) Perluas laporan penjualan.
5) Tambahkan testing dan logging.
