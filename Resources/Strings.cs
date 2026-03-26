using System.Globalization;
using System.Resources;

namespace GreenStock.Resources;

/// <summary>
/// Предоставляет типобезопасный доступ к строкам локализации.
/// По умолчанию используется русский язык (ru).
/// </summary>
public static class Strings
{
    private static ResourceManager? _manager;

    /// <summary>
    /// Менеджер ресурсов для текущей культуры.
    /// </summary>
    private static ResourceManager Manager =>
        _manager ??= new ResourceManager(
            "GreenStock.Resources.Strings.ru",
            typeof(Strings).Assembly);

    /// <summary>
    /// Возвращает локализованную строку по ключу.
    /// Если ключ не найден — возвращает сам ключ.
    /// </summary>
    /// <param name="key">Ключ ресурса.</param>
    public static string Get(string key) =>
        Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    /// <summary>
    /// Возвращает форматированную локализованную строку.
    /// </summary>
    /// <param name="key">Ключ ресурса.</param>
    /// <param name="args">Аргументы форматирования.</param>
    public static string Get(string key, params object[] args) =>
        string.Format(Get(key), args);

    // ── Common ──────────────────────────────────────────────────────────────────

    /// <summary>Заголовок диалога ошибки.</summary>
    public static string Error            => Get("Error");
    /// <summary>Заголовок диалога предупреждения.</summary>
    public static string Warning          => Get("Warning");
    /// <summary>Заголовок успешного завершения.</summary>
    public static string Done             => Get("Done");
    /// <summary>Подсказка о необратимости действия.</summary>
    public static string ActionCannotBeUndone => Get("ActionCannotBeUndone");
    /// <summary>Сообщение об обязательном поле.</summary>
    public static string RequiredField    => Get("RequiredField");

    // ── LoginForm ────────────────────────────────────────────────────────────────

    /// <summary>Заголовок формы входа.</summary>
    public static string Login_Title              => Get("Login_Title");
    /// <summary>Название приложения на форме входа.</summary>
    public static string Login_AppTitle           => Get("Login_AppTitle");
    /// <summary>Метка поля логина.</summary>
    public static string Login_LabelLogin         => Get("Login_LabelLogin");
    /// <summary>Метка поля пароля.</summary>
    public static string Login_LabelPassword      => Get("Login_LabelPassword");
    /// <summary>Текст кнопки входа.</summary>
    public static string Login_BtnLogin           => Get("Login_BtnLogin");
    /// <summary>Текст ссылки на регистрацию.</summary>
    public static string Login_LinkRegister       => Get("Login_LinkRegister");
    /// <summary>Сообщение при неверных учётных данных.</summary>
    public static string Login_ErrInvalidCredentials => Get("Login_ErrInvalidCredentials");
    /// <summary>Сообщение при незаполненных полях.</summary>
    public static string Login_ErrEmptyFields     => Get("Login_ErrEmptyFields");
    /// <summary>Сообщение об ошибке подключения к БД.</summary>
    public static string Login_ErrDbConnection    => Get("Login_ErrDbConnection");

    // ── RegisterForm ─────────────────────────────────────────────────────────────

    /// <summary>Заголовок формы регистрации.</summary>
    public static string Register_Title              => Get("Register_Title");
    /// <summary>Метка поля логина.</summary>
    public static string Register_LabelLogin         => Get("Register_LabelLogin");
    /// <summary>Метка поля пароля.</summary>
    public static string Register_LabelPassword      => Get("Register_LabelPassword");
    /// <summary>Метка поля подтверждения пароля.</summary>
    public static string Register_LabelConfirm       => Get("Register_LabelConfirm");
    /// <summary>Метка поля роли.</summary>
    public static string Register_LabelRole          => Get("Register_LabelRole");
    /// <summary>Название роли кладовщика.</summary>
    public static string Register_RoleKladovshik     => Get("Register_RoleKladovshik");
    /// <summary>Текст кнопки регистрации.</summary>
    public static string Register_BtnRegister        => Get("Register_BtnRegister");
    /// <summary>Ошибка занятого логина.</summary>
    public static string Register_ErrLoginTaken      => Get("Register_ErrLoginTaken");
    /// <summary>Ошибка несовпадения паролей.</summary>
    public static string Register_ErrPasswordMismatch => Get("Register_ErrPasswordMismatch");
    /// <summary>Сообщение об успешной регистрации. Параметр {0}: логин.</summary>
    public static string Register_SuccessMsg(string login) => Get("Register_SuccessMsg", login);

    // ── CatalogForm ──────────────────────────────────────────────────────────────

