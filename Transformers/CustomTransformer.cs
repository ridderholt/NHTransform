using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NHibernate.Transform;

namespace NHTransform.Transformers
{
    public class CustomTransformer<T> : IResultTransformer
    {
        private readonly Func<T, object> _group;

        public CustomTransformer()
        {
            _group = null;
        }

        public CustomTransformer(Func<T, object> groupBy)
        {
            _group = groupBy;
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var obj = CreateInstance();
            var type = obj.GetType();

            for (var i = 0; i < tuple.Length; i++)
            {
                var value = tuple[i];
                var propertyName = aliases[i];

                if(value == null) continue;

                if (IsRelation(propertyName))
                {
                    var subName = GetPropertyName(propertyName);
                    object subObj = null;
                    var property = type.GetProperty(subName.PropertyType);

                    if (IsPropertyNull(property, obj))
                    {
                        subObj = CreateInstance(property.PropertyType);
                    }
                    else
                    {
                        subObj = property.GetValue(obj);
                    }

                    var subType = subObj.GetType();
                    var info = subType.GetProperty(subName.PropertyName);

                    if (IsList(property))
                    {
                        var first = subType.GenericTypeArguments.First();
                        var listItem = CreateInstance(first);
                        var propertyInfo = listItem.GetType().GetProperty(subName.PropertyName);
                        SetValue(propertyInfo, listItem, value);
                        ((IList) subObj).Add(listItem);
                    }
                    else
                    {
                        SetValue(info, subObj, value);
                    }

                    SetValue(property, obj, subObj);
                }
                else
                {
                    var propertyInfo = type.GetProperty(propertyName);
                    propertyInfo.SetValue(obj, value);
                }
            }

            return obj;
        }

        private static bool IsList(PropertyInfo property)
        {
            return property.PropertyType.GetInterface("IEnumerable") != null;
        }

        private static bool IsPropertyNull(PropertyInfo info, T obj)
        {
            return info.GetValue(obj) == null;
        }

        private static SubPropertyInfo GetPropertyName(string propertyName)
        {
            var values = propertyName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

            return new SubPropertyInfo
            {
                PropertyName = values[1],
                PropertyType = values[0]
            };
        }

        private static bool IsRelation(string propertyName)
        {
            return propertyName.Contains(".");
        }

        private static void SetValue(PropertyInfo info, object target, object value)
        {
            info.SetValue(target, value);
        }

        public IList TransformList(IList list)
        {
            //if (_group == null) return collection;

            //var list = new List<T>();
            //list.AddRange((IEnumerable<T>) collection);

            //var groupBy = list.GroupBy(_group);

            //return collection;
            var result = (IList)Activator.CreateInstance(list.GetType());
            var distinct = new HashSet<Identity>();

            for (int i = 0; i < list.Count; i++)
            {
                object entity = list[i];
                if (distinct.Add(new Identity(entity)))
                {
                    result.Add(entity);
                }
            }

            return result;
        }

        private T CreateInstance()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
        private object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }


        internal sealed class Identity
        {
            internal readonly object entity;

            internal Identity(object entity)
            {
                this.entity = entity;
            }

            public override bool Equals(object other)
            {
                var that = (Identity)other;
                return ReferenceEquals(entity, that.entity);
            }

            public override int GetHashCode()
            {
                return RuntimeHelpers.GetHashCode(entity);
            }
        }
    }

    internal class SubPropertyInfo
    {
        public string PropertyType { get; set; }
        public string PropertyName { get; set; }
    }

    public static class SQLTransformer
    {
        public static CustomTransformer<T> For<T>()
        {
            return new CustomTransformer<T>();
        }
    }
}