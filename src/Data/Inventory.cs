﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Inventory
    {
        #region Fields
        List<Item> m_Items;
        int m_MaxCapacity;
        #endregion

        #region Properties
        public IEnumerable<Item> Items
        {
            get { return m_Items; }
        }

        public int CountItems
        {
            get { return m_Items.Count; }
        }

        public Item this[int index]
        {
            get 
            {
                if (index < 0 || index >= m_Items.Count)
                    return null;

                return m_Items[index];
            }
        }

        public int MaxCapacity
        {
            get { return m_MaxCapacity; }
            set { m_MaxCapacity = value; }
        }

        public bool IsEmpty
        {
            get { return m_Items.Count == 0; }
        }

        public bool IsFull
        {
            get { return m_Items.Count >= m_MaxCapacity; }
        }

        public Item TopItem
        {
            get
            {
                if (m_Items.Count == 0)
                    return null;
                return m_Items[m_Items.Count - 1];
            }
        }

        public Item BottomItem
        {
            get
            {
                if (m_Items.Count == 0)
                    return null;
                return m_Items[0];
            }
        }
        #endregion

        #region Init
        public Inventory(int maxCapacity)
        {
            if (maxCapacity < 0)
                throw new ArgumentOutOfRangeException("maxCapacity < 0");

            m_MaxCapacity = maxCapacity;
            //m_Items = new List<Item>(maxCapacity);
            m_Items = new List<Item>(1);
        }
        #endregion

        #region Managing
        /// <summary>
        /// Try to add all the item quantity.
        /// </summary>
        /// <param name="it"></param>
        /// <returns></returns>
        public bool AddAll(Item it)
        {
            if (it == null)
                throw new ArgumentNullException("it");

            // Try stacking.
            int stackedQuantity;
            List<Item> stackList = GetItemsStackableWith(it, out stackedQuantity);

            // If can stack all, do it and success.
            if (stackedQuantity == it.Quantity)
            {
                int quantityLeft = it.Quantity;
                foreach (Item other in stackList)
                {
                    int canStackOther = other.Model.StackingLimit - other.Quantity;
                    int stackOther = Math.Min(canStackOther, quantityLeft);
                    AddToStack(it, stackOther, other);
                    quantityLeft -= stackOther;
                    if (quantityLeft <= 0)
                        break;
                }
                return true;
            }

            // We cannot stack all.
            // If inventory full, fail.
            if (IsFull)
                return false;

            // One slot free, add.
            m_Items.Add(it);
            return true;
        }

        /// <summary>
        /// Try to add as much item quantity as possible.
        /// Changes item quantity.
        /// </summary>
        /// <returns></returns>
        public bool AddAsMuchAsPossible(Item it, out int quantityAdded)
        {
            if (it == null)
                throw new ArgumentNullException("it");

            int startQuantity = it.Quantity;

            // Try stacking.
            int stackedQuantity;
            List<Item> stackList = GetItemsStackableWith(it, out stackedQuantity);

            if (stackList != null)
            {
                // Stack as much as possible.
                quantityAdded = 0;
                foreach (Item other in stackList)
                {
                    int added = AddToStack(it, it.Quantity - quantityAdded, other);
                    quantityAdded += added;
                }
                // If quantity left, try to add rest in free slot.
                if (quantityAdded < it.Quantity)
                {
                    // we could stack this much.
                    it.Quantity -= quantityAdded;
                    // if slot free, add rest to free slot.
                    if (!IsFull)
                    {
                        // we stacked or added evertyhing.
                        m_Items.Add(it);
                        quantityAdded = startQuantity;
                    }
                }
                else
                {
                    // we stacked everything, item should die.
                    it.Quantity = 0;
                }

                // Done.
                return true;
            }

            // Cannot stack, need a free slot.
            if (IsFull)
            {
                quantityAdded = 0;
                return false;
            }

            // Add to free slot.
            quantityAdded = it.Quantity;
            m_Items.Add(it);
            return true;
        }

        /// <summary>
        /// Checks if can add at least one quantity of item.
        /// </summary>
        /// <param name="it"></param>
        /// <returns></returns>
        public bool CanAddAtLeastOne(Item it)
        {
            if (it == null)
                throw new ArgumentNullException("it");

            // If free slot, sure!
            if (!IsFull)
                return true;

            // Inventory full.
            // Can we stack at least one?
            int stackedQuantity;
            return GetItemsStackableWith(it, out stackedQuantity) != null;
        }

#if false
        obsolete
        public bool AddAllQuantity(Item it)
        {
            short q = it.Quantity;

            do
            {
                Add(it);
                --q;
            }
            while (q > 0 && !Contains(it));

            return true;
        }
#endif

#if false
        /// <summary>
        /// Remove one quantity of item.
        /// </summary>
        /// <param name="it"></param>
        public void Remove(Item it)
        {
            if (it == null)
                throw new ArgumentNullException("it");
            if (!m_Items.Contains(it))
                return;

            // Try destacking first.
            Item destackFrom = GetBestDestackable(it);
            if (destackFrom != null && destackFrom.Quantity > 1)
            {
                --destackFrom.Quantity;
                if (destackFrom.Quantity <= 0)
                    m_Items.Remove(destackFrom);
                return;
            }

            // Cant destack, remove it.
            m_Items.Remove(it);
        }
#endif

        /// <summary>
        /// Remove completly the item (all its quantity).
        /// </summary>
        /// <param name="it"></param>
        public void RemoveAllQuantity(Item it)
        {
            m_Items.Remove(it);
        }

        /// <summary>
        /// Decrease quantity by 1, if no more quantity remove item.
        /// </summary>
        /// <param name="it"></param>
        public void Consume(Item it)
        {
            if (--it.Quantity <= 0)
                m_Items.Remove(it);
        }

        int AddToStack(Item from, int addThis, Item to)
        {
            int added = 0;
            while (addThis > 0 && to.Quantity < to.Model.StackingLimit)
            {
                ++to.Quantity;
                ++added;
                --addThis;
            }
            return added;
        }

        /// <summary>
        /// Gets items in inventory with which item can be stacked with and quantity of item stacked this way.
        /// </summary>
        /// <param name="it"></param>
        /// <param name="stackedQuantity">it.Quantity if fully stacked otherwise quantity that fit.</param>
        /// <returns>list of items to be stacked with; null if it not stackable or no stack found.</returns>
        List<Item> GetItemsStackableWith(Item it, out int stackedQuantity)
        {
            stackedQuantity = 0;

            if (!it.Model.IsStackable)
                return null;

            List<Item> stackList = null;
            foreach (Item other in m_Items)
            {
                if (other.Model == it.Model &&  // other same model
                    other.CanStackMore &&       // other not full
                    !other.IsEquipped)          // other not equiped
                {
                    // can stack with this one.
                    if (stackList == null) stackList = new List<Item>(m_Items.Count);
                    stackList.Add(other);

                    // how many can we stack there?
                    int stackOnOther = other.Model.StackingLimit - other.Quantity;

                    // how many do we want to stack here.
                    int wantToStack = Math.Min(it.Quantity - stackedQuantity, stackOnOther);

                    // we stack this much here.
                    stackedQuantity += wantToStack;

                    // if enough, done.
                    if (stackedQuantity == it.Quantity)
                        break;
                }
            }

            return stackList;
        }

        /// <summary>
        /// Gets smallest stack this item can be destacked from.
        /// </summary>
        /// <param name="it"></param>
        /// <returns></returns>
        Item GetBestDestackable(Item it)
        {
            if (!it.Model.IsStackable)
                return null;

            Item smallestStack = null;
            foreach(Item other in m_Items)
                if (other.Model == it.Model)
                {
                    if (smallestStack == null || other.Quantity < smallestStack.Quantity)
                        smallestStack = other;
                }

            return smallestStack;
        }

        public bool Contains(Item it)
        {
            return m_Items.Contains(it);
        }

        public Item GetFirstByModel(ItemModel model)
        {
            foreach (Item it in m_Items)
                if (it.Model == model)
                    return it;
            return null;
        }

        public bool HasItemOfType(Type tt)
        {
            return GetFirstByType(tt) != null;
        }

        public Item GetFirstByType(Type tt)
        {
            foreach (Item it in m_Items)
                if (it.GetType() == tt)
                    return it;
            return null;
        }

        public List<_T_> GetItemsByType<_T_>() where _T_ : Item
        {
            List<_T_> list = null;
            Type tt = typeof(_T_);

            foreach (Item it in m_Items)
                if (it.GetType() == tt)
                {
                    if (list == null)
                        list = new List<_T_>(m_Items.Count);
                    list.Add(it as _T_);
                }

            return list;
        }

        public Item GetFirstMatching(Predicate<Item> fn)
        {
            foreach (Item it in m_Items)
                if (fn(it))
                    return it;
            return null;
        }
        #endregion

        #region Helpers
        public int CountItemsMatching(Predicate<Item> fn)
        {
            int count = 0;
            foreach (Item it in m_Items)
                if (fn(it)) ++count;
            return count;
        }

        public bool HasItemMatching(Predicate<Item> fn)
        {
            foreach (Item it in m_Items)
                if (fn(it)) return true;
            return false;
        }
        #endregion
    }
}