    /// <summary>Заголовок каталога. {0}: логин, {1}: роль.</summary>
    public static string Catalog_Title(string login, string role) => Get("Catalog_Title", login, role);
    /// <summary>Пункт меню "Каталог".</summary>
    public static string Catalog_MenuCatalog    => Get("Catalog_MenuCatalog");
    /// <summary>Пункт меню "Категории".</summary>
    public static string Catalog_MenuCategories => Get("Catalog_MenuCategories");
    /// <summary>Пункт меню "Отгрузки".</summary>
    public static string Catalog_MenuShipments  => Get("Catalog_MenuShipments");
    /// <summary>Пункт меню "История".</summary>
    public static string Catalog_MenuHistory    => Get("Catalog_MenuHistory");
    /// <summary>Пункт меню "Выйти".</summary>
    public static string Catalog_MenuExit       => Get("Catalog_MenuExit");
    /// <summary>Метка поля поиска.</summary>
    public static string Catalog_LabelSearch    => Get("Catalog_LabelSearch");
    /// <summary>Метка фильтра по категории.</summary>
    public static string Catalog_LabelCategory  => Get("Catalog_LabelCategory");
    /// <summary>Текст кнопки добавления товара.</summary>
    public static string Catalog_BtnAdd         => Get("Catalog_BtnAdd");
    /// <summary>Текст кнопки редактирования.</summary>
    public static string Catalog_BtnEdit        => Get("Catalog_BtnEdit");
    /// <summary>Текст кнопки удаления.</summary>
    public static string Catalog_BtnDelete      => Get("Catalog_BtnDelete");
    /// <summary>Метка "только для администратора".</summary>
    public static string Catalog_AdminOnly      => Get("Catalog_AdminOnly");
    /// <summary>Элемент "Все" в списке категорий.</summary>
    public static string Catalog_AllCategories  => Get("Catalog_AllCategories");
    /// <summary>Счётчик позиций. {0}: количество.</summary>
    public static string Catalog_CountLabel(int count) => Get("Catalog_CountLabel", count);
    /// <summary>Подсказка выбора товара.</summary>
    public static string Catalog_SelectProduct  => Get("Catalog_SelectProduct");
    /// <summary>Сообщение об ошибке загрузки.</summary>
    public static string Catalog_ErrLoading     => Get("Catalog_ErrLoading");
    /// <summary>Заголовок столбца "Артикул".</summary>
    public static string Catalog_ColArticle     => Get("Catalog_ColArticle");
    /// <summary>Заголовок столбца "Название".</summary>
    public static string Catalog_ColName        => Get("Catalog_ColName");
    /// <summary>Заголовок столбца "Категория".</summary>
    public static string Catalog_ColCategory    => Get("Catalog_ColCategory");
    /// <summary>Заголовок столбца "Ед. изм.".</summary>
    public static string Catalog_ColUnit        => Get("Catalog_ColUnit");
    /// <summary>Заголовок столбца цены.</summary>
    public static string Catalog_ColPrice       => Get("Catalog_ColPrice");
    /// <summary>Заголовок столбца остатка.</summary>
    public static string Catalog_ColStock       => Get("Catalog_ColStock");
    /// <summary>Заголовок столбца срока годности.</summary>
    public static string Catalog_ColExpiry      => Get("Catalog_ColExpiry");
    /// <summary>Значение "Бессрочно" в столбце срока годности.</summary>
    public static string Catalog_Perpetual      => Get("Catalog_Perpetual");

    // ── CategoryForm ─────────────────────────────────────────────────────────────

    /// <summary>Заголовок формы категорий.</summary>
    public static string Category_Title          => Get("Category_Title");
    /// <summary>Подзаголовок списка категорий.</summary>
    public static string Category_ListTitle      => Get("Category_ListTitle");
    /// <summary>Подзаголовок поля ввода.</summary>
    public static string Category_InputTitle     => Get("Category_InputTitle");
    /// <summary>Текст кнопки добавления.</summary>
    public static string Category_BtnAdd         => Get("Category_BtnAdd");
    /// <summary>Текст кнопки переименования.</summary>
    public static string Category_BtnRename      => Get("Category_BtnRename");
    /// <summary>Текст кнопки удаления.</summary>
    public static string Category_BtnDelete      => Get("Category_BtnDelete");
    /// <summary>Ошибка дублирующейся категории.</summary>
    public static string Category_ErrAlreadyExists => Get("Category_ErrAlreadyExists");
    /// <summary>Ошибка удаления категории с товарами.</summary>
    public static string Category_ErrHasProducts => Get("Category_ErrHasProducts");
    /// <summary>Описание удаляемой категории. {0}: название.</summary>
    public static string Category_DeleteConfirm(string name) => Get("Category_DeleteConfirm", name);

    // ── ProductForm ──────────────────────────────────────────────────────────────

