using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    public int capacity = 24;
    public List<Slot> slots = new List<Slot>();

    /// <summary>
    /// Ensures that "slots" always has 'capacity' slots
    /// </summary>
    public void EnsureCapacity()
    {
        while (slots.Count < capacity)
            slots.Add(new Slot());      // empty slot-objects

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
            Debug.Log($"AddItem: {_entityGhost.entityTypeId}, quantity: {_stackable.GetQuantity()}");
        EnsureCapacity();

        // 1) search for slot wich same entityTypeID and try to stack
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i].stack;
            if (s != null && s.entityGhost.entityTypeId == _entityGhost.entityTypeId)
            {
                if (s.entityGhost.TryGet(out EntityComponent_Stackable stackable))
                {
                    if (_stackable.GetQuantity() + stackable.GetQuantity() < stackable.GetMaxQuantity())
                    {
                        stackable.Add(_stackable.GetQuantity());
                        Debug.Log($"AddItem: {_entityGhost.entityTypeId} x {_stackable.GetQuantity()} to stack with index: {i}, -> new quantity: {stackable.GetQuantity()}/{stackable.GetMaxQuantity()}");                 
                        return; 
                    }                                               
                }
            }
        }

        // 2) search for free slot
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

        // 3) No slot
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
            if (stackable.GetQuantity() > quantity)
            {
                stackable.Remove(quantity);
                return quantity;
            }
            else
            {
                int removed = stackable.GetQuantity();
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
    /// Move/Merge/Swap between two slots.
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

        // nothing to move
        if (fromStack == null) return false;

        // Empty target -> move entire slot
        if (toStack == null)
        {
            toInv.slots[toIndex].stack     = fromStack;
            fromInv.slots[fromIndex].stack = null;
            return true;
        }

        // Try to merge
        bool fromHasStackable = fromStack.entityGhost.TryGet(out EntityComponent_Stackable fromStackable);
        bool toHasStackable   = toStack.entityGhost.TryGet(out EntityComponent_Stackable toStackable);

        if (fromHasStackable && toHasStackable &&
            fromStack.entityGhost.entityTypeId == toStack.entityGhost.entityTypeId)
        {
            int space = toStackable.GetMaxQuantity() - toStackable.GetQuantity();
            if (space <= 0)
            {
                // full -> swap
            }
            else
            {
                int move = Mathf.Min(space, fromStackable.GetQuantity());
                toStackable.Add(move);
                fromStackable.Remove(move);

                if (fromStackable.GetQuantity() <= 0)
                    fromInv.slots[fromIndex].stack = null;

                return true;
            }
        }

        // Default: swap slots
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

        Debug.LogWarning("[Inventory]: Could not drop the item"); 
        return false;
    }
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