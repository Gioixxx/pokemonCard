using PokemonCardManager.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonCardManager.Services
{
    public enum OperationType
    {
        AddCard,
        UpdateCard,
        DeleteCard,
        AddSale,
        UpdateSale,
        DeleteSale
    }

    public class Operation
    {
        public OperationType Type { get; set; }
        public object? Entity { get; set; }
        public object? PreviousState { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public interface IUndoRedoService
    {
        bool CanUndo { get; }
        bool CanRedo { get; }
        void RecordOperation(OperationType type, object entity, object? previousState = null);
        void Undo();
        void Redo();
        void Clear();
        string? GetUndoDescription();
        string? GetRedoDescription();
    }

    public class UndoRedoService : IUndoRedoService
    {
        private readonly Stack<Operation> _undoStack = new();
        private readonly Stack<Operation> _redoStack = new();
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public UndoRedoService(ILogger logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void RecordOperation(OperationType type, object entity, object? previousState = null)
        {
            var operation = new Operation
            {
                Type = type,
                Entity = entity,
                PreviousState = previousState,
                Timestamp = DateTime.UtcNow
            };

            _undoStack.Push(operation);
            _redoStack.Clear(); // Clear redo stack when new operation is recorded
            _logger.LogDebug($"Recorded operation: {type} at {operation.Timestamp}");
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            var operation = _undoStack.Pop();
            _logger.LogInformation($"Undoing operation: {operation.Type}");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cardService = scope.ServiceProvider.GetRequiredService<ICardService>();
                var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();

                switch (operation.Type)
                {
                    case OperationType.AddCard:
                        if (operation.Entity is Card card)
                        {
                            cardService.DeleteCardAsync(card.Id).Wait();
                        }
                        break;

                    case OperationType.UpdateCard:
                        if (operation.PreviousState is Card previousCard)
                        {
                            cardService.UpdateCardAsync(previousCard).Wait();
                        }
                        break;

                    case OperationType.DeleteCard:
                        if (operation.Entity is Card deletedCard)
                        {
                            cardService.AddCardAsync(deletedCard).Wait();
                        }
                        break;

                    case OperationType.AddSale:
                        if (operation.Entity is Sale sale)
                        {
                            saleService.DeleteSaleAsync(sale.Id).Wait();
                        }
                        break;

                    case OperationType.UpdateSale:
                        if (operation.PreviousState is Sale previousSale)
                        {
                            saleService.UpdateSaleAsync(previousSale).Wait();
                        }
                        break;

                    case OperationType.DeleteSale:
                        if (operation.Entity is Sale deletedSale)
                        {
                            saleService.AddSaleAsync(deletedSale).Wait();
                        }
                        break;
                }

                _redoStack.Push(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error undoing operation: {ex.Message}", ex);
                // Push back to undo stack if operation failed
                _undoStack.Push(operation);
                throw;
            }
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var operation = _redoStack.Pop();
            _logger.LogInformation($"Redoing operation: {operation.Type}");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cardService = scope.ServiceProvider.GetRequiredService<ICardService>();
                var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();

                switch (operation.Type)
                {
                    case OperationType.AddCard:
                        if (operation.Entity is Card card)
                        {
                            cardService.AddCardAsync(card).Wait();
                        }
                        break;

                    case OperationType.UpdateCard:
                        if (operation.Entity is Card updatedCard)
                        {
                            cardService.UpdateCardAsync(updatedCard).Wait();
                        }
                        break;

                    case OperationType.DeleteCard:
                        if (operation.Entity is Card cardToDelete)
                        {
                            cardService.DeleteCardAsync(cardToDelete.Id).Wait();
                        }
                        break;

                    case OperationType.AddSale:
                        if (operation.Entity is Sale sale)
                        {
                            saleService.AddSaleAsync(sale).Wait();
                        }
                        break;

                    case OperationType.UpdateSale:
                        if (operation.Entity is Sale updatedSale)
                        {
                            saleService.UpdateSaleAsync(updatedSale).Wait();
                        }
                        break;

                    case OperationType.DeleteSale:
                        if (operation.Entity is Sale saleToDelete)
                        {
                            saleService.DeleteSaleAsync(saleToDelete.Id).Wait();
                        }
                        break;
                }

                _undoStack.Push(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error redoing operation: {ex.Message}", ex);
                // Push back to redo stack if operation failed
                _redoStack.Push(operation);
                throw;
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _logger.LogDebug("Undo/Redo stacks cleared");
        }

        public string? GetUndoDescription()
        {
            if (!CanUndo)
                return null;

            var operation = _undoStack.Peek();
            return GetOperationDescription(operation);
        }

        public string? GetRedoDescription()
        {
            if (!CanRedo)
                return null;

            var operation = _redoStack.Peek();
            return GetOperationDescription(operation);
        }

        private string GetOperationDescription(Operation operation)
        {
            return operation.Type switch
            {
                OperationType.AddCard => $"Aggiungi carta: {((Card)operation.Entity).Name}",
                OperationType.UpdateCard => $"Modifica carta: {((Card)operation.Entity).Name}",
                OperationType.DeleteCard => $"Elimina carta: {((Card)operation.Entity).Name}",
                OperationType.AddSale => $"Aggiungi vendita: {((Sale)operation.Entity).Card?.Name}",
                OperationType.UpdateSale => $"Modifica vendita: {((Sale)operation.Entity).Card?.Name}",
                OperationType.DeleteSale => $"Elimina vendita: {((Sale)operation.Entity).Card?.Name}",
                _ => operation.Type.ToString()
            };
        }
    }
}

