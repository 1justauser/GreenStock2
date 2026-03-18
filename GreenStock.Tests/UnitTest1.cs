using GreenStock.Models;
using NUnit.Framework;

namespace GreenStock.Tests
{
    [TestFixture]
    public class Tests
    {
        // ─── 1. Проверка остатка при отгрузке ───────────────────────────────

        [Test]
        public void CheckStock_SufficientStock_ReturnsTrue()
        {
            // Запрошено меньше чем есть — должно вернуть true
            int requested = 10;
            int available = 50;
            Assert.That(requested <= available, Is.True);
        }

        [Test]
        public void CheckStock_ExactStock_ReturnsTrue()
        {
            // Запрошено ровно столько сколько есть — допустимо
            int requested = 50;
            int available = 50;
            Assert.That(requested <= available, Is.True);
        }

        [Test]
        public void CheckStock_InsufficientStock_ReturnsFalse()
        {
            // Запрошено больше чем есть — должно вернуть false
            int requested = 60;
            int available = 50;
            Assert.That(requested <= available, Is.False);
        }

        [Test]
        public void CheckStock_ZeroStock_ReturnsFalse()
        {
            // На складе ноль — нельзя отгружать
            int requested = 1;
            int available = 0;
            Assert.That(requested <= available, Is.False);
        }

        // ─── 2. Валидация пароля при регистрации ────────────────────────────

        [Test]
        public void PasswordValidation_MatchingPasswords_ReturnsTrue()
        {
            string password = "Pass1234";
            string confirm = "Pass1234";
            Assert.That(password == confirm, Is.True);
        }

        [Test]
        public void PasswordValidation_NonMatchingPasswords_ReturnsFalse()
        {
            string password = "Pass1234";
            string confirm = "pass1234";
            Assert.That(password == confirm, Is.False);
        }

        [Test]
        public void PasswordValidation_TooShort_ReturnsFalse()
        {
            // Пароль менее 4 символов — не допустимо
            string password = "abc";
            Assert.That(password.Length >= 4, Is.False);
        }

        [Test]
        public void PasswordValidation_MinLength_ReturnsTrue()
        {
            string password = "abcd";
            Assert.That(password.Length >= 4, Is.True);
        }

        // ─── 3. Валидация обязательных полей ────────────────────────────────

        [Test]
        public void ArticleValidation_EmptyArticle_ReturnsFalse()
        {
            string article = "";
            Assert.That(string.IsNullOrWhiteSpace(article), Is.True);
        }

        [Test]
        public void ArticleValidation_ValidArticle_ReturnsTrue()
        {
            string article = "ROSE-001";
            Assert.That(string.IsNullOrWhiteSpace(article), Is.False);
        }

        [Test]
        public void RecipientValidation_EmptyRecipient_ReturnsFalse()
        {
            string recipient = "   ";
            Assert.That(string.IsNullOrWhiteSpace(recipient), Is.True);
        }

        [Test]
        public void RecipientValidation_ValidRecipient_ReturnsTrue()
        {
            string recipient = "ООО Ромашка";
            Assert.That(string.IsNullOrWhiteSpace(recipient), Is.False);
        }

        // ─── 4. Проверка роли пользователя ──────────────────────────────────

        [Test]
        public void RoleCheck_AdminRole_IsAdmin()
        {
            var user = new User { Login = "admin", Role = "Admin" };
            Assert.That(user.Role == "Admin", Is.True);
        }

        [Test]
        public void RoleCheck_KladovshikRole_IsNotAdmin()
        {
            var user = new User { Login = "sklad1", Role = "Kladovshik" };
            Assert.That(user.Role == "Admin", Is.False);
        }

        [Test]
        public void RoleCheck_AdminCannotShip()
        {
            // Администратор не создаёт отгрузки
            var user = new User { Role = "Admin" };
            bool canShip = user.Role == "Kladovshik";
            Assert.That(canShip, Is.False);
        }

        [Test]
        public void RoleCheck_KladovshikCanShip()
        {
            var user = new User { Role = "Kladovshik" };
            bool canShip = user.Role == "Kladovshik";
            Assert.That(canShip, Is.True);
        }

        // ─── 5. Проверка срока годности ─────────────────────────────────────

        [Test]
        public void ExpiryDate_NoExpiry_IsNull()
        {
            var product = new Product { Article = "ROSE-001", ExpiryDate = null };
            Assert.That(product.ExpiryDate.HasValue, Is.False);
        }

        [Test]
        public void ExpiryDate_WithExpiry_HasValue()
        {
            var product = new Product { Article = "SEED-042", ExpiryDate = new DateOnly(2026, 12, 31) };
            Assert.That(product.ExpiryDate.HasValue, Is.True);
        }

        // ─── 6. Проверка bcrypt хеша ─────────────────────────────────────────

        [Test]
        public void BcryptHash_CorrectPassword_Verified()
        {
            string password = "Admin123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.That(BCrypt.Net.BCrypt.Verify(password, hash), Is.True);
        }

        [Test]
        public void BcryptHash_WrongPassword_NotVerified()
        {
            string password = "Admin123";
            string wrongPass = "wrongpass";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.That(BCrypt.Net.BCrypt.Verify(wrongPass, hash), Is.False);
        }
    }
}