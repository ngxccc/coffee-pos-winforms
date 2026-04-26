#### 1. Sơ đồ 1: Kiến trúc Lõi & Quản lý tiến trình nền (Core Architecture)

```mermaid
classDiagram
    class Program {
        +Main() void
        -CreateHostBuilder(connStr: string) IHostBuilder
    }

    class DependencyInjectionSetup {
        +AddCoffeePosServices(services: IServiceCollection) IServiceCollection
    }

    class AppStateManager {
        -_formFactory: IUiFactory
        -_session: IUserSession
        -ShowLoginForm() void
        -ShowMainWorkspace() void
        -OnFormClosed(sender, e) void
    }

    class IUiFactory {
        <<interface>>
        +CreateForm~TForm~(params object[]) TForm
        +CreateControl~TControl~(params object[]) TControl
    }

    class UiFactory {
        -_serviceProvider: IServiceProvider
        +CreateForm~TForm~(params object[]) TForm
        +CreateControl~TControl~(params object[]) TControl
    }

    class IUserSession {
        <<interface>>
        +Login(user)
        +Logout()
    }

    class PdfPrintWorker {
        -printQueue: PdfPrintQueue
        +ExecuteAsync(stoppingToken) Task
    }

    class TrashCleanupWorker {
        -dataSource: NpgsqlDataSource
        +ExecuteAsync(stoppingToken) Task
    }

    class PdfPrintQueue {
        -_queue: Channel~IPdfPayload~
        +EnqueueJobAsync(job: IPdfPayload) ValueTask
        +DequeueJobAsync(ct: CancellationToken) ValueTask~IPdfPayload~
        +StreamJobsAsync(ct: CancellationToken) IAsyncEnumerable~IPdfPayload~
    }

    class IPdfPayload {
        <<interface>>
    }

    class BillPrintPayload {
        +BillId: int
        +BuzzerNumber: int
        +TotalAmount: decimal
        +Details: List~BillDetailDto~
        +IsReprint: bool
    }

    class ShiftReportPrintPayload {
        +CashierName: string
        +StartTime: DateTime
        +EndTime: DateTime
        +TotalBills: int
        +ExpectedCash: decimal
        +ActualCash: decimal
        +Variance: decimal
        +Note: string
    }

    Program ..> DependencyInjectionSetup : Configures DI
    Program ..> AppStateManager : Bootstraps

    AppStateManager "1" --> "1" IUiFactory : uses
    AppStateManager "1" --> "1" IUserSession : uses
    UiFactory ..|> IUiFactory

    PdfPrintWorker "1" --> "1" PdfPrintQueue : consumes
    PdfPrintQueue "1" --> "0..*" IPdfPayload : queues
    BillPrintPayload ..|> IPdfPayload
    ShiftReportPrintPayload ..|> IPdfPayload
```

#### 2. Sơ đồ 2: Mô hình Đa tầng & CQRS (N-Layered & CQRS Pattern)

```mermaid
classDiagram
    class CashierWorkspaceForm {
        -_billService: IBillService
        -_billQueryService: IBillQueryService
        +ProcessPayment(sender, e) void
        +HandleBillHistoryClickedAsync(sender, e) void
    }

    class UC_Billing {
        +CartItems: List~CartItemDto~
        +TotalAmount: decimal
        +AddItem(item: CartItemDto) void
        +UpdateItem(oldItem: CartItemDto, newItem: CartItemDto) void
        +ClearCart() void
    }

    class IBillService {
        <<interface>>
        +CreateBillAsync(dto: CreateBillDto, ct) Task~int~
        +CancelBillAsync(billId: int, userId: int, reason: string) Task
        +RestoreBillAsync(billId: int) Task
    }

    class IBillQueryService {
        <<interface>>
        +GetBillDetailsAsync(billId: int) Task~List~BillDetailDto~~
        +GetBillsByDateRangeAsync(from: DateOnly, to: DateOnly) Task~List~BillReportDto~~
    }

    class BillService {
        -_billRepo: IBillRepository
        -_session: IUserSession
        +CreateBillAsync(dto: CreateBillDto, ct) Task~int~
        +CancelBillAsync(billId: int, userId: int, reason: string) Task
        +RestoreBillAsync(billId: int) Task
    }

    class BillQueryService {
        -_billRepo: IBillRepository
        +GetBillDetailsAsync(billId: int) Task~List~BillDetailDto~~
        +GetBillsByDateRangeAsync(from: DateOnly, to: DateOnly) Task~List~BillReportDto~~
    }

    class IBillRepository {
        <<interface>>
        +InsertBillAsync(dto: CreateBillDto, ct) Task~int~
        +GetBillDetailsAsync(billId: int) Task~List~BillDetailDto~~
        +GetBillReportsAsync(from: DateOnly, to: DateOnly) Task~List~BillReportDto~~
    }

    class BillRepository {
        -_dataSource: NpgsqlDataSource
        +InsertBillAsync(dto: CreateBillDto, ct) Task~int~
        +GetBillDetailsAsync(billId: int) Task~List~BillDetailDto~~
        +GetBillReportsAsync(from: DateOnly, to: DateOnly) Task~List~BillReportDto~~
    }

    class CreateBillDto
    class CreateBillItemDto
    class BillReportDto
    class BillDetailDto

    CashierWorkspaceForm "1" *-- "1" UC_Billing : composition

    UC_Billing "1" --> "1" IBillService : commands
    UC_Billing "1" --> "1" IBillQueryService : queries

    BillService ..|> IBillService
    BillQueryService ..|> IBillQueryService

    BillService "1" --> "1" IBillRepository : injects
    BillQueryService "1" --> "1" IBillRepository : injects

    BillRepository ..|> IBillRepository
    BillRepository "1" ..> "1" CreateBillDto : writes
    BillRepository "1" ..> "0..*" BillReportDto : reads
    BillRepository "1" ..> "0..*" BillDetailDto : reads

    CreateBillDto "1" --> "1..*" CreateBillItemDto : one_to_many
    BillReportDto "1" --> "0..*" BillDetailDto : one_to_many
```

