﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public static class ObjectHolder
    {
        private static readonly Dictionary<Type, object> _objects = new Dictionary<Type, object>();

        public static void AddObject(object obj, params Type[] customTypes)
        {
            _objects.Add(obj.GetType(), obj);

            foreach (Type type in customTypes)
                _objects.Add(type, obj);
        }

        public static T GetObject<T>()
        {
            Type type = typeof(T);
            _objects.TryGetValue(type, out object obj);

            if (obj != null) return (T)obj;
            
            Debug.LogError($"Object not found {type}");
            return default(T);
        }
    }
}