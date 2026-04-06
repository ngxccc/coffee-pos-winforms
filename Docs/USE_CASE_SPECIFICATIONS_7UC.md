# CoffeePOS - Đặc tả Use Case rút gọn (7 Use Case)

## Phạm vi

Tài liệu này tái cấu trúc danh sách use case từ 31 xuống 7 use case cấp cao, phù hợp cho nộp học thuật.
Các tác vụ nền như in PDF hàng đợi và dọn dữ liệu nền được chuyển sang mục Yêu cầu phi chức năng, không nằm trong danh sách Use Case nghiệp vụ.

## Danh sách 7 Use Case

| Use Case ID | Tên Use Case | Tác nhân chính | Mục tiêu ngắn |
| --- | --- | --- | --- |
| UC-01 | Quản lý Phiên làm việc (Session Management) | Thu ngân, Quản trị viên | Quản lý đăng nhập, đổi mật khẩu, đăng xuất |
| UC-02 | Xử lý Giao dịch Bán hàng (Point-of-Sale Checkout) | Thu ngân | Hoàn tất quy trình bán hàng từ chọn món tới tạo bill |
| UC-03 | Quản lý Ca làm việc (Shift Management) | Thu ngân | Xem bill ca, chốt ca, in báo cáo ca |
| UC-04 | Quản trị Danh mục Bán hàng (Catalog Management) | Quản trị viên | Quản trị Product/Category theo vòng đời dữ liệu |
| UC-05 | Quản trị Tài nguyên Nhân sự (Human Resource Management) | Quản trị viên | Thêm, sửa, khóa/mở tài khoản người dùng |
| UC-06 | Xử lý Sự cố Hóa đơn (Bill Exception Handling) | Quản trị viên | Tìm kiếm, xem chi tiết, hủy/khôi phục hóa đơn |
| UC-07 | Phân tích Dữ liệu Hệ thống (Dashboard & Analytics) | Quản trị viên | KPI, biểu đồ, top sản phẩm, xuất CSV |

---

## UC-01

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-01 |
| Tên Use Case | Quản lý Phiên làm việc (Session Management) |
| Tác nhân | Thu ngân, Quản trị viên |
| Mục tiêu | Quản lý vòng đời phiên người dùng gồm đăng nhập, đăng xuất và đổi mật khẩu. |
| Điều kiện tiên quyết (Preconditions) | 1) Ứng dụng đã khởi động thành công và kết nối CSDL khả dụng.<br>2) Người dùng có tài khoản hợp lệ trong hệ thống. |
| Luồng sự kiện chính (Main Success Scenario) | 1) Người dùng mở màn hình đăng nhập và nhập tài khoản, mật khẩu.<br>2) Hệ thống xác thực thông tin đăng nhập.<br>3) Nếu hợp lệ, hệ thống mở workspace theo vai trò người dùng.<br>4) Trong phiên, người dùng vào Cài đặt để đổi mật khẩu.<br>5) Hệ thống cập nhật mật khẩu đã băm vào CSDL.<br>6) Người dùng đăng xuất, hệ thống kết thúc phiên và quay về màn hình đăng nhập. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [2a] Sai thông tin đăng nhập: Từ chối truy cập, hiển thị lỗi.<br>[2b] Thiếu Username/Password: Từ chối xác thực, yêu cầu nhập đủ dữ liệu.<br>[4a] Mật khẩu mới không hợp lệ: Từ chối đổi mật khẩu nếu không đủ độ dài hoặc xác nhận không khớp.<br>[6a] Đăng xuất khi còn đơn chưa thanh toán: Chặn thao tác và yêu cầu xử lý giỏ hàng trước. |
| Truy vết code (File/Line) | Đăng nhập/điều hướng: Forms/LoginForm.cs:137; Services/UserService.cs:9-14; Core/AppStateManager.cs:10-18,53-61.<br>Đổi mật khẩu: Forms/SettingForm.cs:104,179-190; Services/UserService.cs:38-51; Data/Repositories/UserRepository.cs:135.<br>Đăng xuất: Forms/CashierWorkspaceForm.cs:122-141. |
| Ghi chú chưa làm | Chưa thấy cơ chế timeout phiên tự động (session expiration) ở tầng ứng dụng. |

