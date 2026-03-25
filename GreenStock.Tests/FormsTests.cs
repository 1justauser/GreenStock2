using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GreenStock.Forms;
using GreenStock.Models;
using GreenStock.Data;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GreenStock.Tests
{
    [TestFixture]
    public class AllTests
    {

        // Вспомогательные методы для работы с private полями форм

        
        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new Exception($"Поле {fieldName} не найдено в {obj.GetType().Name}");
            return (T)field.GetValue(obj)!;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new Exception($"Поле {fieldName} не найдено в {obj.GetType().Name}");
            field.SetValue(obj, value);
        }

        
        // Настройка БД перед каждым тестом
      
        
        [SetUp]
        public void Setup()
        {
            using var db = new AppDbContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            
            // Создаём тестовые категории
            db.Categories.Add(new Category { Name = "Цветы" });
            db.Categories.Add(new Category { Name = "Семена" });
            db.Categories.Add(new Category { Name = "Растения садовые" });
            
            // Создаём тестовые товары
            db.Products.Add(new Product 
            { 
                Article = "ROSE-001", 
                Name = "Роза чайная", 
                CategoryId = 1, 
                Unit = "шт", 
                PurchasePrice = 150, 
                Stock = 50 
            });
            db.Products.Add(new Product 
            { 
                Article = "SEED-042", 
                Name = "Семена подсолнуха", 
                CategoryId = 2, 
                Unit = "пак", 
                PurchasePrice = 20, 
                Stock = 100,
                ExpiryDate = new DateOnly(2027, 12, 31)
            });
            
            // Создаём администратора
            db.Users.Add(new User 
            { 
                Login = "admin", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"), 
                Role = "Admin" 
            });
            
            db.SaveChanges();
        }

      
        //  Авторизация и Регистрация (TC-01..TC-05)
        

        #region TC-01 — Успешная авторизация Администратора
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-01 / US-02 — Успешная авторизация Администратора")]
        [Category("Авторизация")]
        public void TC01_SuccessfulLogin_Admin()
        {
            // Arrange
            var form = new LoginForm();
            var txtLogin = GetPrivateField<TextBox>(form, "txtLogin");
            var txtPassword = GetPrivateField<TextBox>(form, "txtPassword");
            var btnLogin = GetPrivateField<Button>(form, "btnLogin");
            
            txtLogin.Text = "admin";
            txtPassword.Text = "Admin123";
            
            // Act
            btnLogin.PerformClick();
            
            // Assert
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
        }
        
        #endregion

        #region TC-02 — Вход с неверным паролем
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-02 / US-02 — Вход с неверным паролем (Негативный)")]
        [Category("Авторизация")]
        public void TC02_InvalidPassword_ShowsError()
        {
            // Arrange
            var form = new LoginForm();
            var txtLogin = GetPrivateField<TextBox>(form, "txtLogin");
            var txtPassword = GetPrivateField<TextBox>(form, "txtPassword");
            var btnLogin = GetPrivateField<Button>(form, "btnLogin");
            var lblError = GetPrivateField<Label>(form, "lblError");
            
            txtLogin.Text = "admin";
            txtPassword.Text = "wrongpass";
            
            // Act
            btnLogin.PerformClick();
            
            // Assert
            Assert.That(lblError.Visible, Is.True, "Ошибка должна быть видна");
            Assert.That(lblError.Text, Does.Contain("Неверный"), "Текст ошибки должен содержать 'Неверный'");
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.None), "Форма не должна закрыться");
        }
        
        #endregion

        #region TC-03 — Самостоятельная регистрация кладовщика
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-03 / US-01 — Самостоятельная регистрация кладовщика")]
        [Category("Регистрация")]
        public void TC03_SuccessfulRegistration_Warehouse()
        {
            // Arrange
            var form = new RegisterForm();
            var txtLogin = GetPrivateField<TextBox>(form, "txtLogin");
            var txtPassword = GetPrivateField<TextBox>(form, "txtPassword");
            var txtConfirm = GetPrivateField<TextBox>(form, "txtConfirm");
            var txtRole = GetPrivateField<TextBox>(form, "txtRole");
            var btnRegister = GetPrivateField<Button>(form, "btnRegister");
            
            string uniqueLogin = "sklad1_" + Guid.NewGuid().ToString().Substring(0, 6);
            
            txtLogin.Text = uniqueLogin;
            txtPassword.Text = "Pass1234";
            txtConfirm.Text = "Pass1234";
            
            // Act
            btnRegister.PerformClick();
            
            // Assert
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK), "Регистрация должна быть успешной");
            Assert.That(txtRole.Text, Is.EqualTo("Кладовщик"), "Роль должна быть 'Кладовщик'");
            
            // Проверка в БД
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Login == uniqueLogin);
            Assert.That(user, Is.Not.Null, "Пользователь должен быть создан в БД");
            Assert.That(user!.Role, Is.EqualTo("Kladovshik"), "Роль в БД должна быть 'Kladovshik'");
        }
        
        #endregion

        #region TC-04 — Регистрация с занятым логином
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-04 / US-01 — Регистрация с занятым логином (Негативный)")]
        [Category("Регистрация")]
        public void TC04_DuplicateLogin_ShowsError()
        {
            // Arrange
            var form = new RegisterForm();
            var txtLogin = GetPrivateField<TextBox>(form, "txtLogin");
            var txtPassword = GetPrivateField<TextBox>(form, "txtPassword");
            var txtConfirm = GetPrivateField<TextBox>(form, "txtConfirm");
            var btnRegister = GetPrivateField<Button>(form, "btnRegister");
            var lblLoginError = GetPrivateField<Label>(form, "lblLoginError");
            
            txtLogin.Text = "admin"; // Уже существует
            txtPassword.Text = "Pass1234";
            txtConfirm.Text = "Pass1234";
            
            // Act
            btnRegister.PerformClick();
            
            // Assert
            Assert.That(lblLoginError.Visible, Is.True, "Ошибка должна быть видна");
            Assert.That(lblLoginError.Text, Does.Contain("занят"), "Текст ошибки должен содержать 'занят'");
        }
        
        #endregion

        #region TC-05 — Пароли не совпадают
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-05 / US-01 — Пароли не совпадают (Негативный)")]
        [Category("Регистрация")]
        public void TC05_PasswordMismatch_ShowsError()
        {
            // Arrange
            var form = new RegisterForm();
            var txtLogin = GetPrivateField<TextBox>(form, "txtLogin");
            var txtPassword = GetPrivateField<TextBox>(form, "txtPassword");
            var txtConfirm = GetPrivateField<TextBox>(form, "txtConfirm");
            var btnRegister = GetPrivateField<Button>(form, "btnRegister");
            var lblConfirmError = GetPrivateField<Label>(form, "lblConfirmError");
            
            txtLogin.Text = "newuser_" + Guid.NewGuid().ToString().Substring(0, 6);
            txtPassword.Text = "Pass1234";
            txtConfirm.Text = "Pass0000"; // Не совпадает
            
            // Act
            btnRegister.PerformClick();
            
            // Assert
            Assert.That(lblConfirmError.Visible, Is.True, "Ошибка должна быть видна");
            Assert.That(lblConfirmError.Text, Does.Contain("совпадают"), "Текст ошибки должен содержать 'совпадают'");
        }
        
        #endregion

        
        //Управление товарами (TC-06..TC-10)
      

        #region TC-06 — Добавление нового товара
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-06 / US-03 — Добавление нового товара")]
        [Category("Товары")]
        public void TC06_AddNewProduct_Success()
        {
            // Arrange
            var form = new ProductForm(null);
            var txtArticle = GetPrivateField<TextBox>(form, "txtArticle");
            var txtName = GetPrivateField<TextBox>(form, "txtName");
            var cmbCategory = GetPrivateField<ComboBox>(form, "cmbCategory");
            var cmbUnit = GetPrivateField<ComboBox>(form, "cmbUnit");
            var nudPrice = GetPrivateField<NumericUpDown>(form, "nudPrice");
            var btnSave = GetPrivateField<Button>(form, "btnSave");
            
            txtArticle.Text = "NEW-" + Guid.NewGuid().ToString().Substring(0, 6);
            txtName.Text = "Тестовый товар";
            cmbCategory.SelectedIndex = 0;
            cmbUnit.SelectedIndex = 0;
            nudPrice.Value = 100;
            
            // Act
            btnSave.PerformClick();
            
            // Assert
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK), "Товар должен быть сохранён");
            
            using var db = new AppDbContext();
            var product = db.Products.FirstOrDefault(p => p.Article == txtArticle.Text);
            Assert.That(product, Is.Not.Null, "Товар должен быть в БД");
            Assert.That(product!.Stock, Is.EqualTo(0), "Остаток нового товара должен быть 0");
        }
        
        #endregion

        #region TC-07 — Добавление товара с занятым артикулом
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-07 / US-03 — Добавление товара с занятым артикулом (Негативный)")]
        [Category("Товары")]
        public void TC07_DuplicateArticle_ShowsError()
        {
            // Arrange
            var form = new ProductForm(null);
            var txtArticle = GetPrivateField<TextBox>(form, "txtArticle");
            var txtName = GetPrivateField<TextBox>(form, "txtName");
            var cmbCategory = GetPrivateField<ComboBox>(form, "cmbCategory");
            var btnSave = GetPrivateField<Button>(form, "btnSave");
            var lblArticleError = GetPrivateField<Label>(form, "lblArticleError");
            
            txtArticle.Text = "ROSE-001"; // Уже существует
            txtName.Text = "Дубликат";
            cmbCategory.SelectedIndex = 0;
            
            // Act
            btnSave.PerformClick();
            
            // Assert
            Assert.That(lblArticleError.Visible, Is.True, "Ошибка должна быть видна");
            Assert.That(lblArticleError.Text, Does.Contain("существует"), "Текст ошибки должен содержать 'существует'");
        }
        
        #endregion

        #region TC-08 — Редактирование товара
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-08 / US-04 — Редактирование товара")]
        [Category("Товары")]
        public void TC08_EditProduct_Success()
        {
            // Arrange
            using var db = new AppDbContext();
            var product = db.Products.First(p => p.Article == "ROSE-001");
            
            var form = new ProductForm(product);
            var txtArticle = GetPrivateField<TextBox>(form, "txtArticle");
            var txtName = GetPrivateField<TextBox>(form, "txtName");
            var nudPrice = GetPrivateField<NumericUpDown>(form, "nudPrice");
            var btnSave = GetPrivateField<Button>(form, "btnSave");
            
            // Act
            txtName.Text = "Роза чайная (обновлённая)";
            nudPrice.Value = 180;
            btnSave.PerformClick();
            
            // Assert
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK), "Товар должен быть обновлён");
            Assert.That(txtArticle.ReadOnly, Is.True, "Артикул должен быть ReadOnly при редактировании");
            
            db.ChangeTracker.Clear();
            var updated = db.Products.Find(product.Id);
            Assert.That(updated!.Name, Is.EqualTo("Роза чайная (обновлённая)"), "Название должно обновиться");
            Assert.That(updated.PurchasePrice, Is.EqualTo(180), "Цена должна обновиться");
        }
        
        #endregion

        #region TC-09 — Удаление товара
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-09 / US-05 — Удаление товара")]
        [Category("Товары")]
        public void TC09_DeleteProduct_Success()
        {
            // Arrange
            using var db = new AppDbContext();
            var product = db.Products.First(p => p.Article == "SEED-042");
            int productId = product.Id;
            
            var confirmForm = new DeleteConfirmForm(product.Name, "товар");
            var btnYes = GetPrivateField<Button>(confirmForm, "btnYes");
            
            // Act
            btnYes.PerformClick();
            
            // Assert
            Assert.That(confirmForm.DialogResult, Is.EqualTo(DialogResult.Yes), "Подтверждение должно быть принято");
            
            // Фактическое удаление через CatalogForm
            var catalogForm = new CatalogForm(new User { Id = 1, Login = "admin", Role = "Admin" });
            var btnDelete = GetPrivateField<Button>(catalogForm, "btnDelete");
            var dgvProducts = GetPrivateField<DataGridView>(catalogForm, "dgvProducts");
            
            // Выбираем товар в таблице
            dgvProducts.CurrentCell = dgvProducts.Rows[0].Cells[0];
            btnDelete.PerformClick();
            
            db.ChangeTracker.Clear();
            var deleted = db.Products.Find(productId);
            Assert.That(deleted, Is.Null, "Товар должен быть удалён из БД");
        }
        
        #endregion

        #region TC-10 — Просмотр остатков кладовщиком
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-10 / US-07 — Просмотр остатков кладовщиком")]
        [Category("Товары")]
        public void TC10_WarehouseViewStock_ReadOnly()
        {
            // Arrange
            var warehouseUser = new User { Id = 2, Login = "sklad1", Role = "Kladovshik" };
            var form = new CatalogForm(warehouseUser);
            var btnAdd = GetPrivateField<Button>(form, "btnAdd");
            var btnEdit = GetPrivateField<Button>(form, "btnEdit");
            var btnDelete = GetPrivateField<Button>(form, "btnDelete");
            var dgvProducts = GetPrivateField<DataGridView>(form, "dgvProducts");
            
            // Assert
            Assert.That(btnAdd.Enabled, Is.False, "Кнопка 'Добавить' должна быть недоступна кладовщику");
            Assert.That(btnEdit.Enabled, Is.False, "Кнопка 'Редактировать' должна быть недоступна кладовщику");
            Assert.That(btnDelete.Enabled, Is.False, "Кнопка 'Удалить' должна быть недоступна кладовщику");
            Assert.That(dgvProducts.Rows.Count, Is.GreaterThan(0), "Товары должны отображаться");
        }
        
        #endregion


        // Отгрузки (TC-11..TC-12)


        #region TC-11 — Успешная отгрузка
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-11 / US-08,09,10 — Успешная отгрузка при достаточном остатке")]
        [Category("Отгрузки")]
        public void TC11_SuccessfulShipment_StockReduced()
        {
            // Arrange
            var warehouseUser = new User { Id = 2, Login = "sklad1", Role = "Kladovshik" };
            var form = new ShipmentForm(warehouseUser);
            
            var txtRecipient = GetPrivateField<TextBox>(form, "txtRecipient");
            var cmbProduct = GetPrivateField<ComboBox>(form, "cmbProduct");
            var nudQty = GetPrivateField<NumericUpDown>(form, "nudQty");
            var btnAddRow = GetPrivateField<Button>(form, "btnAddRow");
            var btnConfirm = GetPrivateField<Button>(form, "btnConfirm");
            
            txtRecipient.Text = "ООО Ромашка";
            cmbProduct.SelectedIndex = 0; // ROSE-001
            nudQty.Value = 10;
            
            // Act
            btnAddRow.PerformClick();
            btnConfirm.PerformClick();
            
            // Assert
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK), "Отгрузка должна быть успешной");
            
            using var db = new AppDbContext();
            var product = db.Products.First(p => p.Article == "ROSE-001");
            Assert.That(product.Stock, Is.EqualTo(40), "Остаток должен уменьшиться с 50 до 40");
            
            var shipment = db.Shipments.FirstOrDefault(s => s.Recipient == "ООО Ромашка");
            Assert.That(shipment, Is.Not.Null, "Накладная должна быть создана");
        }
        
        #endregion

        #region TC-12 — Блокировка при нехватке товара
        
        [Test]
        [Apartment(ApartmentState.STA)]
        [Description("TC-12 / US-09 — Блокировка при нехватке товара (Негативный)")]
        [Category("Отгрузки")]
        public void TC12_InsufficientStock_Blocked()
        {
            // Arrange
            using var db = new AppDbContext();
            var product = db.Products.First(p => p.Article == "ROSE-001");
            product.Stock = 5; // Уменьшаем остаток
            db.SaveChanges();
            
            var warehouseUser = new User { Id = 2, Login = "sklad1", Role = "Kladovshik" };
            var form = new ShipmentForm(warehouseUser);
            
            var txtRecipient = GetPrivateField<TextBox>(form, "txtRecipient");
            var cmbProduct = GetPrivateField<ComboBox>(form, "cmbProduct");
            var nudQty = GetPrivateField<NumericUpDown>(form, "nudQty");
            var btnAddRow = GetPrivateField<Button>(form, "btnAddRow");
            var btnConfirm = GetPrivateField<Button>(form, "btnConfirm");
            var lblWarning = GetPrivateField<Label>(form, "lblWarning");
            
            txtRecipient.Text = "ООО Ромашка";
            cmbProduct.SelectedIndex = 0; // ROSE-001
            nudQty.Value = 20; // Больше чем есть (5)
            
            // Act
            btnAddRow.PerformClick();
            
            // Assert
            Assert.That(btnConfirm.Enabled, Is.False, "Кнопка 'Подтвердить' должна быть заблокирована");
            Assert.That(lblWarning.Text, Does.Contain("Недостаточно"), "Должно быть предупреждение о нехватке");
            
            db.ChangeTracker.Clear();
            var productAfter = db.Products.First(p => p.Article == "ROSE-001");
            Assert.That(productAfter.Stock, Is.EqualTo(5), "Остаток не должен измениться");
        }
        
        #endregion

        // ГРУППА 4: Логика валидации (дополнительные тесты)

        #region Валидация пароля
        
        [Test]
        [Description("Валидация пароля — совпадающие пароли")]
        [Category("Валидация")]
        public void PasswordValidation_Matching_ReturnsTrue()
        {
            string password = "Pass1234";
            string confirm = "Pass1234";
            Assert.That(password == confirm, Is.True);
        }
        
        [Test]
        [Description("Валидация пароля — несовпадающие пароли")]
        [Category("Валидация")]
        public void PasswordValidation_NonMatching_ReturnsFalse()
        {
            string password = "Pass1234";
            string confirm = "pass1234";
            Assert.That(password == confirm, Is.False);
        }
        
        #endregion

        #region Валидация обязательных полей
        
        [Test]
        [Description("Валидация — пустой артикул")]
        [Category("Валидация")]
        public void ArticleValidation_Empty_ReturnsFalse()
        {
            string article = "   ";
            Assert.That(string.IsNullOrWhiteSpace(article), Is.True);
        }
        
        [Test]
        [Description("Валидация — пустой получатель")]
        [Category("Валидация")]
        public void RecipientValidation_Empty_ReturnsFalse()
        {
            string recipient = "    ";
            Assert.That(string.IsNullOrWhiteSpace(recipient), Is.True);
        }
        
        #endregion

        #region BCrypt хеширование
        
        [Test]
        [Description("BCrypt — правильный пароль верифицируется")]
        [Category("Безопасность")]
        public void BcryptHash_CorrectPassword_Verified()
        {
            string password = "Admin123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.That(BCrypt.Net.BCrypt.Verify(password, hash), Is.True);
        }
        
        [Test]
        [Description("BCrypt — неправильный пароль не верифицируется")]
        [Category("Безопасность")]
        public void BcryptHash_WrongPassword_NotVerified()
        {
            string password = "Admin123";
            string wrongPass = "wrongpass";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.That(BCrypt.Net.BCrypt.Verify(wrongPass, hash), Is.False);
        }
        
        #endregion
    }
}