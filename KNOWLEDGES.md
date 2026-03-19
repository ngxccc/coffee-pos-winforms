## record keyword

"Sự vĩ đại của record nằm ở chỗ nó là một Kiểu dữ liệu tham chiếu bất biến (Immutable Reference Type) được thiết kế riêng cho việc truyền tải dữ liệu.

Value-based Equality (So sánh bằng giá trị): Nếu cậu tạo record A (Id=1) và record B (Id=1), thì A == B sẽ trả về true (Giống hệt Struct, nhưng lại nằm trên Heap như Class).

Non-destructive Mutation (Đột biến không phá hủy): Cậu không thể sửa giá trị của Record sau khi tạo. Nếu muốn đổi tên, cậu phải dùng từ khóa with để nhân bản ra một Record mới: var newDto = oldDto with { Name = "Tên Mới" };.

Positional Syntax (Cú pháp định vị): Trình biên dịch sẽ tự động đẻ ra Constructor, Properties (init-only), hàm ToString(), và hàm Equals() ngầm định ở dưới Background."
