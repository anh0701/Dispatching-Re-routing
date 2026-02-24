# URD

## Mối liên kết giữa Quy trình (Route) và Nguồn lực (Resource)

- Bảng `Routes` & `Route_Steps`: Định nghĩa một sản phẩm phải đi qua những bước nào (Ví dụ: Bước 1: Cắt -> Bước 2: Hàn -> Bước 3: Sơn).

- Bảng `Work_Centers` (Máy móc/Tổ đội): Chứa thông tin máy, nhưng quan trọng nhất là cột `Status` (Running, Standby, Breakdown) và `Capacity`.

- Bảng `Work_Orders` (Lệnh sản xuất): Trạng thái của lệnh (`Planned`, `In-Progress`, `On-Hold`, `Completed`).

- Bảng `Machine_Capabilities` (Quan trọng): Bảng trung gian định nghĩa Máy A có thể làm được Bước 1 và Bước 2. Đây là chìa khóa để "chuyển tuyến" khi máy hỏng.

## Logic nghiệp vụ "Re-routing"

Khi một máy đang chạy bị Breakdown, hệ thống của bạn không chỉ đổi màu cái icon trên màn hình, mà phải thực hiện chuỗi logic sau:

- `Freeze` (Đóng băng): Tạm dừng tất cả các Work_Orders đang gán cho máy đó.

- `Lookup Alternative` (Tìm máy thay thế): Truy vấn bảng `Machine_Capabilities` để tìm các máy khác có cùng khả năng xử lý công đoạn đó.

- `Conflict Check` (Kiểm tra xung đột): Kiểm tra xem máy thay thế có đang bận làm đơn hàng gấp hơn không (Dựa trên `Priority` hoặc `Due Date`).

- `Re-assign & Recalculate`: Đẩy lệnh sang máy mới và tính toán lại thời gian kết thúc dự kiến (ETD). Công thức tính ETD đơn giản:

    `ETD = Current_Time + (Machine_Speed / Remaining_Quantity)​ + Setup_Time`

## Thiết kế Database

- WorkCenters (Danh sách máy móc/dây chuyền): Id, Code, Name, Status (0: Offline, 1: Running, 2: Breakdown, 3: Maintenance)

- Operations (Các công đoạn sản xuất (Ví dụ: Cắt, Hàn, Sơn, Lắp ráp)): Id, OpCode, Name

- ResourceCapabilities (Định nghĩa máy nào làm được công đoạn nào): WorkCenterId, OperationId, CycleTime (Thời gian tiêu chuẩn để làm 1 sản phẩm)

- ProductionRoutes (Quy trình mẫu cho một sản phẩm): Id, ProductCode, Version

- RouteSteps (Các bước trong quy trình đó): RouteId, StepOrder (Thứ tự 1, 2, 3...), OperationId

- WorkOrders (Lệnh sản xuất): Id, OrderNo, RouteId, Quantity, Priority (1-10), DueDate, Status

- DispatchingBoard (Bảng kế hoạch chi tiết (Gantt Chart data)): Id, WorkOrderId, WorkCenterId, StepOrder, ScheduledStart, ScheduledEnd, ActualStart, ActualEnd, Status (Pending, Processing, Completed, Interrupted)

## Logic xử lý "Re-routing" khi máy hỏng

- Event: Máy A bị hỏng (Status = 2).

- Find Affected: Tìm tất cả DispatchingBoard item có WorkCenterId = A và Status != 'Completed'.

- Search Candidate: Với mỗi item bị ảnh hưởng, tìm máy B trong ResourceCapabilities có cùng OperationId và Status == 1 (Running/Ready).

- Cost Calculation: Tính toán xem nếu chuyển sang B thì ScheduledEnd mới là bao nhiêu (Dựa trên CycleTime của máy B).

- Re-dispatch: Cập nhật WorkCenterId mới cho item đó.

## API Overview

### Đơn hàng

| Method | Endpoint | Mục đích |
|--------|----------|----------|
| GET | `/api/work-orders/pending` | Lấy các lệnh chưa được xếp lịch để hiện ở thanh **Waiting List**. |
| GET | `/api/work-orders/{id}/route-details` | Xem quy trình và các máy có thể làm. |

---

### Điều phối

| Method | Endpoint | Mục đích |
|--------|----------|----------|
| GET | `/api/dispatching-board` | Lấy dữ liệu để vẽ lên bảng Gantt/Board. |
| POST | `/api/dispatching-board/assign` | Kéo đơn hàng thả vào máy. |
| DELETE | `/api/dispatching-board/{id}` | Hủy phân công, đưa task về lại danh sách chờ. |

---

### Sản xuất

| Method | Endpoint | Mục đích |
|--------|----------|----------|
| PATCH | `/api/dispatching-board/{id}/start` | Công nhân bấm **Bắt đầu**. |
| PATCH | `/api/dispatching-board/{id}/report` | Cập nhật số lượng đã làm được (ActualQuantity). |
| PATCH | `/api/dispatching-board/{id}/complete` | Hoàn thành công đoạn. |

---

### Máy móc

| Method | Endpoint | Mục đích |
|--------|----------|----------|
| GET | `/api/work-centers` | Xem danh sách máy và trạng thái (Xanh/Đỏ/Vàng). |