---

## UC-02 (Core Use Case)

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-02 |
| Tên Use Case | Xử lý Giao dịch Bán hàng (Point-of-Sale Checkout) |
| Tác nhân | Thu ngân (Cashier) |
| Mục tiêu | Cho phép Thu ngân tạo đơn hàng từ yêu cầu của khách và ghi nhận thanh toán thành công vào hệ thống. |
| Điều kiện tiên quyết (Preconditions) | 1) Thu ngân đã được xác thực vào hệ thống.<br>2) Hệ thống có sẵn ít nhất một sản phẩm đang hoạt động.<br>3) Màn hình làm việc thu ngân đã tải đầy đủ module Menu, Billing, Bill History. |
| Luồng sự kiện chính (Main Success Scenario) | 1) Thu ngân duyệt danh mục và chọn sản phẩm khách yêu cầu.<br>2) Hệ thống thêm sản phẩm vào giỏ hàng ảo và tính tổng tạm tính.<br>3) Thu ngân điều chỉnh số lượng hoặc loại bỏ sản phẩm lỗi.<br>4) Thu ngân xác nhận thanh toán và nhập số thẻ rung/số thứ tự đơn.<br>5) Hệ thống kiểm tra tính hợp lệ dữ liệu đơn hàng trước khi lưu.<br>6) Hệ thống khởi tạo hóa đơn (Bill) và chi tiết hóa đơn (BillDetails) trong CSDL theo giao dịch nghiệp vụ.<br>7) Hệ thống kích hoạt in hóa đơn theo hàng đợi in PDF.<br>8) Hệ thống reset giỏ hàng, kết thúc giao dịch. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [2a] Giỏ hàng trống: Từ chối thanh toán và cảnh báo.<br>[4a] Dữ liệu đầu vào không hợp lệ: Nếu số thẻ rung không phải số nguyên, hủy thao tác tạo đơn.<br>[5a] Người dùng hủy xác nhận thanh toán: Dừng quy trình, giữ nguyên giỏ hàng.<br>[6a] Ngoại lệ lưu giao dịch thất bại: Thông báo lỗi và giữ nguyên giỏ hàng để xử lý lại.<br>[7a] Ngoại lệ in hóa đơn: Vẫn ưu tiên giữ kết quả thanh toán thành công, ghi lỗi/in lại sau. |
| Hậu điều kiện (Postconditions) | 1) Hóa đơn hợp lệ đã được ghi nhận trong hệ thống.<br>2) Giỏ hàng trên UI được làm sạch và sẵn sàng cho giao dịch mới.<br>3) Lệnh in hóa đơn được đưa vào pipeline in (nếu module in hoạt động). |
| Truy vết code (File/Line) | Hiển thị menu/danh mục: Features/Products/UC_Menu.cs:80-81; Services/ProductQueryService.cs:9; Services/CategoryQueryService.cs:9-12.<br>Chọn món/thêm giỏ: Forms/CashierWorkspaceForm.cs:218-221; Features/Billing/UC_Billing.cs:145.<br>Kiểm tra giỏ/xử lý thanh toán: Forms/CashierWorkspaceForm.cs:227-264.<br>Tạo bill: Forms/CashierWorkspaceForm.cs:266-272; Services/BillService.cs:10-21; Data/Repositories/BillRepository.cs:18-31.<br>In hóa đơn queue: Forms/CashierWorkspaceForm.cs:273-283; Core/PdfPrintQueue.cs:5-9; Core/PdfPrintWorker.cs:7,19; Core/InvoiceGenerator.cs:11. |
| Ghi chú chưa làm | Chưa thấy bước nhập tiền khách đưa và tính tiền thối trong flow hiện tại (mới xác nhận tổng tiền).<br>Chưa thấy logic trừ tồn kho trong transaction bán hàng (nếu bài toán có quản lý kho). |

