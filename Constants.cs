namespace GreenStock;

public static class Strings
{
    // Common
    public const string Error = "Ошибка";
    public const string Warning = "Внимание";
    public const string Done = "Готово";
    public const string RequiredField = "Поле обязательно для заполнения";
    public const string ActionCannotBeUndone = "Это действие нельзя отменить.";

    // LoginForm
    public const string Login_Title = "Складской учет - Авторизация";
    public const string Login_AppTitle = "🌱 ГринСток";
    public const string Login_LabelLogin = "Логин:";
    public const string Login_LabelPassword = "Пароль:";
    public const string Login_BtnLogin = "ВОЙТИ";
    public const string Login_LinkRegister = "Зарегистрироваться";
    public const string Login_ErrInvalidCredentials = "Неверный логин или пароль";
    public const string Login_ErrEmptyFields = "Введите логин и пароль";
    public const string Login_ErrDbConnection = "Ошибка подключения к БД:";

    // RegisterForm
    public const string Register_Title = "Регистрация кладовщика";
    public const string Register_LabelLogin = "Логин:";
    public const string Register_LabelPassword = "Пароль:";
    public const string Register_LabelConfirm = "Подтвердите пароль:";
    public const string Register_LabelRole = "Роль:";
    public const string Register_RoleKladovshik = "Кладовщик";
    public const string Register_BtnRegister = "Зарегистрироваться";
    public const string Register_ErrLoginTaken = "Логин уже занят";
    public const string Register_ErrPasswordMismatch = "Пароли не совпадают";
    public static string Register_SuccessMsg(string login) => $"Регистрация успешна!\nВойдите под логином «{login}\".";

    // CatalogForm
    public static string Catalog_Title(string user, string role) => $"Каталог товаров — {user} ({role})";
    public const string Catalog_MenuCatalog = "Каталог";
    public const string Catalog_MenuCategories = "Категории";
    public const string Catalog_MenuShipments = "Отгрузки";
    public const string Catalog_MenuHistory = "История";
    public const string Catalog_MenuExit = "Выйти";
    public const string Catalog_LabelSearch = "Поиск:";
    public const string Catalog_LabelCategory = "Категория:";
    public const string Catalog_BtnAdd = "+ Добавить товар";
    public const string Catalog_BtnEdit = "Редактировать";
    public const string Catalog_BtnDelete = "✕ Удалить";
    public const string Catalog_AdminOnly = "недоступно для кладовщика";
    public const string Catalog_AllCategories = "Все";
    public static string Catalog_CountLabel(int count) => $"всего позиций: {count}";
    public const string Catalog_SelectProduct = "Выберите товар.";
    public const string Catalog_ErrLoading = "Ошибка загрузки:";
    public const string Catalog_ColArticle = "Артикул";
    public const string Catalog_ColName = "Название";
    public const string Catalog_ColUnit = "Ед. изм.";
    public const string Catalog_ColPrice = "Цена (руб.)";
    public const string Catalog_ColExpiry = "Срок годности";
    public const string Catalog_Perpetual = "Бессрочно";

    // CategoryForm
    public const string Category_Title = "Управление категориями";
    public const string Category_ListTitle = "Список категорий";
    public const string Category_InputTitle = "Название категории";
    public const string Category_BtnAdd = "+ Добавить";
    public const string Category_BtnRename = "Переименовать";
    public const string Category_BtnDelete = "✕  Удалить";
    public const string Category_ErrAlreadyExists = "Категория уже существует";
    public const string Category_ErrHasProducts = "Нельзя удалить: есть товары в этой категории";
    public static string Category_DeleteConfirm(string name) => $"категорию «{name}»";

