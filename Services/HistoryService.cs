using System;
using System.Collections.Generic;
using System.Linq;
using GreenStock.Data;
using GreenStock.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GreenStock.Services;

/// <summary>
/// Сервис для работы с историей отгрузок.
/// Предоставляет методы получения данных для HistoryForm.
/// </summary>
public static class HistoryService
{
    private static readonly ILogger _log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// DTO для отображения данных отгрузки в истории.
    /// </summary>
    public class ShipmentHistoryDto
    {
        public Guid Id { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// DTO для отображения позиции отгрузки.
    /// </summary>
    public class ShipmentItemDto
    {
        public string Article { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Получает список всех отгрузок с расчётом суммы по каждой.
    /// </summary>
    public static List<ShipmentHistoryDto> GetAllShipments()
    {
        try
        {
            using var db = new AppDbContext();

            var shipments = db.Shipments
                .Include(s => s.CreatedByUser)
                .Include(s => s.Items)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            var result = shipments.Select(s => new ShipmentHistoryDto
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                CreatedBy = s.CreatedByUser?.Login ?? "—",
                Recipient = s.Recipient,
                ItemCount = s.Items.Count,
                TotalAmount = s.Items.Sum(si => (decimal)si.Quantity * si.Product.PurchasePrice)
            }).ToList();

            _log.Debug("Получено {0} отгрузок", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка при получении отгрузок");
            throw;
        }
    }

    /// <summary>
    /// Получает позиции заданной отгрузки.
    /// </summary>
    public static List<ShipmentItemDto> GetShipmentItems(Guid shipmentId)
    {
        try
        {
            using var db = new AppDbContext();

            var shipment = db.Shipments
                .Include(s => s.Items)
                    .ThenInclude(si => si.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefault(s => s.Id == shipmentId);

            if (shipment == null)
            {
                _log.Warn("Отгрузка не найдена: {0}", shipmentId);
                return new List<ShipmentItemDto>();
            }

            var result = shipment.Items.Select(si => new ShipmentItemDto
            {
                Article = si.Product.Article,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                Unit = si.Product.Unit,
                Price = si.Product.PurchasePrice,
                Total = (decimal)si.Quantity * si.Product.PurchasePrice
            }).ToList();

            _log.Debug("Получено {0} позиций для отгрузки {1}", result.Count, shipmentId);
            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка при получении позиций отгрузки {0}", shipmentId);
            throw;
        }
    }
}