---

## UC-03

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-03 |
| Tên Use Case | Quản lý Ca làm việc (Shift Management) |
| Tác nhân | Thu ngân |
| Mục tiêu | Quản lý cuối ca gồm xem hóa đơn ca, chốt ca và in báo cáo ca. |
| Điều kiện tiên quyết (Preconditions) | 1) Thu ngân đang ở phiên làm việc hợp lệ.<br>2) Có thời điểm bắt đầu ca (lấy từ login time của session). |
| Luồng sự kiện chính (Main Success Scenario) | 1) Thu ngân mở lịch sử hóa đơn ca để theo dõi giao dịch trong ngày.<br>2) Thu ngân chọn thao tác đăng xuất/chốt ca.<br>3) Hệ thống mở màn hình báo cáo ca, tính tổng đơn và doanh thu kỳ ca.<br>4) Thu ngân nhập tiền mặt kiểm kê và xác nhận lưu báo cáo ca.<br>5) Hệ thống lưu báo cáo ca vào CSDL.<br>6) Hệ thống gửi yêu cầu tạo/in báo cáo ca theo hàng đợi in.<br>7) Hệ thống kết thúc phiên làm việc. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [1a] Không có hóa đơn trong ca: Hiển thị danh sách rỗng, vẫn cho phép chốt ca.<br>[4a] Dữ liệu chốt ca không hợp lệ: Từ chối lưu báo cáo và yêu cầu nhập lại.<br>[5a] Lỗi lưu báo cáo ca: Giữ form hiện tại, thông báo lỗi và chưa kết thúc phiên. |
| Truy vết code (File/Line) | Xem bill theo user: Forms/CashierWorkspaceForm.cs:109-114.<br>Điều hướng chốt ca: Forms/CashierWorkspaceForm.cs:122-141.<br>Tính tổng/lưu báo cáo: Forms/ShiftReportForm.cs:147,200-210; Services/ShiftReportQueryService.cs:10; Services/ShiftReportService.cs:9-18; Data/Repositories/ShiftReportRepository.cs:33-37.<br>In báo cáo ca: Forms/ShiftReportForm.cs:212; Core/InvoiceGenerator.cs:35,139. |
| Ghi chú chưa làm | Chưa thấy chính sách khóa chốt ca lặp (double close) ở mức nghiệp vụ. |

---

## UC-04

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-04 |
| Tên Use Case | Quản trị Danh mục Bán hàng (Catalog Management) |
| Tác nhân | Quản trị viên |
| Mục tiêu | Quản trị Product và Category theo vòng đời thêm, sửa, xóa mềm, khôi phục. |
| Điều kiện tiên quyết (Preconditions) | Người dùng có vai trò quản trị. |
| Luồng sự kiện chính (Main Success Scenario) | 1) Mở màn hình quản lý sản phẩm hoặc danh mục.<br>2) Thêm mới dữ liệu.<br>3) Sửa dữ liệu hiện có.<br>4) Xóa mềm dữ liệu không còn sử dụng.<br>5) Chuyển sang chế độ Thùng rác và khôi phục dữ liệu khi cần.<br>6) Hệ thống cập nhật danh sách theo trạng thái hoạt động/xóa. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [2a] Trùng tên khi thêm mới: Từ chối thao tác, cảnh báo dữ liệu trùng.<br>[3a] Bản ghi không tồn tại: Thông báo lỗi và yêu cầu tải lại dữ liệu.<br>[4a] Vi phạm ràng buộc nghiệp vụ: Chặn xóa và hiển thị cảnh báo. |
| Truy vết code (File/Line) | Product: Features/Admin/UC_ManageProducts.cs:43-45,87-110,123,127-143; Services/ProductService.cs:9-18,21-30,33-36,46-58; Data/Repositories/ProductRepository.cs:46,59,73,110.<br>Category: Features/Admin/UC_ManageCategories.cs:37-39,84-90,92-103,105-129; Services/CategoryService.cs:9-18,21-31,34-37,40-51; Data/Repositories/CategoryRepository.cs:47,57,68,121. |
| Ghi chú chưa làm | Chưa thấy cơ chế audit trail chi tiết (ai sửa gì, lúc nào) ngoài timestamp mức dữ liệu. |

