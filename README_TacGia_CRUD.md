# Trang CRUD Tác giả - Hướng dẫn sử dụng

## Tổng quan
Trang CRUD (Create, Read, Update, Delete) Tác giả đã được xây dựng lại với giao diện hiện đại, responsive và các tính năng nâng cao.

## Các tính năng chính

### 1. Trang Danh sách (Index)
- **Tìm kiếm và lọc**: Tìm kiếm theo tên tác giả hoặc mô tả
- **Sắp xếp**: Sắp xếp theo tên tác giả (tăng/giảm)
- **Phân trang**: Hiển thị 10 tác giả mỗi trang
- **Thao tác nhanh**: Nút xem chi tiết, chỉnh sửa, xóa
- **Thông báo**: Hiển thị thông báo thành công/lỗi
- **Responsive**: Tương thích với mọi thiết bị

### 2. Trang Thêm mới (Create)
- **Form validation**: Kiểm tra dữ liệu đầu vào
- **Xem trước**: Hiển thị preview real-time
- **Đếm ký tự**: Hiển thị số ký tự đã nhập
- **Kiểm tra trùng lặp**: AJAX kiểm tra tên tác giả đã tồn tại
- **Giao diện thân thiện**: Layout 2 cột với preview

### 3. Trang Chỉnh sửa (Edit)
- **Tương tự Create**: Các tính năng như trang thêm mới
- **Kiểm tra trùng lặp**: Loại trừ tác giả hiện tại
- **Thông tin chi tiết**: Hiển thị ID và thông tin tác giả
- **Nút thao tác**: Chỉnh sửa, xem chi tiết, hủy bỏ

### 4. Trang Chi tiết (Details)
- **Thông tin đầy đủ**: Hiển thị tất cả thông tin tác giả
- **Thống kê**: Số ký tự mô tả, trạng thái
- **Thao tác nhanh**: Nút chỉnh sửa, xóa, quay lại
- **Thông tin liên quan**: Các sản phẩm liên quan (có thể mở rộng)

### 5. Trang Xóa (Delete)
- **Xác nhận an toàn**: Checkbox xác nhận hiểu rõ hậu quả
- **Phân tích tác động**: Hiển thị các ảnh hưởng khi xóa
- **Kiểm tra ràng buộc**: Không cho phép xóa nếu đang được sử dụng
- **Giao diện cảnh báo**: Màu sắc và icon cảnh báo

## Công nghệ sử dụng

### Backend
- **ASP.NET MVC 5**: Framework chính
- **Entity Framework 6**: ORM cho database
- **PagedList**: Phân trang
- **jQuery**: AJAX và validation

### Frontend
- **Bootstrap 5**: Framework CSS
- **Font Awesome**: Icons
- **jQuery**: JavaScript library
- **DataTables**: Bảng dữ liệu nâng cao

## Cấu trúc file

```
Areas/Admin/
├── Controllers/
│   └── Tac_GiaController.cs          # Controller chính
├── Views/Tac_Gia/
│   ├── Index.cshtml                  # Trang danh sách
│   ├── Create.cshtml                 # Trang thêm mới
│   ├── Edit.cshtml                   # Trang chỉnh sửa
│   ├── Details.cshtml                # Trang chi tiết
│   └── Delete.cshtml                 # Trang xóa
└── Views/
    └── _ViewStart.cshtml             # Cấu hình chung

Content/
└── Site.css                          # CSS tùy chỉnh

packages.config                       # Dependencies
```

## Hướng dẫn cài đặt

### 1. Cài đặt packages
```bash
Install-Package PagedList
Install-Package PagedList.Mvc
```

### 2. Cấu hình
- Thêm `@using PagedList.Mvc` vào `Areas/Admin/Views/_ViewStart.cshtml`
- Đảm bảo Bootstrap 5 và Font Awesome đã được include

### 3. Database
- Đảm bảo bảng `Tac_Gia` có các trường:
  - `ID_TAC_GIA` (int, primary key)
  - `TEN_TAC_GIA` (nvarchar)
  - `MO_TA` (nvarchar, nullable)

## Tính năng nâng cao

### 1. Validation
- **Client-side**: jQuery validation
- **Server-side**: Model validation
- **Custom**: Kiểm tra trùng lặp tên tác giả

### 2. Security
- **CSRF Protection**: AntiForgeryToken
- **Input Validation**: Sanitize input
- **Authorization**: Kiểm tra quyền truy cập

### 3. Performance
- **Pagination**: Giảm tải database
- **AJAX**: Tải dữ liệu không đồng bộ
- **Caching**: Cache dữ liệu thường dùng

### 4. UX/UI
- **Responsive Design**: Tương thích mobile
- **Loading States**: Hiển thị trạng thái tải
- **Error Handling**: Xử lý lỗi thân thiện
- **Accessibility**: Hỗ trợ screen reader

## API Endpoints

### GET /Admin/Tac_Gia
- **Parameters**: 
  - `searchString`: Từ khóa tìm kiếm
  - `sortOrder`: Thứ tự sắp xếp
  - `page`: Số trang
- **Response**: Danh sách tác giả phân trang

### POST /Admin/Tac_Gia/Create
- **Parameters**: Tac_Gia model
- **Response**: Redirect to Index hoặc validation errors

### GET /Admin/Tac_Gia/Edit/{id}
- **Parameters**: ID tác giả
- **Response**: Form chỉnh sửa

### POST /Admin/Tac_Gia/Edit/{id}
- **Parameters**: Tac_Gia model
- **Response**: Redirect to Index hoặc validation errors

### GET /Admin/Tac_Gia/Details/{id}
- **Parameters**: ID tác giả
- **Response**: Thông tin chi tiết

### GET /Admin/Tac_Gia/Delete/{id}
- **Parameters**: ID tác giả
- **Response**: Form xác nhận xóa

### POST /Admin/Tac_Gia/Delete/{id}
- **Parameters**: ID tác giả
- **Response**: Redirect to Index

### POST /Admin/Tac_Gia/CheckTacGiaExists
- **Parameters**: 
  - `tenTacGia`: Tên tác giả
  - `id`: ID tác giả (optional, cho Edit)
- **Response**: JSON { exists: boolean }

## Troubleshooting

### 1. Lỗi phân trang
- Kiểm tra package PagedList đã được cài đặt
- Đảm bảo `@using PagedList.Mvc` đã được thêm

### 2. Lỗi validation
- Kiểm tra jQuery validation script
- Đảm bảo model binding đúng

### 3. Lỗi AJAX
- Kiểm tra console browser
- Đảm bảo URL routing đúng

### 4. Lỗi CSS
- Kiểm tra Bootstrap CSS đã được load
- Đảm bảo Font Awesome icons

## Mở rộng tính năng

### 1. Export dữ liệu
- Thêm tính năng xuất Excel/PDF
- Sử dụng EPPlus hoặc iTextSharp

### 2. Import dữ liệu
- Thêm tính năng import từ file Excel
- Validation dữ liệu import

### 3. Thống kê
- Biểu đồ số lượng tác giả
- Thống kê sản phẩm theo tác giả

### 4. Tìm kiếm nâng cao
- Filter theo nhiều tiêu chí
- Tìm kiếm full-text

## Kết luận

Trang CRUD Tác giả mới cung cấp giao diện hiện đại, thân thiện với người dùng và các tính năng nâng cao. Code được tổ chức tốt, dễ bảo trì và mở rộng.

---

**Lưu ý**: Đảm bảo test kỹ các tính năng trước khi deploy lên production. 