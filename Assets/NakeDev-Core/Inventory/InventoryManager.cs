using System;
using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.InventorySystem
{
    /// <summary>
    /// Gerencia os itens do jogador apenas por dados (IDs e Quantidades).
    /// Segue a regra KISS: sem UI acoplada, sem lógica visual pesada.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        // Dicionário mantendo o ID do item e a sua respectiva quantidade.
        private Dictionary<string, int> _items = new Dictionary<string, int>();

#if UNITY_EDITOR
        [Header("Debug View")]
        [Tooltip("Lista apenas para visualização no Inspector (não é usada na lógica).")]
        [SerializeField] private List<string> _debugInventoryViewer = new List<string>();
#endif

        /// <summary>
        /// Evento disparado sempre que um item for adicionado ou removido.
        /// Útil se quisermos conectar uma UI futura sem acoplar diretamente.
        /// Retorna (itemID, novaQuantidade).
        /// </summary>
        public event Action<string, int> OnInventoryChanged;

        // --- Escuta Automática (Regra 4) ---
        private void OnEnable()
        {
            // O Inventário "ouve" quando qualquer item no mundo grita que foi pego
            InventoryEvents.OnAnyItemCollected += AddItem;
        }

        private void OnDisable()
        {
            InventoryEvents.OnAnyItemCollected -= AddItem;
        }

        /// <summary>
        /// Adiciona uma quantidade específica de um item ao inventário.
        /// </summary>
        public void AddItem(string itemID, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemID) || amount <= 0) return;

            if (_items.ContainsKey(itemID))
            {
                _items[itemID] += amount;
            }
            else
            {
                _items.Add(itemID, amount);
            }

            OnInventoryChanged?.Invoke(itemID, _items[itemID]);
            Debug.Log($"[Inventory] Adicionado {amount}x '{itemID}'. Total: {_items[itemID]}");
            UpdateDebugViewer();
        }

        /// <summary>
        /// Remove uma quantidade específica de um item. Retorna true se houver sucesso.
        /// </summary>
        public bool RemoveItem(string itemID, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemID) || amount <= 0) return false;

            if (_items.ContainsKey(itemID) && _items[itemID] >= amount)
            {
                _items[itemID] -= amount;

                // Se a quantidade chegou a zero, limpamos a chave para manter o dicionário enxuto.
                if (_items[itemID] == 0)
                {
                    _items.Remove(itemID);
                    OnInventoryChanged?.Invoke(itemID, 0);
                    Debug.Log($"[Inventory] '{itemID}' foi completamente removido.");
                }
                else
                {
                    OnInventoryChanged?.Invoke(itemID, _items[itemID]);
                    Debug.Log($"[Inventory] Removido {amount}x '{itemID}'. Restante: {_items[itemID]}");
                }
                
                UpdateDebugViewer();
                return true;
            }

            Debug.LogWarning($"[Inventory] Falha ao remover {amount}x '{itemID}'. Quantidade insuficiente.");
            return false;
        }

        /// <summary>
        /// Retorna a quantidade de um item específico que o jogador possui.
        /// </summary>
        public int GetItemQuantity(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) return 0;

            if (_items.TryGetValue(itemID, out int quantity))
            {
                return quantity;
            }
            
            return 0;
        }

        /// <summary>
        /// Checagem rápida para ver se o jogador tem pelo menos 1 unidade do item.
        /// </summary>
        public bool HasItem(string itemID)
        {
            return GetItemQuantity(itemID) > 0;
        }

#if UNITY_EDITOR
        private void UpdateDebugViewer()
        {
            _debugInventoryViewer.Clear();
            foreach (var kvp in _items)
            {
                _debugInventoryViewer.Add($"ID: {kvp.Key} | Quantidade: {kvp.Value}");
            }
        }
#else
        private void UpdateDebugViewer() {}
#endif
    }
}