---

## UC-05

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-05 |
| Tên Use Case | Quản trị Tài nguyên Nhân sự (Human Resource Management) |
| Tác nhân | Quản trị viên |
| Mục tiêu | Quản lý tài khoản nhân sự gồm thêm mới, cập nhật thông tin và khóa/mở khóa tài khoản. |
| Điều kiện tiên quyết (Preconditions) | Người dùng thao tác có quyền Admin. |
| Luồng sự kiện chính (Main Success Scenario) | 1) Mở màn hình quản lý nhân sự.<br>2) Thêm người dùng mới (username, fullname, role, password).<br>3) Cập nhật thông tin tài khoản và mật khẩu (nếu cần).<br>4) Khóa hoặc mở khóa tài khoản theo trạng thái vận hành.<br>5) Hệ thống cập nhật dữ liệu và tải lại danh sách người dùng. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [2a] Dữ liệu đầu vào không hợp lệ: Thiếu trường, role sai, mật khẩu yếu.<br>[2b] Trùng username: Từ chối tạo tài khoản.<br>[4a] Tự khóa tài khoản admin hiện tại: Từ chối thao tác theo luật nghiệp vụ. |
| Truy vết code (File/Line) | Features/Admin/UC_ManageUsers.cs:39-42,70-95,97-131,133-173; Services/UserService.cs:16-35,53-93,95-105; Data/Repositories/UserRepository.cs:76,118,135. |
| Ghi chú chưa làm | Chưa thấy flow phân quyền chi tiết theo nhóm quyền con (permission-based), hiện chủ yếu theo role. |

---

## UC-06

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-06 |
| Tên Use Case | Xử lý Sự cố Hóa đơn (Bill Exception Handling) |
| Tác nhân | Quản trị viên |
| Mục tiêu | Tìm kiếm hóa đơn lịch sử, hủy/khôi phục hóa đơn sai sót và xem chi tiết để đối soát. |
| Điều kiện tiên quyết (Preconditions) | Người dùng có quyền quản trị hóa đơn. |
| Luồng sự kiện chính (Main Success Scenario) | 1) Lọc dữ liệu theo khoảng ngày.<br>2) Hệ thống tải danh sách hóa đơn và thống kê tổng hợp.<br>3) Xem chi tiết hóa đơn nghi vấn.<br>4) Thực hiện hủy hoặc khôi phục hóa đơn.<br>5) Hệ thống cập nhật trạng thái hóa đơn và làm mới danh sách. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [1a] Khoảng ngày không hợp lệ: Từ chối truy vấn.<br>[3a] Hóa đơn không có chi tiết: Cảnh báo và không mở form chi tiết.<br>[4a] Người dùng hủy thao tác xác nhận: Không thay đổi dữ liệu.<br>[4b] Lỗi cập nhật trạng thái: Báo lỗi, giữ nguyên trạng thái hiển thị cũ. |
| Truy vết code (File/Line) | Features/Admin/UC_ManageBills.cs:52,71-80,127-141,143-168,170-215; Services/BillService.cs:24,26; Services/BillQueryService.cs:12-15,18-26; Data/Repositories/BillRepository.cs:61,84,92,120-125. |
| Ghi chú chưa làm | Chưa thấy trạng thái workflow nhiều mức (Pending Review, Approved Cancel, v.v.), hiện là hủy/khôi phục trực tiếp. |

---

## UC-07