    /// <summary>Заголовок формы добавления товара.</summary>
    public static string Product_TitleAdd      => Get("Product_TitleAdd");
    /// <summary>Заголовок формы редактирования товара.</summary>
    public static string Product_TitleEdit     => Get("Product_TitleEdit");
    /// <summary>Метка поля артикула.</summary>
    public static string Product_LabelArticle  => Get("Product_LabelArticle");
    /// <summary>Метка поля названия.</summary>
    public static string Product_LabelName     => Get("Product_LabelName");
    /// <summary>Метка поля категории.</summary>
    public static string Product_LabelCategory => Get("Product_LabelCategory");
    /// <summary>Метка поля единицы измерения.</summary>
    public static string Product_LabelUnit     => Get("Product_LabelUnit");
    /// <summary>Метка поля цены.</summary>
    public static string Product_LabelPrice    => Get("Product_LabelPrice");
    /// <summary>Метка поля остатка.</summary>
    public static string Product_LabelStock    => Get("Product_LabelStock");
    /// <summary>Метка поля срока годности.</summary>
    public static string Product_LabelExpiry   => Get("Product_LabelExpiry");
    /// <summary>Текст чекбокса "Бессрочно".</summary>
    public static string Product_ChkNoExpiry   => Get("Product_ChkNoExpiry");
    /// <summary>Подсказка об обязательных полях.</summary>
    public static string Product_RequiredHint  => Get("Product_RequiredHint");
    /// <summary>Ошибка дублирующегося артикула.</summary>
    public static string Product_ErrArticleExists => Get("Product_ErrArticleExists");

    // ── ShipmentForm ─────────────────────────────────────────────────────────────

    /// <summary>Заголовок формы отгрузки.</summary>
    public static string Shipment_Title         => Get("Shipment_Title");
    /// <summary>Метка поля получателя.</summary>
    public static string Shipment_LabelRecipient => Get("Shipment_LabelRecipient");
    /// <summary>Заголовок группы добавления позиции.</summary>
    public static string Shipment_GroupAdd      => Get("Shipment_GroupAdd");
    /// <summary>Метка выбора товара.</summary>
    public static string Shipment_LabelProduct  => Get("Shipment_LabelProduct");
    /// <summary>Метка поля количества.</summary>
    public static string Shipment_LabelQty      => Get("Shipment_LabelQty");
    /// <summary>Метка доступного остатка.</summary>
    public static string Shipment_LabelAvailable => Get("Shipment_LabelAvailable");
    /// <summary>Текст кнопки добавления строки.</summary>
    public static string Shipment_BtnAddRow     => Get("Shipment_BtnAddRow");
    /// <summary>Текст кнопки подтверждения.</summary>
    public static string Shipment_BtnConfirm    => Get("Shipment_BtnConfirm");
    /// <summary>Ошибка недостаточного остатка. {0}: название, {1}: запрошено, {2}: доступно.</summary>
    public static string Shipment_ErrInsufficientStock(string name, object requested, object available)
        => Get("Shipment_ErrInsufficientStock", name, requested, available);
    /// <summary>Ошибка пустого списка позиций.</summary>
    public static string Shipment_ErrNoRows     => Get("Shipment_ErrNoRows");
    /// <summary>Сообщение об успешной отгрузке.</summary>
    public static string Shipment_Success       => Get("Shipment_Success");

    // ── HistoryForm ──────────────────────────────────────────────────────────────

    /// <summary>Заголовок формы истории.</summary>
    public static string History_Title           => Get("History_Title");
    /// <summary>Подзаголовок списка накладных.</summary>
    public static string History_LabelShipments  => Get("History_LabelShipments");
    /// <summary>Подзаголовок состава накладной. {0}: идентификатор накладной.</summary>
    public static string History_LabelItems(object id) => Get("History_LabelItems", id);
    /// <summary>Заголовок столбца "Дата и Время".</summary>
    public static string History_ColDate         => Get("History_ColDate");
    /// <summary>Заголовок столбца "Кто оформил".</summary>
    public static string History_ColWho          => Get("History_ColWho");
    /// <summary>Заголовок столбца "Получатель".</summary>
    public static string History_ColRecipient    => Get("History_ColRecipient");

    // ── DeleteConfirmForm ────────────────────────────────────────────────────────

    /// <summary>Заголовок диалога подтверждения удаления.</summary>
    public static string Delete_Title    => Get("Delete_Title");
    /// <summary>Вопрос подтверждения удаления.</summary>
    public static string Delete_Question => Get("Delete_Question");

    // ── UserRole display ─────────────────────────────────────────────────────────

    /// <summary>Отображаемое название роли "Администратор".</summary>
    public static string Role_Admin       => Get("Role_Admin");
    /// <summary>Отображаемое название роли "Кладовщик".</summary>
    public static string Role_Kladovshik  => Get("Role_Kladovshik");
}
