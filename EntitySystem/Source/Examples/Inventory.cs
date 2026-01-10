using System;
using System.Collections.Generic;
using UnityEngine;



public namespace EntitySystem.Examples
{
    [Serializable]
    public class Inventory
    {
        public int capacity = 24;
        public List<Slot> slots = new List<Slot>();

        // Toolbar: z.B. 10 Plätze
        public ToolbarConnection[] toolbarConnections = GetToolbarConnections(10);

        /// <summary>
        /// Sorgt dafür, dass die Liste genau 'capacity' Slots hat.
        /// Slot-Objekte nie null, nur slot.stack darf null sein.
        /// </summary>
        public void EnsureCapacity()
        {
            while (slots.Count < capacity)
                slots.Add(new Slot());      // leere Slot-Objekte

            if (slots.Count > capacity)
                slots.RemoveRange(capacity, slots.Count - capacity);
        }

        public bool Contains(string _entityTypeId)
        {       
            EnsureCapacity();
            for (int i = 0; i < slots.Count; i++)
            {
                var stack = slots[i].stack;
                if (stack != null && stack.entityGhost.entityTypeId == _entityTypeId)
                    return true;
            }
            return false;
        }

        public void AddItem(EntityGhost _entityGhost)
        {
            if (_entityGhost.TryGet(out EntityComponent_Stackable _stackable))
                Debug.Log($"AddItem: {_entityGhost.entityTypeId}, quantity: {_stackable.quantity}");
            EnsureCapacity();

            // 1) bestehenden Stack mit gleichem entityTypeId finden und checken ob dieser gestackt werden kann / ob noch platz im stack ist
            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i].stack;
                if (s != null && s.entityGhost.entityTypeId == _entityGhost.entityTypeId)
                {
                    if (s.entityGhost.TryGet(out EntityComponent_Stackable stackable))
                    {
                        if (_stackable.quantity + stackable.quantity < stackable.maxQuantity)
                        {
                            stackable.quantity += _stackable.quantity;
                            Debug.Log($"AddItem: {_entityGhost.entityTypeId} x {_stackable.quantity} to stack with index: {i}, -> new quantity: {stackable.quantity}/{stackable.maxQuantity}");                 
                            return; 
                        }                                               
                    }
                }
            }

            // 2) freien Slot (stack == null) suchen
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                    slots[i] = new Slot();