#### 3. Sơ đồ 3: Cấu trúc Giao diện & Trạm làm việc (UI Composition)

```mermaid
classDiagram
    class CashierWorkspaceForm {
        -_ucSidebar: UC_Sidebar
        -_ucBilling: UC_Billing
        -_ucBillHistory: UC_BillHistory
        -_ucMenu: UC_Menu
        +HandleHomeClicked(sender, e) void
        +HandleBillHistoryClickedAsync(sender, e) void
        +HandleProductSelectedAsync(prodId, prodName, price, imageIdentifier) void
    }

    class AdminDashboardForm {
        -_ucSidebar: UC_Sidebar
        -_viewCache: Dictionary~string, UserControl~
        +HandleNavigation(routeTag: string) void
        +NavigateTo~T~(viewKey: string) void
    }

    class UC_Sidebar {
        +OnNavigate: Action~string~
        +OnHomeClicked: EventHandler
        +OnBillHistoryClicked: EventHandler
        +OnProfilesClicked: EventHandler
        +OnLogoutClicked: EventHandler
        +RenderNavigationTree() void
    }

    class UC_Billing {
        +OnPayClicked: EventHandler
        +OnEditCartItem: EventHandler~CartItemDto~
        +AddItem(item: CartItemDto) void
        +UpdateItem(oldItem: CartItemDto, newItem: CartItemDto) void
    }

    class UC_BillHistory {
        +OnReprintClicked: EventHandler~BillHistoryDto~
        +OnDetailsRequested: EventHandler~BillHistoryDto~
        +OnCancelRequested: EventHandler~BillHistoryDto~
        +OnRestoreRequested: EventHandler~BillHistoryDto~
        +LoadBillsAsync() Task
    }

    class UC_Menu {
        +OnProductSelected: Action~int, string, decimal, string~
        +LoadDataFromDatabaseAsync() Task
        +FilterProducts(categoryId: int) void
        +RenderProducts() void
    }

    class UC_PaymentConfirm {
        +TotalAmount: decimal
        +OnConfirmed: EventHandler
        +ConfirmPayment() void
    }

    class UC_Dashboard {
        +LoadDashboardAsync() Task
    }

    class UC_ManageProducts {
        +LoadProductsAsync() Task
        +HandleEditProduct(record) void
        +HandleDeleteProduct(record) void
    }

    class UC_ManageBills {
        +LoadBillsAsync() Task
        +HandleCancelBill(record) void
        +HandleRestoreBill(record) void
    }

    class UC_ManageUsers {
        +LoadUsersAsync() Task
        +HandleCreateUser() void
        +HandleResetPassword(record) void
    }

    class UC_Profiles {
        +GetPayload() ChangePasswordPayload
        +ValidateInput() bool
    }

    class IUiFactory {
        <<interface>>
        +CreateControl~TControl~(params object[]) TControl
    }

    CashierWorkspaceForm "1" *-- "1" UC_Sidebar
    CashierWorkspaceForm "1" *-- "1" UC_Billing
    CashierWorkspaceForm "1" *-- "1" UC_BillHistory
    CashierWorkspaceForm "1" *-- "1" UC_Menu
    UC_Billing "1" ..> "0..1" UC_PaymentConfirm : opens_modal

    AdminDashboardForm "1" *-- "1" UC_Sidebar
    AdminDashboardForm "1" *-- "1" UC_Dashboard
    AdminDashboardForm "1" *-- "1" UC_ManageProducts
    AdminDashboardForm "1" *-- "1" UC_ManageBills
    AdminDashboardForm "1" *-- "1" UC_ManageUsers

    AdminDashboardForm "1" --> "1" IUiFactory : dynamic_create
    UC_Sidebar "1" --> "0..*" CashierWorkspaceForm : raises_events_to
    UC_Sidebar "1" --> "0..*" AdminDashboardForm : raises_events_to
    UC_Profiles "1" ..> "1" AdminDashboardForm : shown_in_modal
```
