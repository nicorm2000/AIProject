using System;
using System.Collections.Generic;

namespace ECS.Patron
{
    public class ECSEntity
    {
        private readonly List<Type> componentsType;
        private readonly List<Type> flagType;

        private readonly uint ID;

        public ECSEntity()
        {
            ID = EntityID.GetNew();
            componentsType = new List<Type>();
            flagType = new List<Type>();
        }

        public uint GetID()
        {
            return ID;
        }

        public void Dispose()
        {
            componentsType.Clear();
            flagType.Clear();
        }

        public void AddComponentType<ComponentType>() where ComponentType : ECSComponent
        {
            AddComponentType(typeof(ComponentType));
        }

        public void AddComponentType(Type ComponentType)
        {
            componentsType.Add(ComponentType);
        }

        public bool ContainsComponentType<ComponentType>() where ComponentType : ECSComponent
        {
            return ContainsComponentType(typeof(ComponentType));
        }

        public bool ContainsComponentType(Type ComponentType)
        {
            return componentsType.Contains(ComponentType);
        }

        public void RemoveComponentType<ComponentType>() where ComponentType : ECSComponent
        {
            componentsType.Remove(typeof(ComponentType));
        }

        public void RemoveComponentType(Type ComponentType)
        {
            componentsType.Remove(ComponentType);
        }

        public void AddFlagType<FlagType>() where FlagType : ECSFlag
        {
            AddFlagType(typeof(FlagType));
        }

        public void AddFlagType(Type FlagType)
        {
            flagType.Add(FlagType);
        }

        public bool ContainsFlagType<FlagType>() where FlagType : ECSFlag
        {
            return ContainsFlagType(typeof(FlagType));
        }

        public bool ContainsFlagType(Type FlagType)
        {
            return flagType.Contains(FlagType);
        }

        public void RemoveFlagType<FlagType>() where FlagType : ECSFlag
        {
            flagType.Remove(typeof(FlagType));
        }

        public void RemoveFlagType(Type FlagType)
        {
            flagType.Remove(FlagType);
        }

        private class EntityID
        {
            private static uint LastEntityID;

            internal static uint GetNew()
            {
                return LastEntityID++;
            }
        }
    }
}