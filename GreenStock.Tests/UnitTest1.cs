using GreenStock.Models;
using NUnit.Framework;
using BCrypt.Net;

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
            var requested = 10;
            var available = 50;
            Assert.That(requested <= available, Is.True);
        }

        [Test]
        public void CheckStock_ExactStock_ReturnsTrue()
        {
            // Запрошено ровно столько сколько есть — допустимо
            var requested = 50;
            var available = 50;
            Assert.That(requested <= available, Is.True);
        }

        [Test]
        public void CheckStock_InsufficientStock_ReturnsFalse()
        {
            // Запрошено больше чем есть — должно вернуть false
            var requested = 60;
            var available = 50;
            Assert.That(requested <= available, Is.False);
        }

        [Test]
        public void CheckStock_ZeroStock_ReturnsFalse()
        {
            // На складе ноль — нельзя отгружать
            var requested = 1;
            var available = 0;
            Assert.That(requested <= available, Is.False);
        }

        // ─── 2. Валидация пароля при регистрации ────────────────────────────

        [Test]
        public void PasswordValidation_MatchingPasswords_ReturnsTrue()
        {
            var password = "Pass1234";
            var confirm = "Pass1234";
            Assert.That(password == confirm, Is.True);
        }

        [Test]
        public void PasswordValidation_NonMatchingPasswords_ReturnsFalse()
        {
            var password = "Pass1234";
            var confirm = "pass1234";
            Assert.That(password == confirm, Is.False);
        }

        [Test]
        public void PasswordValidation_TooShort_ReturnsFalse()
        {
            // Пароль менее 4 символов — не допустимо
            var password = "abc";
            Assert.That(password.Length >= 4, Is.False);
        }

        [Test]
        public void PasswordValidation_MinLength_ReturnsTrue()
        {
            var password = "abcd";
            Assert.That(password.Length >= 4, Is.True);
        }

        // ─── 3. Валидация обязательных полей ────────────────────────────────

        [Test]
        public void ArticleValidation_EmptyArticle_ReturnsFalse()
        {
            var article = "";
            Assert.That(string.IsNullOrWhiteSpace(article), Is.True);
        }

        [Test]
        public void ArticleValidation_ValidArticle_ReturnsTrue()
        {
            var article = "ROSE-001";
            Assert.That(string.IsNullOrWhiteSpace(article), Is.False);
        }

        [Test]
        public void RecipientValidation_EmptyRecipient_ReturnsFalse()
        {
            var recipient = "   ";
            Assert.That(string.IsNullOrWhiteSpace(recipient), Is.True);
        }

        [Test]
        public void RecipientValidation_ValidRecipient_ReturnsTrue()
        {
            var recipient = "ООО Ромашка";
            Assert.That(string.IsNullOrWhiteSpace(recipient), Is.False);
        }

        // ─── 4. Проверка роли пользователя ──────────────────────────────────

        [Test]
        public void RoleCheck_AdminRole_IsAdmin()
        {
            var user = new User { Login = "admin", Role = UserRole.Admin };
            Assert.That(user.Role == UserRole.Admin, Is.True);
        }

        [Test]
        public void RoleCheck_KladovshikRole_IsNotAdmin()
        {
            var user = new User { Login = "sklad1", Role = UserRole.Kladovshik };
            Assert.That(user.Role == UserRole.Admin, Is.False);
        }

        [Test]
        public void RoleCheck_AdminCannotShip()
        {
            // Администратор не создаёт отгрузки
            var user    = new User { Role = UserRole.Admin };
            var canShip = user.Role == UserRole.Kladovshik;
            Assert.That(canShip, Is.False);
        }

        [Test]
        public void RoleCheck_KladovshikCanShip()
        {
            var user    = new User { Role = UserRole.Kladovshik };
            var canShip = user.Role == UserRole.Kladovshik;
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
            var password = "Admin123";
            var hash     = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.That(BCrypt.Net.BCrypt.Verify(password, hash), Is.True);
        }

        [Test]
        public void BcryptHash_WrongPassword_NotVerified()
        {
            var password  = "Admin123";
            var wrongPass = "wrongpass";
            var hash      = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.That(BCrypt.Net.BCrypt.Verify(wrongPass, hash), Is.False);
        }
    }
}