                if (slots[i].stack == null)
                {
                    slots[i].stack = new Stack(_entityGhost);
                    return;
                }
            }

            // 3) kein Platz
            Debug.LogWarning("Inventory full – AddItem failed.");
        }

        public int RemoveItem(int index, int quantity)
        {
            EnsureCapacity();
            if (index < 0 || index >= slots.Count) return -1;

            var stack = slots[index].stack;
            if (stack == null) return -1;

            // Try get the EntityComponent_Stackable from the entityGhost.
            // If there is one -> get the current stack size
            // If there isn't one -> handle the item to remove from as it is a single item (non-stackable)
            if (stack.entityGhost.TryGet(out EntityComponent_Stackable stackable))
            {
                if (stackable.quantity > quantity)
                {
                    stackable.quantity -= quantity;
                    return quantity;
                }
                else
                {
                    int removed = stackable.quantity;
                    slots[index].stack = null;
                    return removed;
                }
            }

            return 0;
        }

        public bool RemoveFullStack(int index)
        {
            EnsureCapacity();
            if (index < 0 || index >= slots.Count) return false;

            var stack = slots[index].stack;
            if (stack == null) return false;

            slots[index].stack = null;
            return true;
        }


        /// <summary>
        /// Move/Merge/Swap zwischen zwei Slots (auch zwischen zwei Inventaren).
        /// </summary>
        public static bool TryMoveOrMerge(
            Inventory fromInv, int fromIndex,
            Inventory toInv,   int toIndex)
        {
            if (fromInv == null || toInv == null) return false;

            fromInv.EnsureCapacity();
            toInv.EnsureCapacity();

            if (fromIndex < 0 || fromIndex >= fromInv.slots.Count) return false;
            if (toIndex   < 0 || toIndex   >= toInv.slots.Count)   return false;

            if (fromInv.slots[fromIndex] == null)
                fromInv.slots[fromIndex] = new Slot();
            if (toInv.slots[toIndex] == null)
                toInv.slots[toIndex] = new Slot();

            var fromStack = fromInv.slots[fromIndex].stack;
            var toStack   = toInv.slots[toIndex].stack;

            // nix zu bewegen
            if (fromStack == null) return false;

            // Ziel leer → kompletten Stack verschieben
            if (toStack == null)
            {
                toInv.slots[toIndex].stack     = fromStack;
                fromInv.slots[fromIndex].stack = null;
                return true;
            }

            // Versuchen zu mergen, wenn beide stackable sind & gleicher Typ
            bool fromHasStackable = fromStack.entityGhost.TryGet(out EntityComponent_Stackable fromStackable);
            bool toHasStackable   = toStack.entityGhost.TryGet(out EntityComponent_Stackable toStackable);

            if (fromHasStackable && toHasStackable &&
                fromStack.entityGhost.entityTypeId == toStack.entityGhost.entityTypeId)
            {
                int space = toStackable.maxQuantity - toStackable.quantity;
                if (space <= 0)
                {
                    // voll -> tauschen
                }
                else
                {
                    int move = Mathf.Min(space, fromStackable.quantity);
                    toStackable.quantity   += move;
                    fromStackable.quantity -= move;

                    if (fromStackable.quantity <= 0)
                        fromInv.slots[fromIndex].stack = null;

                    return true;
                }
            }

            // Default: einfach Stacks tauschen (auch für nicht-stackbare Items)
            fromInv.slots[fromIndex].stack = toStack;
            toInv.slots[toIndex].stack     = fromStack;
            return true;
        }

        public bool TryDropInto3DWorld(Vector3 position, int index)
        {
            EnsureCapacity();
            if (index < 0 || index >= slots.Count) { Debug.LogWarning("[Inventory]: Could not drop the item because... "); return false;}

            var stack = slots[index].stack;
            if (stack == null || stack.entityGhost == null) { Debug.LogWarning("[Inventory]: Could not drop the item because... "); return false;}

            var ghost = stack.entityGhost;

            if (RemoveFullStack(index))
            {
                // Here you could spawn the item into the 3D world
            }

            Debug.LogWarning("[Inventory]: Could not drop the item because... "); 
            return false;
        }

        // ---------------- Toolbar ----------------

        [System.Serializable]
        public class ToolbarConnection
        {
            // Index des Inventar-Slots, -1 = leer
            public int inventoryIndex;
            // Fixer Index in der Toolbar (0..N-1)
            public int toolbarIndex;

            public void Bind(int inventoryIndex)
            {
                this.inventoryIndex = inventoryIndex;
            }

            public void Unbind()
            {
                this.inventoryIndex = -1;
            }

            public int GetInventoryIndex() => inventoryIndex;
            public int GetToolbarIndex()   => toolbarIndex;

            public ToolbarConnection(int inventoryIndex, int toolbarIndex)
            {
                this.inventoryIndex = inventoryIndex;
                this.toolbarIndex   = toolbarIndex;
            }
        }

        public static ToolbarConnection[] GetToolbarConnections(int count)
        {
            ToolbarConnection[] connections = new ToolbarConnection[count];
            for (int i = 0; i < count; i++)
            {
                connections[i] = new ToolbarConnection(-1, i);
            }
            return connections;
        }
        
        public void BindToolbarConnection(int toolbarIndex, int inventoryIndex) { EnsureCapacity(); if (inventoryIndex < 0 || inventoryIndex >= slots.Count) return; toolbarConnections[toolbarIndex].Bind(inventoryIndex); } 
        public void UnbindToolbarConnection(int toolbarIndex) { toolbarConnections[toolbarIndex].Unbind(); }
        public int GetInventoryIndexByToolbarIndex(int toolbarIndex) => toolbarConnections[toolbarIndex].GetInventoryIndex();
    }

    [Serializable]
    public class Slot 
    { 
        public Stack stack; 
    }

    [Serializable]
    public class Stack
    {
        /// <summary>
        /// EntityGhost holds all relevant information about the item (entity), for example stacksize, spoiltime, durability, etc.
        /// </summary>
        public EntityGhost entityGhost;

        public Stack(EntityGhost _entityGhost)
        {
            this.entityGhost   = _entityGhost;
        }
    }
}