using GreenStock.Forms;
using GreenStock.Models;
using NUnit.Framework;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using GreenStock.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace GreenStock.Tests
{
    [TestFixture]
    public class FormsTests
    {
        // Вспомогательный метод для создания тестового пользователя
        private User CreateTestUser(string login, string password, string role)
        {
            var user = new User
            {
                Login = login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };
            using (var db = new AppDbContext()) {
                db.Users.Add(user);
                db.SaveChanges();
            }
            return user;
        }

        // Вспомогательный метод для очистки базы данных перед каждым тестом
        [SetUp]
        public void Setup()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                // Добавляем тестовые данные, если нужно
                db.Categories.Add(new Category { Name = "Цветы" });
                db.Categories.Add(new Category { Name = "Семена" });
                db.Products.Add(new Product { Article = "ROSE-001", Name = "Роза", Category = db.Categories.First(c => c.Name == "Цветы"), Unit = "шт", PurchasePrice = 150, Stock = 50 });
                db.Products.Add(new Product { Article = "SEED-001", Name = "Семена подсолнуха", Category = db.Categories.First(c => c.Name == "Семена"), Unit = "пак", PurchasePrice = 20, Stock = 100, ExpiryDate = new DateOnly(2027, 12, 31) });
                db.SaveChanges();
            }
        }

        // ─── 1. LoginForm Tests ────────────────────────────────────────────────────

        [Test]
        [Description("TC-01: Успешная авторизация Администратора")]
        public void LoginForm_TC01_AdminLoginSuccess()
        {
            CreateTestUser("admin", "password", "Admin");
            using var form = new LoginForm();
            form.txtLogin.Text = "admin";
            form.txtPassword.Text = "password";

            // Используем Invoke для вызова обработчика события в потоке UI
            form.Invoke((MethodInvoker)delegate { form.btnLogin.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            Assert.That(form.LoggedInUser, Is.Not.Null);
            Assert.That(form.LoggedInUser!.Login, Is.EqualTo("admin"));
            Assert.That(form.LoggedInUser.Role, Is.EqualTo("Admin"));
        }

        [Test]
        [Description("TC-02: Вход с неверным паролем")]
        public void LoginForm_TC02_InvalidPassword()
        {
            CreateTestUser("admin", "password", "Admin");
            using var form = new LoginForm();
            form.txtLogin.Text = "admin";
            form.txtPassword.Text = "wrongpassword";

            form.Invoke((MethodInvoker)delegate { form.btnLogin.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.None)); // Форма не закрывается
            Assert.That(form.lblError.Visible, Is.True);
            Assert.That(form.lblError.Text, Is.EqualTo("Неверный логин или пароль"));
            Assert.That(form.txtPassword.Text, Is.Empty); // Поле пароля очищается
        }

        [Test]
        [Description("Баг: Заголовок формы не соответствует макету")]
        public void LoginForm_Title_Bug()
        {
            using var form = new LoginForm();
            Assert.That(form.Text, Is.EqualTo("Авторизация")); // Ожидаем "Авторизация" по макету
        }

        // ─── 2. RegisterForm Tests ─────────────────────────────────────────────────

        [Test]
        [Description("TC-03: Успешная регистрация кладовщика")]
        public void RegisterForm_TC03_KladovshikRegistrationSuccess()
        {
            using var form = new RegisterForm();
            form.txtLogin.Text = "newklad";
            form.txtPassword.Text = "pass123";
            form.txtConfirm.Text = "pass123";

            form.Invoke((MethodInvoker)delegate { form.btnRegister.PerformClick(); });

            Assert.That(form.RegisteredLogin, Is.EqualTo("newklad"));
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == "newklad");
                Assert.That(user, Is.Not.Null);
                Assert.That(user!.Role, Is.EqualTo("Kladovshik"));
            }
        }

        [Test]
        [Description("TC-04: Регистрация с занятым логином")]
        public void RegisterForm_TC04_DuplicateLogin()
        {
            CreateTestUser("existinguser", "password", "Admin");
            using var form = new RegisterForm();
            form.txtLogin.Text = "existinguser";
            form.txtPassword.Text = "pass123";
            form.txtConfirm.Text = "pass123";

            form.Invoke((MethodInvoker)delegate { form.btnRegister.PerformClick(); });

            Assert.That(form.lblLoginError.Visible, Is.True);
            Assert.That(form.lblLoginError.Text, Is.EqualTo("Логин уже занят"));
            using (var db = new AppDbContext())
            {
                Assert.That(db.Users.Count(u => u.Login == "existinguser"), Is.EqualTo(1));
            }
        }

        [Test]
        [Description("TC-05: Пароли не совпадают")]
        public void RegisterForm_TC05_PasswordsMismatch()
        {
            using var form = new RegisterForm();
            form.txtLogin.Text = "testuser";
            form.txtPassword.Text = "pass123";
            form.txtConfirm.Text = "pass321";

            form.Invoke((MethodInvoker)delegate { form.btnRegister.PerformClick(); });

            Assert.That(form.lblConfirmError.Visible, Is.True);
            Assert.That(form.lblConfirmError.Text, Is.EqualTo("Пароли не совпадают"));
            using (var db = new AppDbContext())
            {
                Assert.That(db.Users.Any(u => u.Login == "testuser"), Is.False);
            }
        }

        [Test]
        [Description("RegisterForm: Поле Роль должно быть 'Кладовщик' и ReadOnly")]
        public void RegisterForm_RoleField_IsKladovshikAndReadOnly()
        {
            using var form = new RegisterForm();
            Assert.That(form.txtRole.Text, Is.EqualTo("Кладовщик"));
            Assert.That(form.txtRole.ReadOnly, Is.True);
        }

        // ─── 3. ProductForm Tests ──────────────────────────────────────────────────

        [Test]
        [Description("TC-06: Добавление нового товара (поле Остаток ReadOnly)")]
        public void ProductForm_TC06_AddNewProduct()
        {
            using var form = new ProductForm(null);
            form.txtArticle.Text = "NEW-001";
            form.txtName.Text = "Новый товар";
            form.cmbCategory.SelectedIndex = 0; // Выбираем первую категорию (Цветы)
            form.cmbUnit.SelectedIndex = 0; // шт
            form.nudPrice.Value = 250;
            // Поле Stock теперь ReadOnly, поэтому не устанавливаем его вручную

            form.Invoke((MethodInvoker)delegate { form.btnSave.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            using (var db = new AppDbContext())
            {
                var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Article == "NEW-001");
                Assert.That(product, Is.Not.Null);
                Assert.That(product!.Name, Is.EqualTo("Новый товар"));
                Assert.That(product.Category!.Name, Is.EqualTo("Цветы"));
                Assert.That(product.Unit, Is.EqualTo("шт"));
                Assert.That(product.PurchasePrice, Is.EqualTo(250));
                Assert.That(product.Stock, Is.EqualTo(0)); // Ожидаем 0 по текущей реализации кода для нового товара
            }
        }

        [Test]
        [Description("TC-07: Добавление товара с занятым артикулом")]
        public void ProductForm_TC07_DuplicateArticle()
        {
            using var form = new ProductForm(null);
            form.txtArticle.Text = "ROSE-001"; // Уже существует
            form.txtName.Text = "Дубликат";
            form.cmbCategory.SelectedIndex = 0;
            form.cmbUnit.SelectedIndex = 0;
            form.nudPrice.Value = 100;

            form.Invoke((MethodInvoker)delegate { form.btnSave.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.None));
            Assert.That(form.lblArticleError.Visible, Is.True);
            Assert.That(form.lblArticleError.Text, Is.EqualTo("Артикул уже существует"));
        }

        [Test]
        [Description("TC-08: Редактирование товара")]
        public void ProductForm_TC08_EditProduct()
        {
            Product existingProduct;
            using (var db = new AppDbContext()) {
                existingProduct = db.Products.First(p => p.Article == "ROSE-001");
            }

            using var form = new ProductForm(existingProduct);
            Assert.That(form.txtArticle.ReadOnly, Is.True); // Артикул заблокирован
            Assert.That(form.txtArticle.Text, Is.EqualTo("ROSE-001"));
            Assert.That(form.txtStock.ReadOnly, Is.True); // Проверяем, что Stock ReadOnly

            form.txtName.Text = "Роза Красная";
            form.nudPrice.Value = 180;

            form.Invoke((MethodInvoker)delegate { form.btnSave.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            using (var db = new AppDbContext())
            {
                var updatedProduct = db.Products.Find(existingProduct.Id);
                Assert.That(updatedProduct, Is.Not.Null);
                Assert.That(updatedProduct!.Name, Is.EqualTo("Роза Красная"));
                Assert.That(updatedProduct.PurchasePrice, Is.EqualTo(180));
                Assert.That(updatedProduct.Stock, Is.EqualTo(existingProduct.Stock)); // Остаток не меняется
            }
        }

        [Test]
        [Description("Баг: Отсутствует поле для ExpiryDate")]
        public void ProductForm_Bug_MissingExpiryDateField()
        {
            using var form = new ProductForm(null);
            // Проверяем, что нет контрола, который мог бы быть полем для ExpiryDate
            // Это косвенная проверка, так как напрямую проверить отсутствие контрола сложно без рефлексии
            // В реальном проекте это был бы ручной тест или UI-тест
            var expiryDateLabel = form.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Срок годности"));
            var expiryDateControl = form.Controls.OfType<DateTimePicker>().FirstOrDefault(); // Или другой тип контрола

            Assert.That(expiryDateLabel, Is.Null, "На форме ProductForm присутствует Label 'Срок годности', хотя его не должно быть по макету.");
            Assert.That(expiryDateControl, Is.Null, "На форме ProductForm присутствует контрол для ввода срока годности, хотя его не должно быть по макету.");
        }

        // ─── 4. CategoryForm Tests ─────────────────────────────────────────────────

        [Test]
        [Description("TC-09: Добавление новой категории")]
        public void CategoryForm_TC09_AddNewCategory()
        {
            using var form = new CategoryForm();
            form.txtCategoryName.Text = "Овощи";

            form.Invoke((MethodInvoker)delegate { form.btnAdd.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            using (var db = new AppDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Name == "Овощи");
                Assert.That(category, Is.Not.Null);
            }
        }

        [Test]
        [Description("TC-10: Добавление существующей категории")]
        public void CategoryForm_TC10_AddExistingCategory()
        {
            using var form = new CategoryForm();
            form.txtCategoryName.Text = "Цветы"; // Уже существует

            form.Invoke((MethodInvoker)delegate { form.btnAdd.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.None));
            Assert.That(form.lblError.Visible, Is.True);
            Assert.That(form.lblError.Text, Is.EqualTo("Категория с таким названием уже существует"));
        }

        [Test]
        [Description("TC-11: Редактирование категории")]
        public void CategoryForm_TC11_EditCategory()
        {
            Category existingCategory;
            using (var db = new AppDbContext()) {
                existingCategory = db.Categories.First(c => c.Name == "Цветы");
            }

            using var form = new CategoryForm();
            form.lstCategories.SelectedItem = existingCategory; // Выбираем категорию
            form.txtCategoryName.Text = "Цветы садовые";

            form.Invoke((MethodInvoker)delegate { form.btnEdit.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            using (var db = new AppDbContext())
            {
                var updatedCategory = db.Categories.Find(existingCategory.Id);
                Assert.That(updatedCategory, Is.Not.Null);
                Assert.That(updatedCategory!.Name, Is.EqualTo("Цветы садовые"));
            }
        }

        [Test]
        [Description("TC-12: Удаление категории")]
        public void CategoryForm_TC12_DeleteCategory()
        {
            Category existingCategory;
            using (var db = new AppDbContext()) {
                existingCategory = db.Categories.First(c => c.Name == "Цветы");
            }

            using var form = new CategoryForm();
            form.lstCategories.SelectedItem = existingCategory; // Выбираем категорию

            // Моделируем подтверждение удаления
            form.ShowDialogResult = DialogResult.OK; // Предполагаем, что пользователь нажал OK в DeleteConfirmForm

            form.Invoke((MethodInvoker)delegate { form.btnDelete.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            using (var db = new AppDbContext())
            {
                var deletedCategory = db.Categories.Find(existingCategory.Id);
                Assert.That(deletedCategory, Is.Null);
            }
        }

        // ─── 5. ShipmentForm Tests ─────────────────────────────────────────────────

        [Test]
        [Description("TC-13: Добавление отгрузки с несколькими позициями")]
        public void ShipmentForm_TC13_AddShipmentWithMultipleItems()
        {
            using var form = new ShipmentForm();
            form.txtRecipient.Text = "Получатель 1";

            // Добавляем первый товар
            var productRose = db.Products.First(p => p.Article == "ROSE-001");
            form.cmbProduct.SelectedItem = productRose; // Моделируем выбор
            form.nudQuantity.Value = 10;
            form.Invoke((MethodInvoker)delegate { form.btnAddItem.PerformClick(); });

            // Добавляем второй товар
            var productSeeds = db.Products.First(p => p.Article == "SEED-001");
            form.cmbProduct.SelectedItem = productSeeds; // Моделируем выбор
            form.nudQuantity.Value = 20;
            form.Invoke((MethodInvoker)delegate { form.btnAddItem.PerformClick(); });

            form.Invoke((MethodInvoker)delegate { form.btnSave.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            using (var db = new AppDbContext())
            {
                var shipment = db.Shipments.Include(s => s.ShipmentItems).ThenInclude(si => si.Product).FirstOrDefault(s => s.Recipient == "Получатель 1");
                Assert.That(shipment, Is.Not.Null);
                Assert.That(shipment!.ShipmentItems.Count, Is.EqualTo(2));
                Assert.That(shipment.ShipmentItems.Sum(si => si.Quantity), Is.EqualTo(30));

                // Проверяем уменьшение остатков
                Assert.That(db.Products.First(p => p.Article == "ROSE-001").Stock, Is.EqualTo(40));
                Assert.That(db.Products.First(p => p.Article == "SEED-001").Stock, Is.EqualTo(80));
            }
        }

        [Test]
        [Description("TC-14: Отгрузка товара с недостаточным остатком")]
        public void ShipmentForm_TC14_InsufficientStock()
        {
            using var form = new ShipmentForm();
            form.txtRecipient.Text = "Получатель 2";

            var productRose = db.Products.First(p => p.Article == "ROSE-001");
            form.cmbProduct.SelectedItem = productRose;
            form.nudQuantity.Value = 60; // Больше, чем есть на складе
            form.Invoke((MethodInvoker)delegate { form.btnAddItem.PerformClick(); });

            Assert.That(form.lblError.Visible, Is.True);
            Assert.That(form.lblError.Text, Is.EqualTo("Недостаточно товара на складе"));
            Assert.That(form.dgvItems.Rows.Count, Is.EqualTo(0)); // Товар не должен быть добавлен
        }

        [Test]
        [Description("TC-15: Отгрузка без указания получателя")]
        public void ShipmentForm_TC15_NoRecipient()
        {
            using var form = new ShipmentForm();
            // form.txtRecipient.Text = ""; // Оставляем пустым

            var productRose = db.Products.First(p => p.Article == "ROSE-001");
            form.cmbProduct.SelectedItem = productRose;
            form.nudQuantity.Value = 10;
            form.Invoke((MethodInvoker)delegate { form.btnAddItem.PerformClick(); });

            form.Invoke((MethodInvoker)delegate { form.btnSave.PerformClick(); });

            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.None));
            Assert.That(form.lblError.Visible, Is.True);
            Assert.That(form.lblError.Text, Is.EqualTo("Поле 'Получатель' обязательно для заполнения"));
            Assert.That(form.dgvItems.Rows.Count, Is.EqualTo(1)); // Товар должен быть добавлен, но отгрузка не сохранена
        }
    }
}