| Thành phần | Nội dung |
| --- | --- |
| Use Case ID | UC-07 |
| Tên Use Case | Phân tích Dữ liệu Hệ thống (Dashboard & Analytics) |
| Tác nhân | Quản trị viên |
| Mục tiêu | Cung cấp dashboard vận hành gồm KPI, biểu đồ xu hướng, top sản phẩm và xuất báo cáo CSV. |
| Điều kiện tiên quyết (Preconditions) | Có dữ liệu hóa đơn trong hệ thống. |
| Luồng sự kiện chính (Main Success Scenario) | 1) Mở Dashboard.<br>2) Hệ thống tải song song dữ liệu KPI, biểu đồ doanh thu và top sản phẩm.<br>3) Hiển thị dữ liệu trực quan trên dashboard.<br>4) Mở màn hình quản lý hóa đơn để xuất báo cáo CSV theo bộ lọc ngày.<br>5) Sinh file CSV và lưu theo vị trí người dùng chọn. |
| Luồng thay thế/Ngoại lệ (Alternative/Exception Flows) | [2a] Lỗi truy vấn dữ liệu dashboard: Hiển thị lỗi tải dashboard.<br>[4a] Không có dữ liệu để xuất: Cảnh báo và không tạo file.<br>[5a] Lỗi ghi file: Hiển thị lỗi I/O và yêu cầu thực hiện lại. |
| Truy vết code (File/Line) | Dashboard: Features/Admin/UC_Dashboard.cs:23,112-118,122-157; Services/DashboardQueryService.cs:14-15; Data/Repositories/DashboardRepository.cs:56.<br>Xuất CSV: Features/Admin/UC_ManageBills.cs:22,230-274; Features/Admin/Controls/UC_BillsHeaderToolbar.cs:15,91,101. |
| Ghi chú chưa làm | Chưa thấy lịch trình xuất báo cáo tự động định kỳ (scheduled export) trong ứng dụng. |

---

## Non-functional Requirements - Yêu cầu phi chức năng

Các nội dung sau là yêu cầu phi chức năng, không thuộc danh sách Use Case nghiệp vụ:

1. Xử lý nền in PDF theo hàng đợi

- Mục tiêu: Tăng độ ổn định và tách luồng in khỏi luồng giao dịch UI.
- Bằng chứng code:
  - Program.cs:78-81
  - Core/PdfPrintQueue.cs:5-9
  - Core/PdfPrintWorker.cs:7, 19
  - Core/InvoiceGenerator.cs:11

1. Dọn dữ liệu nền theo lịch

- Mục tiêu: Duy trì hiệu năng và vệ sinh dữ liệu dài hạn.
- Bằng chứng code:
  - Program.cs:80-81
  - Core/TrashCleanupWorker.cs:7

1. Hiệu năng và trải nghiệm

- Truy vấn bất đồng bộ ở tầng Service/Repository.
- Tải dữ liệu dashboard song song (Task.WhenAll).
- Cơ chế lọc/sắp xếp stateful trên lưới dữ liệu.

1. Bảo mật

- Mật khẩu được băm BCrypt trước khi lưu.
- Ràng buộc không cho admin tự khóa chính mình.

1. Độ tin cậy và khả năng quan sát

- Xử lý ngoại lệ và thông báo người dùng ở các điểm nghiệp vụ chính.
- Có logging ứng dụng qua Serilog trong vòng đời khởi động/chạy.

---

## Bản đồ gộp Use Case (31 -> 7)

- UC-01 gộp: Đăng nhập, Đăng xuất, Đổi mật khẩu
- UC-02 gộp: Xem menu, Thêm vào giỏ, Quản lý giỏ, Thanh toán
- UC-03 gộp: Xem bill ca, Chốt ca, In báo cáo ca
- UC-04 gộp: Thêm/Sửa/Xóa/Khôi phục Product và Category
- UC-05 gộp: Thêm/Sửa/Khóa User
- UC-06 gộp: Tìm kiếm, Hủy, Khôi phục hóa đơn lịch sử
- UC-07 gộp: KPI, Chart, Top món, Xuất CSV

Ngày cập nhật: 2026-03-30