    // ProductForm
    public const string Product_TitleAdd = "Добавить товар";
    public const string Product_TitleEdit = "Редактировать товар";
    public const string Product_LabelArticle = "Артикул*:";
    public const string Product_LabelName = "Название*:";
    public const string Product_LabelCategory = "Категория*:";
    public const string Product_LabelUnit = "Единица изм.:";
    public const string Product_LabelPrice = "Цена закупки:";
    public const string Product_LabelStock = "Количество:";
    public const string Product_LabelExpiry = "Срок годности:";
    public const string Product_ChkNoExpiry = "Бессрочно";
    public const string Product_RequiredHint = "*- обязательное поле";
    public const string Product_ErrArticleExists = "Артикул уже существует";
    public const string Product_Rub = "руб.";
    public const string Product_Pcs = "шт.";

    // ShipmentForm
    public const string Shipment_Title = "Новая отгрузка";
    public const string Shipment_LabelRecipient = "Получатель*:";
    public const string Shipment_GroupAdd = "добавить позицию";
    public const string Shipment_LabelProduct = "Товар:";
    public const string Shipment_LabelQty = "Количество:";
    public const string Shipment_LabelAvailable = "доступно на складе:";
    public const string Shipment_BtnAddRow = "+ Добавить строку";
    public const string Shipment_BtnConfirm = "Подтвердить";
    public static string Shipment_ErrInsufficientStock(string product, decimal requested, decimal available)
        => $"Недостаточно «{product}»: запрошено {requested}, в наличии {available}";
    public const string Shipment_ErrNoRows = "Добавьте хотя бы одну позицию";
    public const string Shipment_Success = "Отгрузка успешно оформлена!";

    // HistoryForm
    public const string History_Title = "История отгрузок";
    public const string History_LabelShipments = "Накладные";
    public static string History_LabelItems(int shipmentNumber) => $"Состав накладной №{shipmentNumber}";
    public const string History_ColDate = "Дата и Время";
    public const string History_ColWho = "Кто оформил";
    public const string History_ColRecipient = "Получатель";

    // DeleteConfirmForm
    public const string Delete_Title = "Подтверждение Удаления";
    public const string Delete_Question = "Вы действительно хотите удалить товар?";

    // UserRoles
    public const string Role_Admin = "Администратор";
    public const string Role_Kladovshik = "Кладовщик";

    // Common Buttons
    public const string Save = "Сохранить";
    public const string Cancel = "Отмена";
    public const string Back = "Назад";
    public const string Yes = "Да";
    public const string No = "Нет";

    // Supplies Form
    public const string Supplies_Title = "Поставка";
    public const string Supplies_LabelProduct = "Товар:";
    public const string Supplies_LabelQty = "Количество:";
    public const string Supplies_LabelPrice = "Цена закупки:";
    public const string Supplies_LabelExpiry = "Срок годности:";
    public const string Supplies_BtnAddManual = "Добавить в поставку";
    public const string Supplies_BtnCalculate = "Очистить";
    public const string Supplies_BtnImport = "Импортировать из файла";
    public const string Supplies_BtnSave = "Сохранить поставку";
    public const string Supplies_BtnDelete = "Удалить";
    public const string Supplies_TotalLabel = "Итого позиций:";
    public const string Supplies_TotalQty = "Общее количество:";

    // Reports Form
    public const string Reports_Title = "Отчет";
    public const string Reports_LabelFrom = "с";
    public const string Reports_LabelTo = "по";
    public const string Reports_BtnGenerate = "Сформировать";
    public const string Reports_ColDate = "Дата";
    public const string Reports_ColCustomer = "Покупатель";
    public const string Reports_ColSum = "Сумма";
    public const string Reports_ColProfit = "Прибыль";
    public const string Reports_BtnExport = "Экспорт";
    public const string Reports_Total = "Итого:";

    // Currency Settings
    public const string Currency_Title = "Валюта";
    public const string Currency_RUB = "РУБ (₽)";
    public const string Currency_USD = "USD ($)";
    public const string Currency_EUR = "EUR (€)";
    public const string Currency_Rub = "руб.";
    public const string Currency_Symbol = "₽";
}
